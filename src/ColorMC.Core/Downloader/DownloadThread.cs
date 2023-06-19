using ColorMC.Core.Helpers;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using System.Buffers;

namespace ColorMC.Core.Downloader;

/// <summary>
/// 下载线程
/// </summary>
public class DownloadThread
{
    private readonly int index;
    private readonly Thread thread;
    private readonly Semaphore semaphore = new(0, 2);
    private readonly Semaphore semaphore1 = new(0, 2);
    private CancellationTokenSource? cancel;
    private bool pause = false;
    private bool run = false;

    /// <summary>
    /// 初始化下载器
    /// </summary>
    /// <param name="index">标号</param>
    public DownloadThread(int index)
    {
        this.index = index;
        thread = new(Run)
        {
            Name = $"DownloadThread_{index}"
        };
        run = true;
        thread.Start();
    }

    /// <summary>
    /// 关闭下载器
    /// </summary>
    public void Close()
    {
        if (!run)
            return;

        run = false;
        cancel?.Cancel();
        semaphore.Release();
    }

    /// <summary>
    /// 停止下载
    /// </summary>
    public void DownloadStop()
    {
        cancel?.Cancel();
        if (pause)
        {
            Resume();
        }
    }

    /// <summary>
    /// 开始下载
    /// </summary>
    public void Start()
    {
        cancel = new();
        semaphore.Release();
    }

    /// <summary>
    /// 暂停下载
    /// </summary>
    public void Pause()
    {
        pause = true;
    }

    /// <summary>
    /// 恢复下载
    /// </summary>
    public void Resume()
    {
        pause = false;
        semaphore1.Release();
    }

    /// <summary>
    /// 检查暂停
    /// </summary>
    private void ChckPause(DownloadItemObj item)
    {
        if (pause)
        {
            item.State = DownloadItemState.Pause;
            item.Update?.Invoke(index);
            semaphore1.WaitOne();
        }
    }

    private void Run()
    {
        while (run)
        {
            semaphore.WaitOne();
            if (!run)
                return;
            DownloadItemObj? item;
            while ((item = DownloadManager.GetItem()) != null)
            {
                ChckPause(item);

                if (cancel!.IsCancellationRequested)
                    break;

                byte[]? buffer = null;

                try
                {
                    //检查文件
                    if (ConfigUtils.Config.Http.CheckFile && File.Exists(item.Local))
                    {
                        if (!string.IsNullOrWhiteSpace(item.SHA1) && !item.Overwrite)
                        {
                            using FileStream stream2 = new(item.Local, FileMode.Open,
                                FileAccess.Read, FileShare.Read);
                            stream2.Seek(0, SeekOrigin.Begin);
                            string sha1 = Funtcions.GenSha1(stream2);
                            if (sha1 == item.SHA1)
                            {
                                item.State = DownloadItemState.Action;
                                item.Update?.Invoke(index);
                                stream2.Seek(0, SeekOrigin.Begin);
                                item.Later?.Invoke(stream2);

                                item.State = DownloadItemState.Done;
                                item.Update?.Invoke(index);
                                DownloadManager.Done();
                                continue;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(item.SHA256) && !item.Overwrite)
                        {
                            using FileStream stream2 = new(item.Local, FileMode.Open,
                                FileAccess.Read, FileShare.Read);
                            stream2.Seek(0, SeekOrigin.Begin);
                            string sha1 = Funtcions.GenSha256(stream2);
                            if (sha1 == item.SHA256)
                            {
                                item.State = DownloadItemState.Action;
                                item.Update?.Invoke(index);
                                stream2.Seek(0, SeekOrigin.Begin);
                                item.Later?.Invoke(stream2);

                                item.State = DownloadItemState.Done;
                                item.Update?.Invoke(index);
                                DownloadManager.Done();
                                continue;
                            }
                        }

                        File.Delete(item.Local);
                    }
                    FileInfo info = new(item.Local);
                    if (!Directory.Exists(info.DirectoryName))
                    {
                        Directory.CreateDirectory(info.DirectoryName!);
                    }
                }
                catch (Exception e)
                {
                    item.State = DownloadItemState.Error;
                    item.ErrorTime++;
                    item.Update?.Invoke(index);
                    DownloadManager.Error(index, item, e);
                    continue;
                }

                if (cancel.IsCancellationRequested)
                    break;

                int time = 0;

                do
                {
                    try
                    {
                        ChckPause(item);

                        if (cancel.IsCancellationRequested)
                            break;

                        //网络请求
                        var data = BaseClient.DownloadClient.GetAsync(item.Url,
                            HttpCompletionOption.ResponseHeadersRead, cancel.Token).Result;
                        item.AllSize = (long)data.Content.Headers.ContentLength!;
                        item.State = DownloadItemState.GetInfo;
                        item.NowSize = 0;
                        item.Update?.Invoke(index);
                        using Stream stream1 = data.Content.ReadAsStream(cancel.Token);
                        buffer = ArrayPool<byte>.Shared.Rent(DownloadManager.GetCopyBufferSize(stream1));

                        string file = Path.GetFullPath(DownloadManager.DownloadDir + '/' + Guid.NewGuid().ToString());

                        using FileStream stream = new(file, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

                        int bytesRead;
                        while ((bytesRead = stream1.ReadAsync(new Memory<byte>(buffer),
                            cancel.Token).Result) != 0)
                        {
                            stream.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0,
                                bytesRead), cancel.Token).AsTask().Wait(cancel.Token);

                            ChckPause(item);

                            if (cancel.IsCancellationRequested)
                                break;

                            item.State = DownloadItemState.Download;
                            item.NowSize += bytesRead;
                            item.Update?.Invoke(index);
                        }

                        if (pause)
                        {
                            item.State = DownloadItemState.Pause;
                            semaphore1.WaitOne();
                        }

                        if (cancel.IsCancellationRequested)
                            break;

                        //检查文件
                        if (ConfigUtils.Config.Http.CheckFile)
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            if (!string.IsNullOrWhiteSpace(item.SHA1))
                            {
                                string sha1 = Funtcions.GenSha1(stream);
                                if (sha1 != item.SHA1)
                                {
                                    item.State = DownloadItemState.Error;
                                    item.Update?.Invoke(index);
                                    DownloadManager.Error(index, item, new Exception("hash error"));

                                    break;
                                }
                            }
                            else if (!string.IsNullOrWhiteSpace(item.SHA256))
                            {
                                string sha1 = Funtcions.GenSha256(stream);
                                if (sha1 != item.SHA256)
                                {
                                    item.State = DownloadItemState.Error;
                                    item.Update?.Invoke(index);
                                    DownloadManager.Error(index, item, new Exception("hash error"));

                                    break;
                                }
                            }
                        }

                        item.State = DownloadItemState.Action;
                        item.Update?.Invoke(index);

                        ChckPause(item);

                        if (cancel.IsCancellationRequested)
                            break;

                        //后续操作
                        stream.Seek(0, SeekOrigin.Begin);
                        item.Later?.Invoke(stream);

                        stream.Dispose();
                        if (File.Exists(item.Local))
                        {
                            File.Delete(item.Local);
                        }
                        new FileInfo(item.Local).Directory?.Create();
                        File.Move(file, item.Local);

                        item.State = DownloadItemState.Done;
                        item.Update?.Invoke(index);
                        DownloadManager.Done();
                        break;
                    }
                    catch (Exception e)
                    {
                        if (cancel.IsCancellationRequested)
                            break;

                        item.State = DownloadItemState.Error;
                        item.ErrorTime++;
                        item.Update?.Invoke(index);
                        time++;
                        DownloadManager.Error(index, item, e);

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
}
