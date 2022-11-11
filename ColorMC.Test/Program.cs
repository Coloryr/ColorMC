using ColorMC.Core;
using ColorMC.Core.Http.Download;
using ColorMC.Core.Path;
using ColorMC.Core.Utils;

namespace ColorMC.Test;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        CoreMain.Init(AppContext.BaseDirectory + "minecraft");

        CoreMain.DownloadUpdate = Update;
        CoreMain.DownloadStateUpdate = Update;

        var version = VersionPath.Versions;
        GameDownload.Download(version.versions.First()).Wait();

        DownloadManager.Start();

        Console.ReadLine();
    }


    public static void Update() 
    {
        Console.WriteLine($"下载项目:{DownloadManager.AllSize}/{DownloadManager.DoneSize}");
    }

    public static void Update(DownloadItem item)
    {
        Console.WriteLine($"下载项目:{item.Name} {item.AllSize}/{item.NowSize}");
    }
}