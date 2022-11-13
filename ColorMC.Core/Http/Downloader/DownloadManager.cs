using ColorMC.Core.Utils;
using System.Collections.Concurrent;

namespace ColorMC.Core.Http.Downloader;

public static class DownloadManager
{
    public readonly static ConcurrentQueue<DownloadItem> Items = new();
    private static List<DownloadThread> threads = new();
    private static Semaphore semaphore = new(0, 2);

    public static int AllSize { get; set; }
    public static int DoneSize { get; set; }

    public static void Init()
    {
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

    public static void Clear()
    {
        Items.Clear();
        AllSize = 0;
        DoneSize = 0;
    }

    public static Task Start()
    {
        CoreMain.DownloadUpdate?.Invoke();
        CoreMain.DownloadState?.Invoke(CoreRunState.Start);
        AllSize = Items.Count;
        foreach (var item in threads)
        {
            item.Start();
        }
        return Task.Run(() =>
        {
            semaphore.WaitOne();
        });
    }

    public static void AddItem(DownloadItem item)
    {
        item.Update = () =>
        {
            CoreMain.DownloadStateUpdate?.Invoke(item);
        };
        Items.Enqueue(item);
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
        CoreMain.DownloadUpdate?.Invoke();

        if (DoneSize == AllSize)
        {
            semaphore.Release();
        }
    }

    public static void Error(DownloadItem item, Exception e)
    {
        Logs.Error($"下载{item.Name}错误", e);
        CoreMain.DownloadError?.Invoke(item, e);
    }
}
