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
    }

    public static void AddItem(DownloadItem item)
    {
        Items.Enqueue(item);
    }

    public static DownloadItem GetItem()
    {
        if (Items.TryDequeue(out var item))
        {
            return item;
        }

        return null;
    }

    public static void Error(DownloadItem item, Exception e)
    {

    }
}
