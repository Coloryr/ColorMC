using ColorMC.Core;
using ColorMC.Core.Http;
using ColorMC.Core.Http.Download;
using ColorMC.Core.Http.Downloader;
using ColorMC.Core.Http.Login;
using ColorMC.Core.Http.Apis;
using ColorMC.Core.Login;
using ColorMC.Core.Objs;
using ColorMC.Core.LaunchPath;
using Newtonsoft.Json;
using System.Diagnostics;

namespace ColorMC.Test;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        CoreMain.Init(AppContext.BaseDirectory);

        CoreMain.DownloadUpdate = Update;
        CoreMain.DownloadState = Update1;
        CoreMain.DownloadStateUpdate = Update;
        CoreMain.GameDownload = Download;
        CoreMain.GameOverwirte = Overwirte;
        CoreMain.PackState = Update;
        CoreMain.PackUpdate = PackUpdate;
        CoreMain.ProcessLog = Log;

        TestItem.Item10();

        Console.ReadLine();
    }

    public static bool Download(GameSettingObj obj)
    {
        return true;
    }

    public static void Log(Process? progress, string? log) 
    {
        Console.WriteLine(log);
    }

    public static void Update1(CoreRunState item)
    { 
        
    }

    public static void Update() 
    {
        Console.WriteLine($"下载项目:{DownloadManager.AllSize}/{DownloadManager.DoneSize}");
    }

    public static void Update(DownloadItem item)
    {
        //Console.WriteLine($"下载项目:{item.Name} {item.AllSize}/{item.NowSize}");
    }

    public static void Update(CoreRunState item)
    {
        Console.WriteLine($"整合包状态:{item}");
    }

    public static void PackUpdate(int a, int b) 
    {
        Console.WriteLine($"整合包信息获取:{a}/{b}");
    }

    public static bool Overwirte(GameSettingObj setting)
    {
        return true;
    }
}