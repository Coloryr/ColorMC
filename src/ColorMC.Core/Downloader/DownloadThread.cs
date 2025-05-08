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
    private bool _pause = false;
    /// <summary>
    /// 是否在运行
    /// </summary>
    private bool _run = false;
    /// <summary>
    /// 是否被取消
    /// </summary>
    private CancellationTokenSource _cancel;
    /// <summary>
    /// 下载任务
    /// </summary>
    private DownloadTask _task;

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
        _thread = new(() =>
        {
            while (_run)
            {
                Run();
            }
        })
        {
            Name = "ColorMC Download Thread"
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
    internal void Start(DownloadTask task)
    {
        if (_cancel != null && !_cancel.IsCancellationRequested)
        {
            _cancel.Cancel();
        }
        _cancel = CancellationTokenSource.CreateLinkedTokenSource(task.Token);

        _task = task;
        _semaphoreWait.Release();
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
    private void CheckPause(FileItemObj item)
    {
        if (_pause)
        {
            item.State = DownloadItemState.Pause;
            item.Update(_index);
            _semaphorePause.WaitOne();
        }
    }

    /// <summary>
    /// 检查下载文件完整
    /// </summary>
    /// <param name="item">下载项目</param>
    /// <returns>是否完整</returns>
    private bool CheckFile(FileItemObj item)
    {
        using var stream2 = PathHelper.OpenRead(item.Local)!;
        item.State = DownloadItemState.Action;
        item.Update(_index);

        if (!string.IsNullOrWhiteSpace(item.Md5))
        {
            if (HashHelper.GenMd5(stream2) != item.Md5)
            {
                return false;
            }
        }
        else if (!string.IsNullOrWhiteSpace(item.Sha1))
        {
            if (HashHelper.GenSha1(stream2) != item.Sha1)
            {
                return false;
            }
        }
        else if (!string.IsNullOrWhiteSpace(item.Sha256))
        {
            if (HashHelper.GenSha256(stream2) != item.Sha256)
            {
                return false;
            }
        }

        stream2.Seek(0, SeekOrigin.Begin);
        item.Later?.Invoke(stream2);

        item.State = DownloadItemState.Done;
        item.Update(_index);
        _task.Done();
        return true;
    }

    /// <summary>
    /// 线程运行
    /// </summary>
    private void Run()
    {
        _semaphoreWait.WaitOne();
        if (!_run)
        {
            return;
        }
        FileItemObj? item;
        while ((item = _task.GetItem()) != null)
        {
            CheckPause(item);

            if (_cancel.IsCancellationRequested)
            {
                break;
            }

            var info = new FileInfo(item.Local);

            if (info.Exists)
            {
                try
                {
                    //检查文件
                    if (item.Overwrite)
                    {
                        PathHelper.Delete(item.Local);
                    }
                    else if (ConfigUtils.Config.Http.CheckFile && CheckFile(item))
                    {
                        continue;
                    }

                }
                catch (Exception e)
                {
                    item.State = DownloadItemState.Error;
                    item.ErrorTime++;
                    item.Update(_index);
                    DownloadManager.Error(item, e);
                    continue;
                }
            }

            if (_cancel.IsCancellationRequested)
            {
                break;
            }

            int time = 0;
            do
            {
                byte[]? buffer = null;
                try
                {
                    CheckPause(item);

                    if (_cancel.IsCancellationRequested)
                    {
                        break;
                    }

                    //网络请求
                    var data = CoreHttpClient.GetAsync(item.Url, _cancel.Token).Result;
                    item.AllSize = data.Content.Headers.ContentLength ?? 0;
                    item.State = DownloadItemState.GetInfo;
                    item.NowSize = 0;
                    item.Update(_index);
                   
                    //创建临时文件
                    var file = Path.Combine(DownloadManager.DownloadDir, FuntionUtils.NewUUID());

                    bool Download()
                    {
                        using var stream1 = data.Content.ReadAsStream();
                        //获取buffer
                        buffer = ArrayPool<byte>.Shared.Rent(DownloadManager.GetCopyBufferSize(stream1));
                        using var stream = PathHelper.OpenWrite(file, true);

                        int bytesRead;
                        //写文件
                        while ((bytesRead = stream1.ReadAsync(buffer, _cancel.Token).AsTask().Result) != 0)
                        {
                            stream.WriteAsync(buffer, 0, bytesRead, _cancel.Token).Wait();

                            CheckPause(item);

                            if (_cancel.IsCancellationRequested)
                            {
                                break;
                            }

                            item.State = DownloadItemState.Download;
                            item.NowSize += bytesRead;
                            item.Update(_index);
                        }

                        CheckPause(item);

                        if (_cancel.IsCancellationRequested)
                        {
                            return true;
                        }

                        //检查文件
                        if (ConfigUtils.Config.Http.CheckFile)
                        {
                            if (!string.IsNullOrWhiteSpace(item.Md5))
                            {
                                stream.Seek(0, SeekOrigin.Begin);
                                if (HashHelper.GenMd5(stream) != item.Md5)
                                {
                                    item.State = DownloadItemState.Error;
                                    item.Update(_index);
                                    DownloadManager.Error(item, new Exception(LanguageHelper.Get("Core.Http.Error10")));
                                    return true;
                                }
                            }
                            if (!string.IsNullOrWhiteSpace(item.Sha1))
                            {
                                stream.Seek(0, SeekOrigin.Begin);
                                if (HashHelper.GenSha1(stream) != item.Sha1)
                                {
                                    item.State = DownloadItemState.Error;
                                    item.Update(_index);
                                    DownloadManager.Error(item, new Exception(LanguageHelper.Get("Core.Http.Error10")));
                                    return true;
                                }
                            }
                            if (!string.IsNullOrWhiteSpace(item.Sha256))
                            {
                                stream.Seek(0, SeekOrigin.Begin);
                                if (HashHelper.GenSha256(stream) != item.Sha256)
                                {
                                    item.State = DownloadItemState.Error;
                                    item.Update(_index);
                                    DownloadManager.Error(item, new Exception(LanguageHelper.Get("Core.Http.Error10")));
                                    return true;
                                }
                            }
                        }

                        CheckPause(item);

                        return false;
                    }

                    //开始下载
                    if (Download() || _cancel.IsCancellationRequested)
                    {
                        break;
                    }

                    PathHelper.MoveFile(file, item.Local);

                    //后续操作
                    if (item.Later != null)
                    {
                        item.State = DownloadItemState.Action;
                        item.Update(_index);
                        using var stream2 = PathHelper.OpenRead(item.Local)!;
                        item.Later(stream2);
                    }

                    item.State = DownloadItemState.Done;
                    item.Update(_index);
                    _task.Done();
                    break;
                }
                catch (Exception e)
                {
                    //下载发生异常
                    if (_cancel.IsCancellationRequested)
                    {
                        break;
                    }

                    item.State = DownloadItemState.Error;
                    item.ErrorTime++;
                    item.Update(_index);
                    time++;
                    DownloadManager.Error(item, e);

                    if (time == 5)
                    {
                        var res = UrlHelper.UrlChange(item.Url);
                        if (res != null)
                        {
                            item.Url = res;
                            time = 0;
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
            } while (time < 5);
        }

        _task.ThreadDone();
    }
}
