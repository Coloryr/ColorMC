using ColorMC.Core;
using ColorMC.Core.Http;
using ColorMC.Core.Http.Download;
using ColorMC.Core.Http.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Path;

namespace ColorMC.Test;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        CoreMain.Init(AppContext.BaseDirectory);

        CoreMain.DownloadUpdate = Update;
        CoreMain.DownloadStateUpdate = Update;
        CoreMain.GameOverwirte = Overwirte;
        CoreMain.PackState = Update;
        CoreMain.PackUpdate = PackUpdate;

        //var version = VersionPath.Versions;
        //GameDownload.Download(version.versions.First()).Wait();
        //DownloadManager.Start();

        //PackDownload.DownloadCurseForge("H:\\stoneBlock-1.0.37.zip").Wait();

        //var res = Get.GetFabricMeta().Result;
        //var item = res.loader.First();
        //GameDownload.DownloadFabric("1.19.2", item.version ).Wait();
        //DownloadManager.Start();

        //using FileStream stream2 = new("E:\\code\\ColorMC\\ColorMC.Test\\bin\\Debug\\net7.0\\minecraft\\assets\\objects\\0c\\0cd209ea16b052a2f445a275380046615d20775e", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        //stream2.Seek(0, SeekOrigin.Begin);
        //string sha1 = Sha1.GenSha1(stream2);

        var list = Get.GetCurseForge().Result;
        var data = list.data.First();
        PackDownload.DownloadCurseForge(data).Wait();
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

    public static void Update(CoreRunState item)
    {
        Console.WriteLine($"整合包状态:{item}");
    }

    public static void PackUpdate(int a, int b) 
    {
        Console.WriteLine($"整合包信息获取:{a}/{b}");
    }

    public static bool Overwirte(GameSetting setting)
    {
        return true;
    }
}