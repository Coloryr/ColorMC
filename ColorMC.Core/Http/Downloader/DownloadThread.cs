using ColorMC.Core.Utils;
using System.Buffers;

namespace ColorMC.Core.Http.Downloader;

public class DownloadThread
{
    private int index;
    private Thread thread;
    private bool run = false;
    private readonly Semaphore semaphore = new(0, 2);
    private CancellationTokenSource cancel;
    public void Init(int index)
    {
        this.index = index;
        thread = new(Run)
        {
            Name = $"DownloadThread[{index}]"
        };
        run = true;
        thread.Start();
    }

    public void Close()
    {
        run = false;
        semaphore.Release();
        thread.Join();
    }

    public void Cancel()
    {
        cancel.Cancel();
    }

    public void Start()
    {
        cancel = new();
        semaphore.Release();
    }

    private static int GetCopyBufferSize(Stream stream)
    {
        const int DefaultCopyBufferSize = 81920;

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

    public static async Task Download(DownloadItem item)
    {
        FileInfo info = new(item.Local);
        if (!Directory.Exists(info.DirectoryName))
        {
            Directory.CreateDirectory(info.DirectoryName!);
        }
        using FileStream stream = new(item.Local, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        var data = await BaseClient.Client.GetAsync(item.Url, HttpCompletionOption.ResponseHeadersRead);
        using Stream stream1 = data.Content.ReadAsStream();
        byte[] buffer = ArrayPool<byte>.Shared.Rent(GetCopyBufferSize(stream1));
        try
        {
            int bytesRead;
            while ((bytesRead = await stream1.ReadAsync(new Memory<byte>(buffer))
                .ConfigureAwait(false)) != 0)
            {
                await stream.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, bytesRead))
                    .ConfigureAwait(false);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
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
                byte[]? buffer = null;
                int time = 0;
                do
                {
                    try
                    {
                        item.State = DownloadItemState.Init;
                        item.Update?.Invoke(index);
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
                                    DownloadManager.Done(index);
                                    break;
                                }
                            }

                            File.Delete(item.Local);
                        }
                        FileInfo info = new(item.Local);
                        if (!Directory.Exists(info.DirectoryName))
                        {
                            Directory.CreateDirectory(info.DirectoryName!);
                        }

                        var data = await BaseClient.Client.GetAsync(item.Url,
                            HttpCompletionOption.ResponseHeadersRead, cancel.Token);
                        item.AllSize = (long)data.Content.Headers.ContentLength!;
                        item.State = DownloadItemState.Download;
                        item.Update?.Invoke(index);
                        using Stream stream1 = data.Content.ReadAsStream(cancel.Token);
                        buffer = ArrayPool<byte>.Shared.Rent(GetCopyBufferSize(stream1));

                        using FileStream stream = new(item.Local, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

                        int bytesRead;
                        while ((bytesRead = await stream1.ReadAsync(new Memory<byte>(buffer),
                            cancel.Token).ConfigureAwait(false)) != 0)
                        {
                            await stream.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0,
                                bytesRead), cancel.Token).ConfigureAwait(false);
                            item.NowSize += bytesRead;
                            item.Update?.Invoke(index);
                        }

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

                        item.Later?.Invoke(stream);

                        item.State = DownloadItemState.Done;
                        item.Update?.Invoke(index);
                        DownloadManager.Done(index);
                        break;
                    }
                    catch (Exception e)
                    {
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
