using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using System.Buffers;

namespace ColorMC.Core.Downloader;

/// <summary>
/// 下载线程
/// </summary>
public class DownloadThread
{
    /// <summary>
    /// 下载线程标号
    /// </summary>
    private readonly int _index;
    /// <summary>
    /// 线程
    /// </summary>
    private readonly Thread _thread;
    /// <summary>
    /// 启动信号量
    /// </summary>
    private readonly Semaphore _semaphore = new(0, 2);
    /// <summary>
    /// 暂停信号量
    /// </summary>
    private readonly Semaphore _semaphore1 = new(0, 2);
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
    private CancellationToken _token;

    /// <summary>
    /// 初始化下载器
    /// </summary>
    /// <param name="index">标号</param>
    public DownloadThread(int index)
    {
        _index = index;
        _thread = new(() =>
        {
            while (_run)
            {
                Run();
            }
        })
        {
            Name = $"DownloadThread_{index}"
        };
        _run = true;
        _thread.Start();
    }

    /// <summary>
    /// 关闭下载器
    /// </summary>
    public void Close()
    {
        if (!_run)
            return;

        _run = false;
        _semaphore.Release();
    }

    /// <summary>
    /// 停止下载
    /// </summary>
    public void DownloadStop()
    {
        if (_pause)
        {
            Resume();
        }
    }

    /// <summary>
    /// 开始下载
    /// </summary>
    public void Start(CancellationToken token)
    {
        _token = token;
        _semaphore.Release();
    }

    /// <summary>
    /// 暂停下载
    /// </summary>
    public void Pause()
    {
        _pause = true;
    }

    /// <summary>
    /// 恢复下载
    /// </summary>
    public void Resume()
    {
        _pause = false;
        _semaphore1.Release();
    }

    /// <summary>
    /// 检查暂停
    /// </summary>
    private void ChckPause(DownloadItemObj item)
    {
        if (_pause)
        {
            item.State = DownloadItemState.Pause;
            item.Update?.Invoke(_index);
            _semaphore1.WaitOne();
        }
    }

    /// <summary>
    /// 线程运行
    /// </summary>
    private void Run()
    {
        _semaphore.WaitOne();
        if (!_run)
        {
            return;
        }
        DownloadItemObj? item;
        while ((item = DownloadManager.GetItem()) != null)
        {
            if (item.Skip)
            {
                item.State = DownloadItemState.Done;
                item.Update?.Invoke(_index);
                DownloadManager.Done();
            }

            ChckPause(item);

            if (_token.IsCancellationRequested)
            {
                break;
            }

            byte[]? buffer = null;
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
                    else if (ConfigUtils.Config.Http.CheckFile)
                    {
                        if (!string.IsNullOrWhiteSpace(item.MD5))
                        {
                            using var stream2 = PathHelper.OpenRead(item.Local)!;
                            string sha1 = HashHelper.GenMd5(stream2);
                            if (sha1 == item.MD5)
                            {
                                item.State = DownloadItemState.Action;
                                item.Update?.Invoke(_index);
                                stream2.Seek(0, SeekOrigin.Begin);
                                item.Later?.Invoke(stream2);

                                item.State = DownloadItemState.Done;
                                item.Update?.Invoke(_index);
                                DownloadManager.Done();
                                continue;
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(item.SHA1))
                        {
                            using var stream2 = PathHelper.OpenRead(item.Local)!;
                            string sha1 = HashHelper.GenSha1(stream2);
                            if (sha1 == item.SHA1)
                            {
                                item.State = DownloadItemState.Action;
                                item.Update?.Invoke(_index);
                                stream2.Seek(0, SeekOrigin.Begin);
                                item.Later?.Invoke(stream2);

                                item.State = DownloadItemState.Done;
                                item.Update?.Invoke(_index);
                                DownloadManager.Done();
                                continue;
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(item.SHA256))
                        {
                            using var stream2 = PathHelper.OpenRead(item.Local)!;
                            string sha1 = HashHelper.GenSha256(stream2);
                            if (sha1 == item.SHA256)
                            {
                                item.State = DownloadItemState.Action;
                                item.Update?.Invoke(_index);
                                stream2.Seek(0, SeekOrigin.Begin);
                                item.Later?.Invoke(stream2);

                                item.State = DownloadItemState.Done;
                                item.Update?.Invoke(_index);
                                DownloadManager.Done();
                                continue;
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    item.State = DownloadItemState.Error;
                    item.ErrorTime++;
                    item.Update?.Invoke(_index);
                    DownloadManager.Error(_index, item, e);
                    continue;
                }
            }

            if (_token.IsCancellationRequested)
            {
                break;
            }

            int time = 0;

            do
            {
                try
                {
                    ChckPause(item);

                    if (_token.IsCancellationRequested)
                    {
                        break;
                    }

                    //网络请求
                    var req = new HttpRequestMessage(HttpMethod.Get, item.Url);
                    if (item.UseColorMCHead)
                    {
                        req.Headers.Add("ColorMC", ColorMCCore.Version);
                    }
                    var data = BaseClient.DownloadClient.SendAsync(req,
                        HttpCompletionOption.ResponseHeadersRead,
                        _token).Result;
                    item.AllSize = (long)data.Content.Headers.ContentLength!;
                    item.State = DownloadItemState.GetInfo;
                    item.NowSize = 0;
                    item.Update?.Invoke(_index);
                    using Stream stream1 = data.Content.ReadAsStream(_token);

                    //获取buffer
                    buffer = ArrayPool<byte>.Shared.Rent(DownloadManager.GetCopyBufferSize(stream1));

                    //创建临时文件
                    string file = Path.GetFullPath(DownloadManager.DownloadDir + '/' + Guid.NewGuid().ToString());

                    using var stream = PathHelper.OpenWrite(file);

                    int bytesRead;
                    //写文件
                    while ((bytesRead = stream1.ReadAsync(buffer, _token).AsTask().Result) != 0)
                    {
                        stream.WriteAsync(buffer, 0, bytesRead, _token).Wait();

                        ChckPause(item);

                        if (_token.IsCancellationRequested)
                        {
                            break;
                        }

                        item.State = DownloadItemState.Download;
                        item.NowSize += bytesRead;
                        item.Update?.Invoke(_index);
                    }

                    ChckPause(item);

                    if (_token.IsCancellationRequested)
                    {
                        break;
                    }

                    //检查文件
                    if (ConfigUtils.Config.Http.CheckFile)
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        if (!string.IsNullOrWhiteSpace(item.MD5))
                        {
                            string sha1 = HashHelper.GenMd5(stream);
                            if (sha1 != item.MD5)
                            {
                                item.State = DownloadItemState.Error;
                                item.Update?.Invoke(_index);
                                DownloadManager.Error(_index, item, new Exception(LanguageHelper.Get("Core.Http.Error10")));

                                break;
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(item.SHA1))
                        {
                            string sha1 = HashHelper.GenSha1(stream);
                            if (sha1 != item.SHA1)
                            {
                                item.State = DownloadItemState.Error;
                                item.Update?.Invoke(_index);
                                DownloadManager.Error(_index, item, new Exception(LanguageHelper.Get("Core.Http.Error10")));

                                break;
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(item.SHA256))
                        {
                            string sha1 = HashHelper.GenSha256(stream);
                            if (sha1 != item.SHA256)
                            {
                                item.State = DownloadItemState.Error;
                                item.Update?.Invoke(_index);
                                DownloadManager.Error(_index, item, new Exception(LanguageHelper.Get("Core.Http.Error10")));

                                break;
                            }
                        }
                    }

                    item.State = DownloadItemState.Action;
                    item.Update?.Invoke(_index);

                    ChckPause(item);

                    if (_token.IsCancellationRequested)
                    {
                        break;
                    }

                    //后续操作
                    stream.Seek(0, SeekOrigin.Begin);
                    item.Later?.Invoke(stream);

                    stream.Dispose();
                    if (File.Exists(item.Local))
                    {
                        PathHelper.Delete(item.Local);
                    }
                    info.Directory?.Create();
                    PathHelper.MoveFile(file, item.Local);

                    item.State = DownloadItemState.Done;
                    item.Update?.Invoke(_index);
                    DownloadManager.Done();
                    break;
                }
                catch (Exception e)
                {
                    //下载发生异常
                    if (_token.IsCancellationRequested)
                    {
                        break;
                    }

                    item.State = DownloadItemState.Error;
                    item.ErrorTime++;
                    item.Update?.Invoke(_index);
                    time++;
                    DownloadManager.Error(_index, item, e);

                    if (time == 5)
                    {
                        var res = UrlHelper.UrlChange(item.Url);
                        if (res.Item1)
                        {
                            item.Url = res.Item2!;
                            time = 0;
                            continue;
                        }
                    }
                }
                finally
                {
                    if (buffer != null)
                        ArrayPool<byte>.Shared.Return(buffer);
                }
            } while (time < 5);
        }

        DownloadManager.ThreadDone();
    }
}
