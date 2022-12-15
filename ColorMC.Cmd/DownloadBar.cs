using ColorMC.Core;
using ColorMC.Core.Http.Downloader;
using ColorMC.Core.Utils;

namespace ColorMC.Cmd;

public static class DownloadBar
{
    private static DownloadItem[] Items1;
    private static ProgressBar Bar;

    public static void Init()
    {
        CoreMain.DownloadItemStateUpdate = DownloadUpdate;
        CoreMain.DownloaderUpdate = DownloaderUpdate;
    }

    public static void DownloaderUpdate(CoreRunState state)
    {
        if (state == CoreRunState.Start)
        {
            ConsoleUtils.Info("开始下载文件");
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Bar.Dispose();
        }
    }

    public static void DownloadUpdate(int index, DownloadItem item)
    {
        if (item.State == DownloadItemState.Done)
        {
            Items1[index] = null;
            Items1 = new DownloadItem[ConfigUtils.Config.Http.DownloadThread];
            Bar = new ProgressBar(ConfigUtils.Config.Http.DownloadThread);
            Bar.Done(index, $"{item.Name} 下载完成");
        }
        else if (item.State != DownloadItemState.Init)
        {
            Items1[index] = item;
            Bar.SetName(index, item.Name);
            Bar.SetAllSize(index, item.AllSize);
            Bar.SetValue(index, item.NowSize);
        }
    }
}
