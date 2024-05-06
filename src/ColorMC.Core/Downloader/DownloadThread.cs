using System.Buffers;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;

namespace ColorMC.Core.Downloader;

/// <summary>
/// 下载线程
/// </summary>
public class DownloadThread
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
    private CancellationToken _cancel;

    /// <summary>
    /// 初始化下载器
    /// </summary>
    /// <param name="index">标号</param>
    public DownloadThread(int index)
    {
        _thread = new(() =>
        {
            while (_run)
            {
                Run();
            }
        })
        {
            Name = $"ColorMC_DownloadThread{index}"
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
        {
            return;
        }

        _run = false;
        _semaphoreWait.Release();
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
        _cancel = token;
        _semaphoreWait.Release();
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
        _semaphorePause.Release();
    }

    /// <summary>
    /// 检查暂停
    /// </summary>
    private void ChckPause(DownloadItemObj item)
    {
        if (_pause)
        {
            item.State = DownloadItemState.Pause;
            item.Update();
            _semaphorePause.WaitOne();
        }
    }

    private static bool CheckFile(DownloadItemObj item)
    {
        if (!string.IsNullOrWhiteSpace(item.MD5))
        {
            using var stream2 = PathHelper.OpenRead(item.Local)!;
            if (HashHelper.GenMd5(stream2) == item.MD5)
            {
                item.State = DownloadItemState.Action;
                item.Update();
                stream2.Seek(0, SeekOrigin.Begin);
                item.Later?.Invoke(stream2);

                item.State = DownloadItemState.Done;
                item.Update();
                DownloadManager.Done();
                return true;
            }
        }
        if (!string.IsNullOrWhiteSpace(item.SHA1))
        {
            using var stream2 = PathHelper.OpenRead(item.Local)!;
            if (HashHelper.GenSha1(stream2) == item.SHA1)
            {
                item.State = DownloadItemState.Action;
                item.Update();
                stream2.Seek(0, SeekOrigin.Begin);
                item.Later?.Invoke(stream2);

                item.State = DownloadItemState.Done;
                item.Update();
                DownloadManager.Done();
                return true;
            }
        }
        if (!string.IsNullOrWhiteSpace(item.SHA256))
        {
            using var stream2 = PathHelper.OpenRead(item.Local)!;
            if (HashHelper.GenSha256(stream2) == item.SHA256)
            {
                item.State = DownloadItemState.Action;
                item.Update();
                stream2.Seek(0, SeekOrigin.Begin);
                item.Later?.Invoke(stream2);

                item.State = DownloadItemState.Done;
                item.Update();
                DownloadManager.Done();
                return true;
            }
        }

        return false;
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
        DownloadItemObj? item;
        while ((item = DownloadManager.GetItem()) != null)
        {
            ChckPause(item);

            if (_cancel.IsCancellationRequested)
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
                    else if (ConfigUtils.Config.Http.CheckFile && CheckFile(item))
                    {
                        continue;
                    }

                }
                catch (Exception e)
                {
                    item.State = DownloadItemState.Error;
                    item.ErrorTime++;
                    item.Update();
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
                try
                {
                    ChckPause(item);

                    if (_cancel.IsCancellationRequested)
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
                        HttpCompletionOption.ResponseHeadersRead, _cancel).Result;
                    item.AllSize = (data.Content.Headers.ContentLength ?? 0);
                    item.State = DownloadItemState.GetInfo;
                    item.NowSize = 0;
                    item.Update();
                    using var stream1 = data.Content.ReadAsStream(_cancel);

                    //获取buffer
                    buffer = ArrayPool<byte>.Shared.Rent(DownloadManager.GetCopyBufferSize(stream1));

                    //创建临时文件
                    var file = Path.GetFullPath(DownloadManager.DownloadDir + '/' + Guid.NewGuid().ToString());
                    using var stream = PathHelper.OpenWrite(file);

                    int bytesRead;
                    //写文件
                    while ((bytesRead = stream1.ReadAsync(buffer, _cancel).AsTask().Result) != 0)
                    {
                        stream.WriteAsync(buffer, 0, bytesRead, _cancel).Wait();

                        ChckPause(item);

                        if (_cancel.IsCancellationRequested)
                        {
                            break;
                        }

                        item.State = DownloadItemState.Download;
                        item.NowSize += bytesRead;
                        item.Update();
                    }

                    ChckPause(item);

                    if (_cancel.IsCancellationRequested)
                    {
                        break;
                    }

                    //检查文件
                    if (ConfigUtils.Config.Http.CheckFile)
                    {
                        if (!string.IsNullOrWhiteSpace(item.MD5))
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            if (HashHelper.GenMd5(stream) != item.MD5)
                            {
                                item.State = DownloadItemState.Error;
                                item.Update();
                                DownloadManager.Error(item, new Exception(LanguageHelper.Get("Core.Http.Error10")));
                                break;
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(item.SHA1))
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            if (HashHelper.GenSha1(stream) != item.SHA1)
                            {
                                item.State = DownloadItemState.Error;
                                item.Update();
                                DownloadManager.Error(item, new Exception(LanguageHelper.Get("Core.Http.Error10")));
                                break;
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(item.SHA256))
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            if (HashHelper.GenSha256(stream) != item.SHA256)
                            {
                                item.State = DownloadItemState.Error;
                                item.Update();
                                DownloadManager.Error(item, new Exception(LanguageHelper.Get("Core.Http.Error10")));
                                break;
                            }
                        }
                    }

                    item.State = DownloadItemState.Action;
                    item.Update();

                    ChckPause(item);

                    if (_cancel.IsCancellationRequested)
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
                    item.Update();
                    DownloadManager.Done();
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
                    item.Update();
                    time++;
                    DownloadManager.Error(item, e);

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
