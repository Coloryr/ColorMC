using System.Buffers;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Downloader;

/// <summary>
/// 下载线程
/// </summary>
internal class DownloadThread
{
    private const int DefaultCopyBufferSize = 81920;

    /// <summary>
    /// 计算buffer大小
    /// </summary>
    /// <param name="stream">流</param>
    /// <returns>大小</returns>
    private static int GetCopyBufferSize(Stream stream)
    {
        int bufferSize = DefaultCopyBufferSize;

        if (stream.CanSeek)
        {
            long length = stream.Length;
            long position = stream.Position;
            if (length <= position)
            {
                bufferSize = 1;
            }
            else
            {
                long remaining = length - position;
                if (remaining > 0)
                {
                    bufferSize = (int)Math.Min(bufferSize, remaining);
                }
            }
        }

        return bufferSize;
    }

    /// <summary>
    /// 线程
    /// </summary>
    private readonly Thread _thread;
    /// <summary>
    /// 启动信号量
    /// </summary>
    private readonly Semaphore _semaphoreWait = new(0, 2);
    /// <summary>
    /// 暂停信号量
    /// </summary>
    private readonly Semaphore _semaphorePause = new(0, 2);
    /// <summary>
    /// 是否在暂停
    /// </summary>
    private bool _pause;
    /// <summary>
    /// 是否在运行
    /// </summary>
    private bool _run;
    /// <summary>
    /// 是否在等待中
    /// </summary>
    private bool _isWait;

    /// <summary>
    /// 线程号
    /// </summary>
    private readonly int _index;

    /// <summary>
    /// 初始化下载器
    /// </summary>
    /// <param name="index">线程号</param>
    public DownloadThread(int index)
    {
        _index = index;
        _run = true;
        _thread = new Thread(() =>
        {
            while (_run)
            {
                Run();
            }
        })
        {
            Name = "ColorMC Download Thread",
            IsBackground = true
        };
        _thread.Start();
    }

    /// <summary>
    /// 关闭下载器
    /// </summary>
    internal void Close()
    {
        if (!_run)
        {
            return;
        }

        _run = false;
        _semaphoreWait.Release();
    }

    /// <summary>
    /// 停止下载
    /// </summary>
    internal void DownloadStop()
    {
        if (_pause)
        {
            Resume();
        }
    }

    /// <summary>
    /// 开始下载
    /// </summary>
    internal void Start()
    {
        if (_isWait)
        {
            _semaphoreWait.Release();
        }
    }

    /// <summary>
    /// 暂停下载
    /// </summary>
    internal void Pause()
    {
        _pause = true;
    }

    /// <summary>
    /// 恢复下载
    /// </summary>
    internal void Resume()
    {
        _pause = false;
        _semaphorePause.Release();
    }

    /// <summary>
    /// 检查是否需要暂停
    /// </summary>
    /// <param name="item">下载项目</param>
    private void CheckPause(DownloadItem item)
    {
        if (!_pause)
        {
            return;
        }

        item.File.State = DownloadItemState.Pause;
        item.Task.UpdateItem(_index, item.File);
        _semaphorePause.WaitOne();
    }

    /// <summary>
    /// 检查下载文件完整
    /// </summary>
    /// <param name="item">下载项目</param>
    /// <returns>是否完整</returns>
    private bool CheckFile(DownloadItem item)
    {
        var file = item.File;
        using var stream2 = PathHelper.OpenRead(file.Local)!;
        file.State = DownloadItemState.Action;
        item.Task.UpdateItem(_index, item.File);

        if (!string.IsNullOrWhiteSpace(file.Md5))
        {
            if (HashHelper.GenMd5(stream2) != file.Md5)
            {
                return false;
            }
        }
        else if (!string.IsNullOrWhiteSpace(file.Sha1))
        {
            if (HashHelper.GenSha1(stream2) != file.Sha1)
            {
                return false;
            }
        }
        else if (!string.IsNullOrWhiteSpace(file.Sha256))
        {
            if (HashHelper.GenSha256(stream2) != file.Sha256)
            {
                return false;
            }
        }

        stream2.Seek(0, SeekOrigin.Begin);
        file.Later?.Invoke(stream2);

        file.State = DownloadItemState.Done;
        item.Task.UpdateItem(_index, item.File);
        item.Task.Done();
        return true;
    }

    /// <summary>
    /// 写文件
    /// </summary>
    /// <param name="item">下载项目</param>
    /// <param name="file">临时文件</param>
    /// <param name="buffer">缓存</param>
    /// <param name="stream1">网络流</param>
    /// <returns>是否下载失败</returns>
    private bool Download(DownloadItem item, string file, byte[] buffer, Stream stream1, bool isKeep)
    {
        //获取buffer
        using var stream = isKeep ? PathHelper.OpenAppend(file) : PathHelper.OpenWrite(file);

        int bytesRead;
        //写文件
        while ((bytesRead = stream1.ReadAsync(buffer, item.Task.Token).AsTask().Result) != 0)
        {
            stream.WriteAsync(buffer, 0, bytesRead, item.Task.Token).Wait();

            CheckPause(item);

            if (item.Task.Token.IsCancellationRequested)
            {
                break;
            }

            item.File.State = DownloadItemState.Download;
            item.File.NowSize += bytesRead;
            item.Task.UpdateItem(_index, item.File);
        }

        CheckPause(item);

        if (item.Task.Token.IsCancellationRequested)
        {
            return true;
        }

        //检查文件
        if (ConfigLoad.Config.Http.CheckFile)
        {
            if (stream.Position != item.File.AllSize)
            {
                item.File.State = DownloadItemState.Error;
                item.Task.UpdateItem(_index, item.File);

                ColorMCCore.OnError(new DownloadSizeErrorEvnetArgs(item.File, stream.Position));

                return true;
            }
            if (!string.IsNullOrWhiteSpace(item.File.Md5))
            {
                stream.Seek(0, SeekOrigin.Begin);
                string md5 = HashHelper.GenMd5(stream);
                if (md5 != item.File.Md5)
                {
                    item.File.State = DownloadItemState.Error;
                    item.Task.UpdateItem(_index, item.File);

                    ColorMCCore.OnError(new DownloadHashErrorEventArgs(
                        item.File, item.File.Md5, md5));
                    return true;
                }
            }
            if (!string.IsNullOrWhiteSpace(item.File.Sha1))
            {
                stream.Seek(0, SeekOrigin.Begin);
                string sha1 = HashHelper.GenSha1(stream);
                if (sha1 != item.File.Sha1)
                {
                    item.File.State = DownloadItemState.Error;
                    item.Task.UpdateItem(_index, item.File);
                    ColorMCCore.OnError(new DownloadHashErrorEventArgs(
                        item.File, item.File.Sha1, sha1));
                    return true;
                }
            }
            if (!string.IsNullOrWhiteSpace(item.File.Sha256))
            {
                stream.Seek(0, SeekOrigin.Begin);
                string sha256 = HashHelper.GenSha256(stream);
                if (sha256 != item.File.Sha256)
                {
                    item.File.State = DownloadItemState.Error;
                    item.Task.UpdateItem(_index, item.File);
                    ColorMCCore.OnError(new DownloadHashErrorEventArgs(
                         item.File, item.File.Sha256, sha256));
                    return true;
                }
            }
        }

        CheckPause(item);

        return false;
    }

    /// <summary>
    /// 线程运行
    /// </summary>
    private void Run()
    {
        _isWait = true;
        _semaphoreWait.WaitOne();
        _isWait = false;
        if (!_run)
        {
            return;
        }

        while (DownloadManager.GetDownloadItem() is { } item)
        {
            CheckPause(item);

            if (item.Task.Token.IsCancellationRequested)
            {
                break;
            }

            var info = new FileInfo(item.File.Local);

            if (info.Exists)
            {
                try
                {
                    //检查文件
                    if (item.File.Overwrite)
                    {
                        PathHelper.Delete(item.File.Local);
                    }
                    else if (ConfigLoad.Config.Http.CheckFile && CheckFile(item))
                    {
                        //已经有了跳过
                        item.Task.Done();
                        continue;
                    }
                }
                catch (Exception e)
                {
                    //读取或者判断时错误 则强制覆盖原文件
                    item.File.State = DownloadItemState.Error;
                    item.File.ErrorTime++;
                    item.Task.UpdateItem(_index, item.File);
                    ColorMCCore.OnError(new DownloadExceptionErrorEventArgs(item.File, e));
                }
            }

            if (item.Task.Token.IsCancellationRequested)
            {
                break;
            }

            //尝试下载次数
            int time = 0;
            //返回头是否支持断续
            bool useBreak = false;
            //是否真正支持断续
            bool serverRanges = true;
            //是否正在断点续传
            bool isKeep = false;
            //创建临时文件
            var file = Path.Combine(DownloadManager.DownloadDir, FuntionUtils.NewUUID());
            for (; ; )
            {
                byte[]? buffer = null;
                try
                {
                    CheckPause(item);

                    if (item.Task.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    //网络请求
                    HttpResponseMessage data;
                    if (useBreak && serverRanges)
                    {
                        data = CoreHttpClient.GetRangesAsync(item.File.Url, item.File.NowSize, item.Task.Token).Result;
                        if (!data.IsSuccessStatusCode)
                        {
                            serverRanges = false;
                            continue;
                        }
                        isKeep = true;
                    }
                    else
                    {
                        data = CoreHttpClient.GetAsync(item.File.Url, item.Task.Token).Result;
                        item.File.NowSize = 0;
                        item.File.AllSize = data.Content.Headers.ContentLength ?? 0;
                    }
                    if (data.Headers.AcceptRanges.FirstOrDefault() == "bytes")
                    {
                        useBreak = true;
                    }

                    item.File.State = DownloadItemState.GetInfo;
                    item.Task.UpdateItem(_index, item.File);

                    using var stream1 = data.Content.ReadAsStream();

                    buffer = ArrayPool<byte>.Shared.Rent(GetCopyBufferSize(stream1));

                    //开始下载，断点续传时不覆盖原有文件
                    if (Download(item, file, buffer, stream1, isKeep)
                        || item.Task.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    PathHelper.MoveFile(file, item.File.Local);

                    //后续操作
                    if (item.File.Later != null)
                    {
                        item.File.State = DownloadItemState.Action;
                        item.Task.UpdateItem(_index, item.File);
                        using var stream2 = PathHelper.OpenRead(item.File.Local)!;
                        item.File.Later(stream2);
                    }

                    //下载完成
                    item.File.State = DownloadItemState.Done;
                    item.Task.UpdateItem(_index, item.File);
                    item.Task.Done();
                    break;
                }
                catch (Exception e)
                {
                    if (item.Task.Token.IsCancellationRequested)
                    {
                        break;
                    }

                    //下载发生异常
                    item.File.State = DownloadItemState.Error;
                    item.File.ErrorTime++;
                    item.Task.UpdateItem(_index, item.File);
                    time++;
                    ColorMCCore.OnError(new DownloadExceptionErrorEventArgs(item.File, e));

                    if (time >= 5)
                    {
                        var res = UrlHelper.UrlChange(item.File.Url);
                        if (res != null)
                        {
                            item.File.Url = res;
                            time = 0;
                        }
                        else
                        {
                            item.Task.Error();
                            continue;
                        }
                    }
                }
                finally
                {
                    if (buffer != null)
                    {
                        ArrayPool<byte>.Shared.Return(buffer);
                    }
                }
            }
        }
    }
}
