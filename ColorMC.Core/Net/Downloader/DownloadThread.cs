using ColorMC.Core.Utils;
using System.Buffers;

namespace ColorMC.Core.Net.Downloader;

public class DownloadThread
{
    private int index;
    private Thread thread;
    private bool run = false;
    private readonly Semaphore semaphore = new(0, 2);
    private readonly Semaphore semaphore1 = new(0, 2);
    private CancellationTokenSource cancel;
    private bool pause = false;
    public void Init(int index)
    {
        this.index = index;
        thread = new(Run)
        {
            Name = $"DownloadThread_{index}"
        };
        run = true;
        thread.Start();
    }

    public void Close()
    {
        if (!run)
            return;

        run = false;
        cancel?.Cancel();
        semaphore.Release();
    }

    public void DownloadStop()
    {
        cancel?.Cancel();
    }

    public void Start()
    {
        cancel = new();
        semaphore.Release();
    }

    public void Pause()
    {
        pause = true;
    }

    public void Resume()
    {
        pause = false;
        semaphore1.Release();
    }

    private async void Run()
    {
        while (run)
        {
            semaphore.WaitOne();
            if (!run)
                return;
            DownloadItem? item;
            while ((item = DownloadManager.GetItem()) != null)
            {
                if (pause)
                    semaphore1.WaitOne();

                byte[]? buffer = null;

                try
                {
                    if (File.Exists(item.Local))
                    {
                        if (!string.IsNullOrWhiteSpace(item.SHA1) && !item.Overwrite)
                        {
                            using FileStream stream2 = new(item.Local, FileMode.Open, FileAccess.Read, FileShare.Read);
                            stream2.Seek(0, SeekOrigin.Begin);
                            string sha1 = Funtcions.GenSha1(stream2);
                            if (sha1 == item.SHA1)
                            {
                                item.State = DownloadItemState.Action;
                                item.Update?.Invoke(index);
                                item.Later?.Invoke(stream2);

                                item.State = DownloadItemState.Done;
                                item.Update?.Invoke(index);
                                DownloadManager.Done();
                                continue;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(item.SHA256) && !item.Overwrite)
                        {
                            using FileStream stream2 = new(item.Local, FileMode.Open, FileAccess.Read, FileShare.Read);
                            stream2.Seek(0, SeekOrigin.Begin);
                            string sha1 = Funtcions.GenSha256(stream2);
                            if (sha1 == item.SHA256)
                            {
                                item.State = DownloadItemState.Action;
                                item.Update?.Invoke(index);
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

                int time = 0;

                do
                {
                    try
                    {
                        if (pause)
                            semaphore1.WaitOne();

                        var data = await BaseClient.DownloadClient.GetAsync(item.Url,
                            HttpCompletionOption.ResponseHeadersRead, cancel.Token);
                        item.AllSize = (long)data.Content.Headers.ContentLength!;
                        item.State = DownloadItemState.GetInfo;
                        item.NowSize = 0;
                        item.Update?.Invoke(index);
                        using Stream stream1 = data.Content.ReadAsStream(cancel.Token);
                        buffer = ArrayPool<byte>.Shared.Rent(DownloadManager.GetCopyBufferSize(stream1));

                        using FileStream stream = new(item.Local, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

                        int bytesRead;
                        while ((bytesRead = await stream1.ReadAsync(new Memory<byte>(buffer),
                            cancel.Token).ConfigureAwait(false)) != 0)
                        {
                            await stream.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0,
                                bytesRead), cancel.Token).ConfigureAwait(false);

                            if (pause)
                                semaphore1.WaitOne();

                            item.State = DownloadItemState.Download;
                            item.NowSize += bytesRead;
                            item.Update?.Invoke(index);
                        }

                        if (pause)
                            semaphore1.WaitOne();

                        if (!string.IsNullOrWhiteSpace(item.SHA1))
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            string sha1 = Funtcions.GenSha1(stream);
                            if (sha1 != item.SHA1)
                            {
                                item.State = DownloadItemState.Error;
                                item.Update?.Invoke(index);
                                DownloadManager.Error(index, item, new Exception("hash error"));
                            }
                        }

                        item.State = DownloadItemState.Action;
                        item.Update?.Invoke(index);

                        if (pause)
                            semaphore1.WaitOne();

                        item.Later?.Invoke(stream);

                        item.State = DownloadItemState.Done;
                        item.Update?.Invoke(index);
                        DownloadManager.Done();
                        break;
                    }
                    catch (Exception e)
                    {
                        if (cancel.IsCancellationRequested)
                            return;

                        item.State = DownloadItemState.Error;
                        item.ErrorTime++;
                        item.Update?.Invoke(index);
                        time++;
                        DownloadManager.Error(index, item, e);
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
