using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace ColorMC.Test;

internal class Program
{
    public static void Run()
    {
        using HttpClient client = new HttpClient();
        //var data = await client.GetStringAsync("http://www.baidu.com");
        Console.WriteLine("Task");
    }

    public static void Test()
    {
        Run();

        Console.WriteLine("Run");
    }

    static void Main(string[] args)
    {
        Test();

        Console.WriteLine("Hello, World!");

        ColorMCCore.Init(new CoreInitArg
        {
            Local = AppContext.BaseDirectory
        });
        ColorMCCore.Init1();

        //ColorMCCore.GameLog += Log;

        //TestItem.Item43();

        GetSha1();

        Console.ReadLine();
    }

    public static void GetSha1()
    {
        var text = File.Exists("tmp/sha1.json")
            ? File.ReadAllText("tmp/sha1.json") : "{\"text\":\"\"}";
        var dir = AppContext.BaseDirectory;
        //var obj = JObject.Parse(text);
        //{
        //    using var file = File.OpenRead($"{dir}tmp/ColorMC.Core.dll");
        //    var sha1 = GenSha1(file); ;
        //    if (!obj.TryAdd("core.dll", sha1))
        //    {
        //        obj["core.dll"] = sha1;
        //    }
        //    Console.WriteLine($"ColorMC.Core.dll:{obj["core.dll"]}");
        //}
        //{
        //    using var file = File.OpenRead($"{dir}tmp/ColorMC.Gui.dll");
        //    var sha1 = GenSha1(file); ;
        //    if (!obj.TryAdd("gui.dll", sha1))
        //    {
        //        obj["gui.dll"] = sha1;
        //    }
        //    Console.WriteLine($"ColorMC.Gui.dll:{obj["gui.dll"]}");
        //}
        //File.WriteAllText($"{dir}tmp/sha1.json", obj.ToString(Formatting.Indented));
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

    public static Task<bool> Download(string state)
    {
        return Task.Run(() =>
        {
            Console.WriteLine("补全游戏文件");
            return true;
        });
    }

    public static void Log(GameSettingObj obj, string? log)
    {
        Logs.Info(log);
    }

    public static void Log(Process? progress, string? log)
    {
        Logs.Info(log);
    }

    //public static void Update(DownloadItemObj obj)
    //{
    //    Console.WriteLine($"下载项目:{DownloadManager.AllSize}/{DownloadManager.DoneSize} {obj.Name} {obj.NowSize}/{obj.AllSize}");
    //}

    public static void Update(int index, FileItemObj item)
    {
        if (item.State == DownloadItemState.Error)
        {
            Logs.Info($"下载器{index} 下载项目:{item.Name} 下载错误");
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