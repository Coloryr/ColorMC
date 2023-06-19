using ColorMC.Core;
using ColorMC.Core.Downloader;
using ColorMC.Core.Objs;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace ColorMC.Test;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        ColorMCCore.Init(AppContext.BaseDirectory);

        ColorMCCore.DownloaderUpdate = Update;
        ColorMCCore.DownloadItemStateUpdate = Update;
        ColorMCCore.GameDownload = Download;
        ColorMCCore.GameOverwirte = Overwirte;
        ColorMCCore.PackState = Update;
        ColorMCCore.PackUpdate = PackUpdate;
        ColorMCCore.ProcessLog = Log;
        ColorMCCore.LoginOAuthCode = Login;
        ColorMCCore.AuthStateUpdate = AuthStateUpdate;
        ColorMCCore.GameLog = Log;

        TestItem.Item25();

        //GetSha1();

        Console.ReadLine();
    }

    public static void GetSha1()
    {
        {
            using var file = File.OpenRead($"tmp/ColorMC.Core.dll");
            Console.WriteLine($"ColorMC.Core.dll:{GenSha1(file)}");
        }
        {
            using var file = File.OpenRead($"tmp/ColorMC.Core.pdb");
            Console.WriteLine($"ColorMC.Core.pdb:{GenSha1(file)}");
        }
        {
            using var file = File.OpenRead($"tmp/ColorMC.Gui.dll");
            Console.WriteLine($"ColorMC.Gui.dll:{GenSha1(file)}");
        }
        {
            using var file = File.OpenRead($"tmp/ColorMC.Gui.pdb");
            Console.WriteLine($"ColorMC.Gui.pdb:{GenSha1(file)}");
        }
    }

    public static string GenSha1(Stream stream)
    {
        SHA1 sha1 = SHA1.Create();
        StringBuilder EnText = new();
        foreach (byte iByte in sha1.ComputeHash(stream))
        {
            EnText.AppendFormat("{0:x2}", iByte);
        }
        return EnText.ToString().ToLower();
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

    public static Task<bool> Download(LaunchState state, GameSettingObj obj)
    {
        return Task.Run(() =>
        {
            Console.WriteLine("补全游戏文件");
            return true;
        });
    }

    public static void Log(GameSettingObj obj, string? log)
    {
        Console.WriteLine(log);
    }

    public static void Log(Process? progress, string? log)
    {
        Console.WriteLine(log);
    }

    public static void Update(int index)
    {
        Console.WriteLine($"下载器{index} 下载项目:{DownloadManager.AllSize}/{DownloadManager.DoneSize}");
    }

    public static void Update(int index, DownloadItemObj item)
    {
        if (item.State == DownloadItemState.Error)
        {
            Console.WriteLine($"下载器{index} 下载项目:{item.Name} 下载错误");
        }
    }

    public static void Update(CoreRunState item)
    {
        Console.WriteLine($"整合包状态:{item}");
    }

    public static void PackUpdate(int a, int b)
    {
        Console.WriteLine($"整合包信息获取:{a}/{b}");
    }

    public static Task<bool> Overwirte(GameSettingObj setting)
    {
        return Task.Run(() =>
        {
            return true;
        });
    }
}