using ColorMC.Core;
using ColorMC.Core.Game.Auth;
using ColorMC.Core.Http.Downloader;
using ColorMC.Core.Objs;
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
        CoreMain.LoginOAuthCode = Login;
        CoreMain.AuthStateUpdate = AuthStateUpdate;
        CoreMain.LoginFail = LoginFail;

        TestItem.Item11();

        Console.ReadLine();
    }

    public static void LoginFail(LoginState state, string msg)
    {
        Console.WriteLine($"登录失败{state} {msg}");
    }

    public static void AuthStateUpdate(AuthState state)
    {
        Console.WriteLine($"登录状态{state}");
    }

    public static void Login(string url, string code)
    {
        Console.WriteLine(url);
        Console.WriteLine(code);
    }

    public static bool Download(GameSettingObj obj)
    {
        Console.WriteLine("补全游戏文件");
        return true;
    }

    public static void Log(Process? progress, string? log) 
    {
        Console.WriteLine(log);
    }

    public static void Update1(CoreRunState item)
    { 
        
    }

    public static void Update(int a) 
    {
        Console.WriteLine($"下载器{a} 下载项目:{DownloadManager.AllSize}/{DownloadManager.DoneSize}");
    }

    public static void Update(int index, DownloadItem item)
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