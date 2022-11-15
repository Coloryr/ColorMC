using ColorMC.Core.Utils;
using System.Collections.Concurrent;

namespace ColorMC.Core.Http.Downloader;

public static class DownloadManager
{
    private readonly static ConcurrentQueue<DownloadItem> Items = new();
    private readonly static List<string> Name = new();
    private static List<DownloadThread> threads = new();
    private static Semaphore semaphore = new(0, 6);

    public static int AllSize { get; set; }
    public static int DoneSize { get; set; }

    public static void Init()
    {
        //ConfigUtils.Config.Http.DownloadThread = 1;
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

    public static void Clear()
    {
        Name.Clear();
        Items.Clear();
        AllSize = 0;
        DoneSize = 0;
    }

    public static async Task<bool> Start()
    {
        CoreMain.DownloadUpdate?.Invoke();
        CoreMain.DownloadState?.Invoke(CoreRunState.Start);
        AllSize = Items.Count;
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

        return AllSize == DoneSize;
    }

    public static void FillAll(List<DownloadItem> list)
    {
        foreach (var item in list)
        {
            if (Name.Contains(item.Name))
                continue;
            CoreMain.DownloadStateUpdate?.Invoke(item);
            item.Update = () =>
            {
                CoreMain.DownloadStateUpdate?.Invoke(item);
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
        CoreMain.DownloadUpdate?.Invoke();
    }

    public static void ThreadDone()
    {
        semaphore.Release();
    }

    public static void Error(DownloadItem item, Exception e)
    {
        Logs.Error($"下载{item.Name}错误", e);
        CoreMain.DownloadError?.Invoke(item, e);
    }
}
