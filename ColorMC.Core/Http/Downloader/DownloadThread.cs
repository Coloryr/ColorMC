using ColorMC.Core.Http.Download;
using ColorMC.Core.Utils;
using System.Buffers;
using static ICSharpCode.SharpZipLib.Zip.FastZip;

namespace ColorMC.Core.Http.Downloader;

public class DownloadThread
{
    private Thread thread;
    private bool run = false;
    private Semaphore semaphore = new(0, 2);
    private CancellationTokenSource cancel;
    public void Init(int index)
    {
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

    private static async Task Download(DownloadItem item, CancellationToken cancel)
    {
        try
        {
            item.State = DownloadItemState.Init;
            item.Update?.Invoke();
            if (File.Exists(item.Local))
            {
                if (!string.IsNullOrWhiteSpace(item.SHA1) && !item.Overwrite)
                {
                    using FileStream stream2 = new(item.Local, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                    stream2.Seek(0, SeekOrigin.Begin);
                    string sha1 = Sha1.GenSha1(stream2);
                    if (sha1 == item.SHA1)
                    {
                        item.Later?.Invoke();

                        item.State = DownloadItemState.Done;
                        item.Update?.Invoke();
                        DownloadManager.Done();
                        return;
                    }
                }

                File.Delete(item.Local);
            }
            FileInfo info = new(item.Local);
            if (!Directory.Exists(info.DirectoryName))
            {
                Directory.CreateDirectory(info.DirectoryName!);
            }
            using FileStream stream = new(item.Local, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            var data = await BaseClient.Client.GetAsync(item.Url, HttpCompletionOption.ResponseHeadersRead, cancel);
            item.AllSize = (long)data.Content.Headers.ContentLength!;
            item.State = DownloadItemState.Download;
            item.Update?.Invoke();
            using Stream stream1 = data.Content.ReadAsStream(cancel);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(GetCopyBufferSize(stream1));
            try
            {
                int bytesRead;
                while ((bytesRead = await stream1.ReadAsync(new Memory<byte>(buffer), cancel).ConfigureAwait(false)) != 0)
                {
                    await stream.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, bytesRead), cancel).ConfigureAwait(false);
                    item.NowSize += bytesRead;
                    item.Update?.Invoke();
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
            if (!string.IsNullOrWhiteSpace(item.SHA1))
            {
                stream.Seek(0, SeekOrigin.Begin);
                string sha1 = Sha1.GenSha1(stream);
                if (sha1 != item.SHA1)
                {
                    item.State = DownloadItemState.Error;
                    item.Update?.Invoke();
                    DownloadManager.Error(item, new Exception("hash error"));
                }
            }
            item.State = DownloadItemState.Action;
            item.Update?.Invoke();

            item.Later?.Invoke();

            item.State = DownloadItemState.Done;
            item.Update?.Invoke();
            DownloadManager.Done();
        }
        catch (Exception e)
        {
            item.State = DownloadItemState.Error;
            item.Update?.Invoke();
            DownloadManager.Error(item, e);
        }
    }

    public static async Task Download(DownloadItem item)
    {
        FileInfo info = new(item.Local);
        if (!Directory.Exists(info.DirectoryName))
        {
            Directory.CreateDirectory(info.DirectoryName!);
        }
        if (info.Exists && info.Length > 0)
        {
            return;
        }
        using FileStream stream = new(item.Local, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        var data = await BaseClient.Client.GetAsync(item.Url, HttpCompletionOption.ResponseHeadersRead);
        using Stream stream1 = data.Content.ReadAsStream();
        byte[] buffer = ArrayPool<byte>.Shared.Rent(GetCopyBufferSize(stream1));
        try
        {
            int bytesRead;
            while ((bytesRead = await stream1.ReadAsync(new Memory<byte>(buffer)).ConfigureAwait(false)) != 0)
            {
                await stream.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, bytesRead)).ConfigureAwait(false);
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
                await Download(item, cancel.Token);
            }
        }
    }
}
