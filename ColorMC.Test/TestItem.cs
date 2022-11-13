using ColorMC.Core.Game;
using ColorMC.Core.Http;
using ColorMC.Core.Http.Download;
using ColorMC.Core.Http.Downloader;
using ColorMC.Core.Http.MoJang;
using ColorMC.Core.Login;
using ColorMC.Core.Objs.Pack;
using ColorMC.Core.Path;
using ColorMC.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Test;

public static class TestItem
{
    public static void Item1() 
    {
        VersionPath.CheckUpdate("1.12.2").Wait();
        AssetsPath.CheckUpdate("1.12.2").Wait();
    }

    public static void Item2()
    {
        var version = VersionPath.Versions;
        if (version == null)
        {
            Console.WriteLine("版本信息为空");
        }
        else
        {
            //GameDownload.Download(version.versions.First()).Wait();
            GameDownload.Download(version.versions.Where(a=>a.id == "1.12.2").First()).Wait();
            DownloadManager.Start();
        }
    }

    public static void Item3()
    {
        PackDownload.DownloadCurseForge("H:\\ColonyVenture-1.13.zip").Wait();
        DownloadManager.Start().Wait();
    }

    public static void Item4()
    {
        var res = Get.GetFabricMeta().Result;
        if (res == null)
        {
            Console.WriteLine("Fabric信息为空");
        }
        else
        {
            var item = res.loader.First();
            GameDownload.DownloadFabric("1.19.2", item.version).Wait();
            DownloadManager.Start();
        }
    }

    public static void Item5() 
    {
        using FileStream stream2 = new("E:\\code\\ColorMC\\ColorMC.Test\\bin\\Debug\\net7.0\\minecraft\\assets\\objects\\0c\\0cd209ea16b052a2f445a275380046615d20775e", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        stream2.Seek(0, SeekOrigin.Begin);
        string sha1 = Sha1.GenSha1(stream2);
    }

    public static void Item6()
    {
        var list = Get.GetCurseForge("1.16.5").Result;
        if (list == null)
        {
            Console.WriteLine("整合包信息为空");
        }
        else
        {
            var data = list.data[6];
            PackDownload.DownloadCurseForge(data).Wait();
            DownloadManager.Start();
        }
    }

    public static void Item7()
    {
        var data = InstancesPath.GetGames().First();
        var list = Launch.CheckGameFile(data).Result;
        if (list == null)
        {
            Console.WriteLine("文件检查失败");
        }
        else
        {
            foreach (var item in list)
            {
                Console.WriteLine($"文件丢失:{item.Name}");
            }
        }
    }

    public static void Item8()
    {
        var data = Auth.LoginWithOAuth().Result;
        if (data == null)
        {
            Console.WriteLine("登录错误");
        }
        else
        {
            var data1 = APIs.GetMinecraftProfileAsync(data.Token).Result;
            Console.WriteLine(data);
            Console.WriteLine(JsonConvert.SerializeObject(data1));
        }
    }

    public static void Item9() 
    {
        var game = InstancesPath.GetGames();
        foreach (var item in game)
        {
            string temp = Launch.MakeV2JvmArg(item);
            Console.WriteLine(temp);
        }
    }

    public static void Item10() 
    {
        //{Files { projectID = 556490, fileID = 3959686, required = True }}
        var item = new CurseForgePackObj.Files()
        { 
            projectID = 556490,
            fileID = 3959686
        };
        var res = Get.GetCurseForgeMod(item).Result;

        Console.WriteLine();
    }
}
