using ColorMC.Core.Utils;
using System.Buffers;
using System.Collections.Concurrent;

namespace ColorMC.Core.Net.Downloader;

public static class DownloadManager
{
    private readonly static ConcurrentQueue<DownloadItem> Items = new();
    private readonly static List<string> Name = new();
    private static List<DownloadThread> threads = new();
    private static Semaphore semaphore = new(0, 10);
    public static CoreRunState State { get;private set; }

    public static int AllSize { get; private set; }
    public static int DoneSize { get; private set; }

    public static void Init()
    {
        Logs.Info($"下载器初始化，线程数{ConfigUtils.Config.Http.DownloadThread}");
        semaphore = new(0, ConfigUtils.Config.Http.DownloadThread + 1);
        threads.ForEach(a => a.Close());
        threads.Clear();
        for (int a = 0; a < ConfigUtils.Config.Http.DownloadThread; a++)
        {
            DownloadThread thread = new();
            thread.Init(a);
            threads.Add(thread);
        }
        Clear();
    }

    public static void Stop()
    {
        threads.ForEach(a => a.Close());
        threads.Clear();
    }

    public static void DownloadStop()
    {
        Name.Clear();
        Items.Clear();

        threads.ForEach(a => a.DownloadStop());
    }

    public static void Pause()
    {
        foreach (var item in threads)
        {
            item.Pause();
        }
    }

    public static void Resume()
    {
        foreach (var item in threads)
        {
            item.Resume();
        }
    }

    public static void Clear()
    {
        Logs.Info($"下载器清空");
        Name.Clear();
        Items.Clear();
        AllSize = 0;
        DoneSize = 0;
    }

    public static async Task<bool> Start()
    {
        Logs.Info($"下载器启动");
        DoneSize = 0;
        AllSize = Items.Count;
        CoreMain.DownloaderUpdate?.Invoke(State = CoreRunState.Start);
        foreach (var item in threads)
        {
            item.Start();
        }
        await Task.Run(() =>
        {
            for (int a = 0; a < ConfigUtils.Config.Http.DownloadThread; a++)
            {
                semaphore.WaitOne();
            }
        });

        CoreMain.DownloaderUpdate?.Invoke(State = CoreRunState.End);

        return AllSize == DoneSize;
    }

    public static void FillAll(List<DownloadItem> list)
    {
        Logs.Info($"下载器装填内容");
        CoreMain.DownloaderUpdate?.Invoke(State = CoreRunState.Init);
        foreach (var item in list)
        {
            if (Name.Contains(item.Name))
                continue;
            CoreMain.DownloadItemStateUpdate?.Invoke(-1, item);
            item.Update = (index) =>
            {
                CoreMain.DownloadItemStateUpdate?.Invoke(index, item);
            };
            Items.Enqueue(item);
            Name.Add(item.Name);
        }
    }

    public static DownloadItem? GetItem()
    {
        if (Items.TryDequeue(out var item))
        {
            return item;
        }

        return null;
    }

    public static void Done()
    {
        DoneSize++;
    }

    public static void ThreadDone()
    {
        semaphore.Release();
    }

    public static void Error(int index, DownloadItem item, Exception e)
    {
        Logs.Error($"下载{item.Name}错误", e);
        CoreMain.DownloadItemError?.Invoke(index, item, e);
    }

    public static int GetCopyBufferSize(Stream stream)
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
        var data = await BaseClient.DownloadClient.GetAsync(item.Url, HttpCompletionOption.ResponseHeadersRead);
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
}
