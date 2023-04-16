using ColorMC.Core.Game;
using ColorMC.Core.Game.Auth;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Net.Download;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Net.Java;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using System.Diagnostics;

namespace ColorMC.Test;

public static class TestItem
{
    public static void Item1()
    {
        VersionPath.CheckUpdate("1.12.2").Wait();
        AssetsPath.Check("1.12.2").Wait();
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
            var list = GameDownload.Download(version.versions.Where(a => a.id == "1.12.2").First()).Result;
            if (list.State != GetDownloadState.End)
            {
                Console.WriteLine("下载列表获取失败");
                return;
            }
            DownloadManager.Start(list.List!).Wait();
        }
    }

    public static void Item3()
    {
        var list = PackDownload.DownloadCurseForgeModPack("H:\\ColonyVenture-1.13.zip", null, null).Result;
    }

    public static void Item4()
    {
        var res = FabricHelper.GetMeta().Result;
        if (res == null)
        {
            Console.WriteLine("Fabric信息为空");
        }
        else
        {
            var item = res.loader.First();
            var list = GameDownload.DownloadFabric("1.19.2", item.version).Result;
            if (list.State != GetDownloadState.End)
            {
                Console.WriteLine("下载列表获取失败");
                return;
            }
            DownloadManager.Start(list.List!).Wait();
        }
    }

    public static void Item5()
    {
        using FileStream stream2 = new("E:\\code\\ColorMC\\ColorMC.Test\\bin\\Debug\\net7.0\\minecraft\\assets\\objects\\0c\\0cd209ea16b052a2f445a275380046615d20775e", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        stream2.Seek(0, SeekOrigin.Begin);
        string sha1 = Funtcions.GenSha1(stream2);
    }

    public static void Item6()
    {
        var list = CurseForge.GetModPackList("1.16.5").Result;
        if (list == null)
        {
            Console.WriteLine("整合包信息为空");
        }
        else
        {
            var data = list.data[6];

            //var item2 = PackDownload.MakeCurseForge(data.latestFiles[0]);
            //DownloadManager.Start(new() { item2 }).Wait();

            //var list1 = PackDownload.DownloadCurseForge(item2.Local).Result;
            //if (list1.State != DownloadState.End)
            //{
            //    Console.WriteLine("下载列表获取失败");
            //    return;
            //}
            //DownloadManager.Start(list1.List!).Wait();
        }
    }

    public static void Item7()
    {
        var data = InstancesPath.Games.First();
        var list = Launch.CheckGameFile(data, new LoginObj()).Result;
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
        var login = BaseAuth.LoginWithOAuth().Result;
        if (login.State1 != LoginState.Done)
        {
            Console.WriteLine("登录错误");
        }
        else
        {
            var game = new GameSettingObj()
            {
                DirName = "test1",
                Name = "test1",
                Version = "1.12.2",
                Loader = Loaders.Forge,
                LoaderVersion = "14.23.5.2860"
            };
            var process = game.StartGame(login.Obj!).Result;
            process?.WaitForExit();
        }
    }

    public static void Item9()
    {
        var game = InstancesPath.Games;
        var login = new LoginObj()
        {
            UUID = "UUID",
            AccessToken = "AccessToken",
            UserName = "Test"
        };

        var process = game[0].StartGame(login).Result;
        process?.WaitForExit();
    }

    public static void Item10()
    {
        var login = new LoginObj()
        {
            UUID = "UUID",
            AccessToken = "AccessToken",
            UserName = "Test"
        };


        var game = new GameSettingObj()
        {
            DirName = "test1",
            Name = "test1",
            Version = "1.7.2",
            Loader = Loaders.Forge,
            LoaderVersion = "10.12.2.1161"
        };

        Console.ReadLine();

        Process? process;
        //process = game.StartGame(login).Result;
        //process?.WaitForExit();

        //game.Version = "1.7.10";
        //game.LoaderVersion = "10.13.4.1614";
        //process = game.StartGame(login).Result;
        //process?.WaitForExit();

        //game.Version = "1.8";
        //game.LoaderVersion = "11.14.4.1577";
        //process = game.StartGame(login).Result;
        //process?.WaitForExit();

        //game.Version = "1.8.8";
        //game.LoaderVersion = "11.15.0.1655";
        //process = game.StartGame(login).Result;
        //process?.WaitForExit();

        //game.Version = "1.8.9";
        //game.LoaderVersion = "11.15.1.2318";
        //process = game.StartGame(login).Result;
        //process?.WaitForExit();

        //game.Version = "1.9";
        //game.LoaderVersion = "12.16.1.1938";
        //process = game.StartGame(login).Result;
        //process?.WaitForExit();

        //game.Version = "1.9.4";
        //game.LoaderVersion = "12.17.0.2317";
        //process = game.StartGame(login).Result;
        //process?.WaitForExit();

        //game.Version = "1.10";
        //game.LoaderVersion = "12.18.0.2000";
        //process = game.StartGame(login).Result;
        //process?.WaitForExit();

        //game.Version = "1.10.2";
        //game.LoaderVersion = "12.18.3.2511";
        //process = game.StartGame(login).Result;
        //process?.WaitForExit();

        //game.Version = "1.11";
        //game.LoaderVersion = "13.19.1.2199";
        //process = game.StartGame(login).Result;
        //process?.WaitForExit();

        //game.Version = "1.11.2";
        //game.LoaderVersion = "13.20.1.2588";
        //process = game.StartGame(login).Result;
        //process?.WaitForExit();

        //game.Version = "1.12.2";
        //game.LoaderVersion = "14.23.4.2760";
        //process = game.StartGame(login).Result;
        //process?.WaitForExit();

        //game.Version = "1.12.2";
        //game.LoaderVersion = "14.23.5.2860";
        //process = game.StartGame(login).Result;
        //process?.WaitForExit();

        //game.Version = "1.13.2";
        //game.LoaderVersion = "25.0.223";
        //process = game.StartGame(login).Result;
        //process?.WaitForExit();

        //game.Version = "1.14.4";
        //game.LoaderVersion = "28.2.26";
        //process = game.StartGame(login).Result;
        //process?.WaitForExit();

        //game.Version = "1.15.2";
        //game.LoaderVersion = "31.2.57";
        //process = game.StartGame(login).Result;
        //process?.WaitForExit();

        //game.Version = "1.16.5";
        //game.LoaderVersion = "36.2.39";
        //process = game.StartGame(login).Result;
        //process?.WaitForExit();

        //game.Version = "1.17.1";
        //game.LoaderVersion = "37.1.1";
        //process = game.StartGame(login).Result;
        //process?.WaitForExit();

        //game.Version = "1.18.2";
        //game.LoaderVersion = "40.1.85";
        //process = game.StartGame(login).Result;
        //process?.WaitForExit();

        Stopwatch stopwatch = new();
        stopwatch.Start();

        game.Version = "1.19.2";
        game.LoaderVersion = "43.1.52";
        process = game.StartGame(login).Result;
        stopwatch.Stop();
        Console.WriteLine(stopwatch.Elapsed);

        process?.WaitForExit();
    }

    public static void Item11()
    {
        var login = BaseAuth.LoginWithNide8("f0930d6ac12f11ea908800163e095b49", "402067010@qq.com", "123456").Result;
        if (login.Obj == null)
        {
            Console.WriteLine("登录错误");
        }
        else
        {
            var game = new GameSettingObj()
            {
                DirName = "test1",
                Name = "test1",
                Version = "1.18.2",
                Loader = Loaders.Forge,
                LoaderVersion = "40.1.85"
            };
            var process = game.StartGame(login.Obj).Result;
            process?.WaitForExit();
        }
    }

    public static void Item12()
    {
        var games = InstancesPath.Games;
        var list = games[0].GetMods().Result;

        list[0].Disable();
        Console.ReadLine();
        list[0].Enable();
        Console.ReadLine();

        foreach (var item in list)
        {
            Console.WriteLine($"{item.V2} {item.modid} {item.name} {item.description}");
        }

        list = Mods.GetMods(games[1]).Result;

        foreach (var item in list)
        {
            Console.WriteLine($"{item.V2} {item.modid} {item.name} {item.description}");
        }

        list = Mods.GetMods(games[2]).Result;

        foreach (var item in list)
        {
            Console.WriteLine($"{item.V2} {item.modid} {item.name} {item.description}");
        }
    }

    public static void Item13()
    {
        var games = InstancesPath.Games;
        var list = Servers.GetServerInfos(games[0]);

        foreach (var item in list)
        {
            Console.WriteLine($"{item}");
        }
    }

    public static void Item14()
    {
        var games = InstancesPath.Games;
        var packs = games[0].GetResourcepacks().Result;

        foreach (var item in packs)
        {
            Console.WriteLine($"{item}");
        }

        //packs[0].Disable();
        Console.ReadLine();
        //packs[0].Enable();
    }

    public static void Item15()
    {
        var games = InstancesPath.Games;
        var packs = games[0].GetWorlds().Result;

        foreach (var item in packs)
        {
            Console.WriteLine($"{item}");
        }

    }

    public static void Item16()
    {
        var list = ForgeHelper.GetVersionList("1.12.2").Result;
        foreach (var item in list)
        {
            Console.Write(item + " ");
        }

        Console.WriteLine();

        var list1 = ForgeHelper.GetSupportVersion().Result;
        foreach (var item in list1)
        {
            Console.Write(item + " ");
        }

        Console.WriteLine();
    }

    public static void Item17()
    {
        var game = InstancesPath.Games[0];
        var list = game.GetWorlds().Result;
        list[0].ExportWorldZip("test.zip").Wait();
    }

    public static void Item18()
    {
        var motd = ServerMotd.GetServerInfo("color.coloryr.xyz", 25565).Result;

        Console.WriteLine(motd.State);
    }

    public static void Item19()
    {
        var res = OpenJ9.GetJavaList().Result;
    }

    public static void Item20()
    {
        var game = InstancesPath.GetGame("test");

        var list = game.GetSchematics().Result;
    }

    public static void Item21()
    {
        var list = Modrinth.GetModPackList().Result;
        var item = list.hits.First();
        var list1 = Modrinth.GetFileVersions(item.project_id, "", Loaders.Fabric).Result;
        var item1 = list1.First();

        InstancesPath.InstallFromModrinth(item1, null, null).Wait();
    }

    public static void Item22()
    {
        var list = OptifineHelper.GetOptifineVersion().Result;
        var data = OptifineHelper.GetOptifineDownloadUrl(new()
        {
            Url1 = "http://optifine.net/adloadx?f=preview_OptiFine_1.19.3_HD_U_I2_pre5.jar",
            Url2 = "http://optifine.net/adloadx?f=preview_OptiFine_1.19.3_HD_U_I2_pre5.jar"
        }).Result;
    }

    public static void Item23()
    {

    }
}
