using ColorMC.Core.Config;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Http.Download;

public static class DownloadManager
{
    public readonly static ConcurrentQueue<DownloadItem> Items = new();
    private static List<DownloadThread> threads = new();

    public static int AllSize { get; set; }
    public static int DoneSize { get; set; }

    public static void Init()
    {
        threads.ForEach(a => a.Close());
        threads.Clear();
        ConfigUtils.Config.Http.DownloadThread = 1;
        for (int a = 0; a < ConfigUtils.Config.Http.DownloadThread; a++)
        {
            DownloadThread thread = new();
            thread.Init(a);
            threads.Add(thread);
        }
    }

    public static void Clear() 
    {
        Items.Clear();
        AllSize = 0;
        DoneSize = 0;
    }

    public static void Start() 
    {
        CoreMain.DownloadUpdate?.Invoke();
        CoreMain.DownloadState?.Invoke(DownloadState.Start);
        foreach (var item in threads)
        {
            item.Start();
        }
    }

    public static void AddItem(DownloadItem item)
    {
        item.Update = () =>
        {
            CoreMain.DownloadStateUpdate?.Invoke(item);
        };
        Items.Enqueue(item);
        AllSize++;
    }

    public static DownloadItem? GetItem()
    {
        if (Items.TryDequeue(out var item))
        {
            return item;
        }

        return null;
    }

    public static void Done(DownloadItem item) 
    {
        DoneSize++;
        CoreMain.DownloadUpdate?.Invoke();
    }

    public static void Error(DownloadItem item, Exception e)
    {

    }
}
