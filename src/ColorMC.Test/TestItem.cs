using Ae.Dns.Client;
using Ae.Dns.Protocol;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Net.Motd;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using ColorMC.Gui.MusicPlayer;
using ColorMC.Gui.Utils;
using SkiaSharp;
using System.IO.Compression;
using CoreHttpClient = ColorMC.Core.Net.CoreHttpClient;

namespace ColorMC.Test;

public static class TestItem
{
    private static DesktopGameHandel? Start(GameSettingObj obj, LoginObj obj1)
    {
        return obj.StartGameAsync(new GameLaunchArg 
        { 
            Auth = obj1 
        }, CancellationToken.None).Result as DesktopGameHandel;
    }

    public static void Item1()
    {
        VersionPath.CheckUpdateAsync("1.12.2").Wait();
        //AssetsPath.Check("1.12.2").Wait();
    }

    public static void Item2()
    {
        var version = VersionPath.GetVersionsAsync().Result;
        if (version == null)
        {
            Console.WriteLine("版本信息为空");
        }
        else
        {
            //GameDownload.Download(version.versions.First()).Wait();
            //var list = DownloadItemHelper.BuildVersionDownloadAsync(version.Versions.Where(a => a.Id == "1.12.2").First()).Result;
            //if (list == null)
            //{
            //    Console.WriteLine("下载列表获取失败");
            //    return;
            //}
            //DownloadManager.StartAsync(list).Wait();
        }
    }

    public static void Item3()
    {
        var list = ModPackHelper.InstallCurseForgeModPackAsync(new InstallModPackZipArg
        {
            Zip = File.OpenRead("H:\\ColonyVenture-1.13.zip"),
            Request = Program.Download,
            Overwirte = (_) => Task.FromResult(true)
        }).Result;
    }

    public static void Item4()
    {
        var res = FabricAPI.GetMeta().Result;
        if (res == null)
        {
            Console.WriteLine("Fabric信息为空");
        }
        else
        {
            var item = res.Loader.First();
            //var list = DownloadItemHelper.BuildFabricAsync("1.19.2", item.version).Result;
            //if (list == null)
            //{
            //    Console.WriteLine("下载列表获取失败");
            //    return;
            //}
            //DownloadManager.StartAsync(list).Wait();
        }
    }

    public static void Item5()
    {
        using FileStream stream2 = new("E:\\code\\ColorMC\\ColorMC.Test\\bin\\Debug\\net7.0\\minecraft\\assets\\objects\\0c\\0cd209ea16b052a2f445a275380046615d20775e", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        stream2.Seek(0, SeekOrigin.Begin);
        string sha1 = HashHelper.GenSha1(stream2);
    }

    public static void Item6()
    {
        var list = CurseForgeAPI.GetModPackList("1.16.5").Result;
        if (list == null)
        {
            Console.WriteLine("整合包信息为空");
        }
        else
        {
            var data = list.Data[6];

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
        //var data = InstancesPath.Games.First();
        //var list = CheckHelpers.CheckGameFileAsync(data, CancellationToken.None).Result;
        //if (list == null)
        //{
        //    Console.WriteLine("文件检查失败");
        //}
        //else
        //{
        //    foreach (var item in list)
        //    {
        //        Console.WriteLine($"文件丢失:{item.Name}");
        //    }
        //}
    }

    public static void Item8()
    {
        var login = GameAuth.LoginOAuthAsync((_, _, _) => { }).Result;
        if (login.LoginState != LoginState.Done)
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
            var process = Start(game, login.Auth!);
            process?.Process.WaitForExit();
        }
    }

    public static void Item9()
    {
        var game = InstancesPath.Games;
        var login = new LoginObj()
        {
            UUID = Guid.NewGuid().ToString().ToLower(),
            AccessToken = "AccessToken",
            UserName = "Test"
        };

        var process = Start(game[0], login);
        process?.Process.WaitForExit();
    }

    public static void Item10()
    {
        var login = new LoginObj()
        {
            UUID = Guid.NewGuid().ToString().ToLower(),
            AccessToken = "AccessToken",
            UserName = "Test"
        };


        var game = new GameSettingObj()
        {
            DirName = "test1",
            Name = "test1",
            Loader = Loaders.Forge
        };

        CancellationToken token = CancellationToken.None;

        CoreHttpClient.Source = SourceLocal.Offical;

        DesktopGameHandel? process;
        //process = game.StartGame(login).Result;
        //process?.WaitForExit();

        //game.Version = "1.7.10";
        //game.LoaderVersion = "10.13.4.1614";
        //process = game.StartGameAsync(login, null, token).Result;
        //process?.WaitForExit();

        //game.Version = "1.8";
        //game.LoaderVersion = "11.14.4.1577";
        //process = game.StartGame(login, null, token).Result;
        //process?.WaitForExit();

        //game.Version = "1.8.8";
        //game.LoaderVersion = "11.15.0.1655";
        //process = game.StartGame(login, null, token).Result;
        //process?.WaitForExit();

        //game.Version = "1.8.9";
        //game.LoaderVersion = "11.15.1.2318";
        //process = game.StartGame(login, null, token).Result;
        //process?.WaitForExit();

        //game.Version = "1.9";
        //game.LoaderVersion = "12.16.1.1938";
        //process = game.StartGame(login, null, token).Result;
        //process?.WaitForExit();

        //game.Version = "1.9.4";
        //game.LoaderVersion = "12.17.0.2317";
        //process = game.StartGameAsync(login, null, token).Result;
        //process?.WaitForExit();

        //game.Version = "1.10";
        //game.LoaderVersion = "12.18.0.2000";
        //process = game.StartGame(login, null, token).Result;
        //process?.WaitForExit();

        //game.Version = "1.10.2";
        //game.LoaderVersion = "12.18.3.2511";
        //process = game.StartGame(login, null, token).Result;
        //process?.WaitForExit();

        //game.Version = "1.11";
        //game.LoaderVersion = "13.19.1.2199";
        //process = game.StartGame(login, null, token).Result;
        //process?.WaitForExit();

        //game.Version = "1.11.2";
        //game.LoaderVersion = "13.20.1.2588";
        //process = game.StartGameAsync(login, null, token).Result;
        //process?.WaitForExit();

        //game.Version = "1.12.2";
        //game.LoaderVersion = "14.23.4.2760";
        //process = game.StartGameAsync(login, null, token).Result;
        //process?.WaitForExit();

        //game.Version = "1.12.2";
        //game.LoaderVersion = "14.23.5.2860";
        //process = game.StartGameAsync(login, null, token).Result;
        //process?.WaitForExit();

        //game.Version = "1.13.2";
        //game.LoaderVersion = "25.0.223";
        //process = game.StartGameAsync(login, null, token).Result;
        //process?.WaitForExit();

        //game.Version = "1.14.4";
        //game.LoaderVersion = "28.2.26";
        //process = game.StartGame(login, null, token).Result;
        //process?.WaitForExit();

        //game.Version = "1.15.2";
        //game.LoaderVersion = "31.2.57";
        //process = game.StartGame(login, null, token).Result;
        //process?.WaitForExit();

        //game.Version = "1.16.5";
        //game.LoaderVersion = "36.2.39";
        //process = game.StartGame(login, null, token).Result;
        //process?.WaitForExit();

        //game.Version = "1.17.1";
        //game.LoaderVersion = "37.1.1";
        //process = game.StartGameAsync(login, null, token).Result;
        //process?.WaitForExit();

        //game.Version = "1.18.2";
        //game.LoaderVersion = "40.1.85";
        //process = game.StartGameAsync(login, null, token).Result;
        //process?.WaitForExit();

        //game.Version = "1.19.2";
        //game.LoaderVersion = "43.1.52";
        //process = game.StartGameAsync(login, null, token).Result;
        //process?.WaitForExit();

        //game.Version = "1.20.1";
        //game.Loader = Loaders.Forge;
        //game.LoaderVersion = "47.2.18";
        //process = game.StartGameAsync(login, null, token).Result;
        //process?.WaitForExit();

        //game.Version = "1.20.1";
        //game.Loader = Loaders.NeoForge;
        //game.LoaderVersion = "47.1.84";
        //process = game.StartGameAsync(login, null, token).Result;
        //process?.WaitForExit();

        //game.Version = "1.20.1";
        //game.Loader = Loaders.Fabric;
        //game.LoaderVersion = "0.14.22";
        //process = game.StartGameAsync(login, null, token).Result;
        //process?.WaitForExit();

        //game.Version = "1.20.1";
        //game.Loader = Loaders.Quilt;
        //game.LoaderVersion = "0.22.0";
        //process = game.StartGameAsync(login, null, token).Result;
        //process?.WaitForExit();

        game.Version = "1.20.4";
        game.Loader = Loaders.Forge;
        game.LoaderVersion = "49.0.11";
        process = Start(game, login);
        process?.Process.WaitForExit();

        game.Version = "1.20.4";
        game.Loader = Loaders.NeoForge;
        game.LoaderVersion = "20.4.50-beta";
        process = Start(game, login);
        process?.Process.WaitForExit();

    }

    public static void Item11()
    {
        var login = GameAuth.LoginNide8Async("f0930d6ac12f11ea908800163e095b49", "402067010@qq.com", "123456").Result;
        if (login.Auth == null)
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
            var process = Start(game, login.Auth);
            process?.Process.WaitForExit();
        }
    }

    public static void Item12()
    {
        var games = InstancesPath.Games;
        var list = games[0].GetModsAsync(false).Result;

        list[0].Disable();
        Console.ReadLine();
        list[0].Enable();
        Console.ReadLine();

        foreach (var item in list)
        {
            Console.WriteLine($"{item.ModId} {item.Name} {item.Description}");
        }

        list = Mods.GetModsAsync(games[1], false).Result;

        foreach (var item in list)
        {
            Console.WriteLine($"{item.ModId} {item.Name} {item.Description}");
        }

        list = Mods.GetModsAsync(games[2], false).Result;

        foreach (var item in list)
        {
            Console.WriteLine($"{item.ModId} {item.Name} {item.Description}");
        }
    }

    public static async void Item13()
    {
        var games = InstancesPath.Games;
        var list = await Servers.GetServerInfosAsync(games[0]);

        foreach (var item in list)
        {
            Console.WriteLine($"{item}");
        }
    }

    public static void Item14()
    {
        var games = InstancesPath.Games;
        var packs = games[0].GetResourcepacksAsync(false).Result;

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
        var packs = games[0].GetWorldsAsync().Result;

        foreach (var item in packs)
        {
            Console.WriteLine($"{item}");
        }

    }

    public static void Item16()
    {
        var list = ForgeAPI.GetVersionList(false, "1.12.2").Result;
        foreach (var item in list!)
        {
            Console.Write(item + " ");
        }

        Console.WriteLine();

        var list1 = ForgeAPI.GetSupportVersion(false).Result;
        foreach (var item in list1!)
        {
            Console.Write(item + " ");
        }

        Console.WriteLine();
    }

    public static void Item17()
    {
        var game = InstancesPath.Games[0];
        var list = game.GetWorldsAsync().Result;
        list[0].ExportWorldZip("test.zip").Wait();
    }

    public static void Item18()
    {
        var motd = ServerMotd.GetServerInfo("color.coloryr.xyz", 25565).Result;

        Console.WriteLine(motd.State);
    }

    public static void Item19()
    {
        var res = OpenJ9Api.GetJavaList().Result;
    }

    public static void Item20()
    {
        var game = InstancesPath.GetGame("test");

        var list = game!.GetSchematicsAsync();
    }

    public static void Item21()
    {
        var list = ModrinthAPI.GetModPackList().Result;
        var item = list!.Hits.First();
        var list1 = ModrinthAPI.GetFileVersions(item.ProjectId, "", Loaders.Fabric).Result;
        var item1 = list1!.First();

        AddGameHelper.InstallModrinth(new DownloadModrinthArg
        {
            Data = item1,
            Data1 = item,
            Request = Program.Download,
            Overwirte = (_) => Task.FromResult(true),
        }).Wait();
    }

    public static void Item22()
    {
        var list = OptifineAPI.GetOptifineVersion().Result;
        var data = OptifineAPI.GetOptifineDownloadUrl(new()
        {
            Url1 = "http://optifine.net/adloadx?f=preview_OptiFine_1.19.3_HD_U_I2_pre5.jar",
            Url2 = "http://optifine.net/adloadx?f=preview_OptiFine_1.19.3_HD_U_I2_pre5.jar"
        }).Result;
    }

    public static void Item23()
    {
        var list = JavaHelper.FindJava();
    }

    public static void Item24()
    {
        var list = ColorMCAPI.GetMcMod("魔法", 0, Loaders.Normal, "1.12.2", "", 0).Result;
    }

    public static void Item25()
    {
        var game = InstancesPath.GetGameByName("test");
        var list = game.GetLogFiles();
        var data = game.ReadLog(list.First());
    }

    public static void Item26()
    {
        var list = ForgeAPI.GetSupportVersion(false).Result;
    }

    public static void Item27()
    {
        //var data = ChunkMca.Read(@"F:\minecraft\ColorMC\minecraft\instances\game\.minecraft\saves\新的世界\region\r.-2.1.mca").Result;
        //ChunkMca.Write(data, "r.-2.1.mca");

        {
            using var temp = File.Open(@"F:\minecraft\ColorMC\minecraft\instances\game\.minecraft\saves\新的世界\region\r.-2.1.mca", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            temp.Seek(0x6005, SeekOrigin.Begin);
            var temp1 = new byte[0x168];
            temp.ReadExactly(temp1);
            using var temp2 = new ZLibStream(new MemoryStream(temp1), CompressionMode.Decompress);
            MemoryStream decompressed = new MemoryStream();
            temp2.CopyTo(decompressed);
            File.WriteAllBytes("a.data", decompressed.ToArray());
        }
        {
            using var temp = File.Open("r.-2.1.mca", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            temp.Seek(0x2005, SeekOrigin.Begin);
            var temp1 = new byte[0x16F];
            temp.ReadExactly(temp1);
            using var temp2 = new ZLibStream(new MemoryStream(temp1), CompressionMode.Decompress);
            MemoryStream decompressed = new MemoryStream();
            temp2.CopyTo(decompressed);
            File.WriteAllBytes("2.data", decompressed.ToArray());
        }

    }

    public static void Item28()
    {
        string temp = "\"$INST_JAVA\" -jar packwiz-installer-bootstrap.jar --bootstrap-main-jar packwiz-installer.jar --bootstrap-no-update \"https://archive.teacon.cn/2023/packwiz/prod/pack.toml\"";
        var list = StringHelper.ArgParse(temp);
    }

    public static void Item29()
    {
        byte a = 0x98;
        string temp = StringHelper.ToHex(a);
        short b = 0x18A8;
        temp = StringHelper.ToHex(b);
        int c = 0x18A66098;
        temp = StringHelper.ToHex(c);
        long d = 0x18A6609aaa8;
        temp = StringHelper.ToHex(d);
    }

    public static void Item30()
    {
        string temp = "H:\\jre17-arm64-20230721-release.tar.gz";
        new ZipUtils().UnzipAsync("H:\\jre17", temp, File.OpenRead(temp)).Wait();
    }

    public static void Item31()
    {
        VersionPath.GetFromWebAsync().Wait();
        var game = new GameSettingObj()
        {
            DirName = "test1",
            Name = "test1",
            Loader = Loaders.Forge
        };
        game.Version = "1.20.1";
        game.Loader = Loaders.NeoForge;
        game.LoaderVersion = "47.1.76";
        //game.MakeInstallForgeArg(true);
    }

    public static void Item32()
    {
        var obj = VersionPath.GetNeoForgeInstallObj("1.20.1", "47.1.76")!;
        //var res = CheckHelpers.CheckForgeInstall(obj, "47.1.76", true);
    }

    public static void Item33()
    {
        var client = new LanClient
        {
            FindLan = (motd, ip, port) =>
            {
                Console.WriteLine($"发现服务器 {ip} {port} {motd}");
            }
        };
        var server = new LanServer("25565", "测试服务器");
    }

    public static void Item34()
    {
        //var regex = StringHelper.VersionRegex();
        //var version = regex.Match("15.0.1");
        //Version version1;
        //version1 = new Version(version.Groups[0].Value + version.Groups[1].Value);

        //version = regex.Match("0.10.6+build.214");
        //version1 = new Version(version.Groups[0].Value + version.Groups[1].Value);
    }

    public static void Item35()
    {
        SdlUtils.InitTest();

        Media.Init();
        Media.PlayMusic("F:\\music\\AstroNoteS\\空飛ぶ旅 - AstroNoteS.mp3", false, 100);
    }
    public static void Item36()
    {
        //var image = Skin3DHeadB.MakeHeadImage(SKBitmap.Decode("D:\\skin\\Skins\\color_yr.png"));
        //using var stream = File.OpenWrite("output.png");
        //image.CopyTo(stream);
    }

    public static void Item37()
    {
        //SystemInfo.Os = OsType.Linux;
        //SystemInfo.IsArm = true;

        //var list = CheckHelpers.CheckGameFileAsync(new()
        //{ 
        //    Version = "1.21.1",
        //    DirName = "test1",
        //    Name = "test1"
        //}, CancellationToken.None).Result;
    }

    public static void Item38()
    {
        //using var cloudFlare1 = new DnsUdpClient(IPAddress.Parse("1.1.1.1"));
        //using var cloudFlare2 = new DnsUdpClient(IPAddress.Parse("1.0.0.1"));
        //using var google1 = new DnsUdpClient(IPAddress.Parse("8.8.8.8"));
        //using var google2 = new DnsUdpClient(IPAddress.Parse("8.8.4.4"));

        //// Construct the race: you must pass at least two clients
        //using var dnsClient = new DnsRacerClient(cloudFlare1, cloudFlare2, google1, google2);

        using var httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://223.5.5.5/dns-query")
        };

        using var dnsClient = new DnsHttpClient(httpClient);

        // Create an HttpClient with the DnsDelegatingHandler
        using var httpClient1 = new HttpClient(new DnsDelegatingHandler(dnsClient)
        {
            InnerHandler = new SocketsHttpHandler()
        });

        // Make a request to GET www.google.com using the DNS middleware
        HttpResponseMessage response = httpClient1.GetAsync("https://www.github.com/").Result;

        Console.WriteLine(response.StatusCode);
    }

    public static void Item39()
    {
        SdlUtils.InitTest();

        Media.Init();
        //16
        Media.PlayMusic("F:\\music\\Famishin\\リフレイン ~Piano Version~ - Famishin.flac", false, 100);

        //24
        //Media.PlayMusic("F:\\music\\777☆SISTERS\\Departures -あしたの歌- - 777☆SISTERS.flac", false ,100);
    }

    public static void Item40()
    {
        SdlUtils.InitTest();

        Media.Init();
        //16
        //Media.PlayMusic("F:\\music\\9-tie\\Naked Blue - 9-tie.flac", false, 100);

        //24
        Media.PlayMusic("F:\\music\\777☆SISTERS\\Departures -あしたの歌- - 777☆SISTERS.flac", false, 100);
    }

    public static async void Item41()
    {
        var handler = new HttpClientHandler
        {
            //SslProtocols = SslProtocols.Tls13,

            //ServerCertificateCustomValidationCallback = 
            //    (message, cert, chain, errors) => 
            //    {
            //        return true;
            //    } // 仅用于开发，生产中请进行适当的验证
        };

        using var client = new HttpClient(handler);

        //client.DefaultRequestVersion = HttpVersion.Version11;
        //client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;

        try
        {
            var response = await client.GetByteArrayAsync("https://maven.fabricmc.net/org\\ow2\\asm\\asm\\9.7.1\\asm-9.7.1.jar");
            Console.WriteLine(response);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Request error: {e.Message}");
            if (e.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {e.InnerException.Message}");
                if (e.InnerException.InnerException != null)
                {
                    Console.WriteLine($"Inner exception's inner exception: {e.InnerException.InnerException.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General error: {ex.Message}");
        }
    }

    public static void Item42()
    {
        char c = (char)0x3F;
        var v1 = char.IsControl(c);
        var v2 = char.IsLetter(c);
        var v3 = char.IsPunctuation(c);
        var v4 = char.IsSeparator(c);
        var v5 = char.IsSurrogate(c);
        var v6 = char.IsSymbol(c);

    }

    public static async Task Item43()
    {
        using var temp = new HttpClient();
        var res = await temp.GetAsync("https://edge.forgecdn.net/files/4351/224/wolf-armor-compat-1.0.3-1.19.2.jar", HttpCompletionOption.ResponseHeadersRead);
        var data = res.Content.ReadAsStream();
    }
}