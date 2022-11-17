using ColorMC.Core.Http;
using ColorMC.Core.Http.Download;
using ColorMC.Core.Http.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Game;
using ColorMC.Core.Utils;
using System;
using System.Net.Mime;

namespace ColorMC.Core.LaunchPath;

public static class LibrariesPath
{
    private const string Name = "libraries";
    public static string BaseDir { get; private set; }
    private static string NativeDir;

    public static void Init(string dir)
    {
        BaseDir = dir + "/" + Name;
        NativeDir = $"{BaseDir}/native-{SystemInfo.Os}-{SystemInfo.SystemArch}".ToLower();

        Directory.CreateDirectory(BaseDir);
        Directory.CreateDirectory(NativeDir);
    }

    public static string GetNativeDir(string version) 
    {
        string dir = $"{NativeDir}/{version}";
        Directory.CreateDirectory(dir);
        return dir;
    }

    public static List<DownloadItem> CheckGame(GameArgObj obj)
    {
        var list = new List<DownloadItem>();

        var list1 = GameHelp.MakeGameLibs(obj);
        foreach (var item in list1)
        {
            if (!File.Exists(item.Local))
            {
                list.Add(item);
                continue;
            }
            using var stream = new FileStream(item.Local, FileMode.Open, FileAccess.Read,
                FileShare.Read);
            var sha1 = Sha1.GenSha1(stream);
            if (item.SHA1 != sha1)
            {
                list.Add(item);
            }
            item.Later?.Invoke(stream);
        }

        return list;
    }

    public static List<DownloadItem>? CheckForge(GameSettingObj obj)
    {
        var v2 = CheckRule.GameLaunchVersion(obj.Version);
        if (v2)
        {
            ForgeHelp.ReadyForgeWrapper();
        }

        var forge = VersionPath.GetForgeObj(obj.Version, obj.LoaderInfo.Version);
        if (forge == null)
            return null;

        var list = new List<DownloadItem>();
        var list1 = ForgeHelp.MakeForgeLibs(forge, obj.Version, obj.LoaderInfo.Version);
        foreach (var item in list1)
        {
            if (!File.Exists(item.Local))
            {
                list.Add(item);
                continue;
            }
            if (item.SHA1 == null)
                continue;

            using var stream = new FileStream(item.Local, FileMode.Open, FileAccess.ReadWrite,
                FileShare.ReadWrite);
            var sha1 = Sha1.GenSha1(stream);
            if (item.SHA1 != sha1)
            {
                list.Add(item);
            }
        }

        var forgeinstall = VersionPath.GetForgeInstallObj(obj.Version, obj.LoaderInfo.Version);
        if (forgeinstall == null && v2)
            return null;

        if (forgeinstall != null)
        {
            foreach (var item in forgeinstall.libraries)
            {
                if (item.name.StartsWith("net.minecraftforge:forge:")
                && string.IsNullOrWhiteSpace(item.downloads.artifact.url))
                {
                    continue;
                }

                string file = $"{BaseDir}/{item.downloads.artifact.path}";
                if (!File.Exists(file))
                {
                    list.Add(new()
                    {
                        Local = file,
                        Name = item.name,
                        SHA1 = item.downloads.artifact.sha1,
                        Url = item.downloads.artifact.url
                    });
                    continue;
                }
                using var stream = new FileStream(file, FileMode.Open, FileAccess.ReadWrite,
                    FileShare.ReadWrite);
                var sha1 = Sha1.GenSha1(stream);
                if (item.downloads.artifact.sha1 != sha1)
                {
                    list.Add(new()
                    {
                        Local = file,
                        Name = item.name,
                        SHA1 = item.downloads.artifact.sha1,
                        Url = item.downloads.artifact.url
                    });
                }
            }
        }

        return list;
    }

    public static List<DownloadItem>? CheckFabric(GameSettingObj obj)
    {
        var fabric = VersionPath.GetFabricObj(obj.Version, obj.LoaderInfo.Version);
        if (fabric == null)
            return null;

        var list = new List<DownloadItem>();

        foreach (var item in fabric.libraries)
        {
            var name = PathC.ToName(item.name);
            string file = $"{BaseDir}/{name.Item1}";
            if (!File.Exists(file))
            {
                list.Add(new()
                {
                    Url = UrlHelp.DownloadFabric(item.url + name.Item1, BaseClient.Source),
                    Name = name.Item2,
                    Local = $"{BaseDir}/{name.Item1}"
                });
                continue;
            }
        }

        return list;
    }

    public static List<DownloadItem>? CheckQuilt(GameSettingObj obj)
    {
        var quilt = VersionPath.GetQuiltObj(obj.Version, obj.LoaderInfo.Version);
        if (quilt == null)
            return null;

        var list = new List<DownloadItem>();

        foreach (var item in quilt.libraries)
        {
            var name = PathC.ToName(item.name);
            string file = $"{BaseDir}/{name.Item1}";
            if (!File.Exists(file))
            {
                list.Add(new()
                {
                    Url = UrlHelp.DownloadQuilt(item.url + name.Item1, BaseClient.Source),
                    Name = name.Item2,
                    Local = $"{BaseDir}/{name.Item1}"
                });
                continue;
            }
        }

        return list;
    }

    public static string MakeGameDir(string mc) 
    {
        return $"{BaseDir}/net/minecraft/client/{mc}/client-{mc}.jar";
    }
}
