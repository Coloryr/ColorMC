using ColorMC.Core.Http.Downloader;
using ColorMC.Core.Http;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Game;
using ColorMC.Core.Utils;
using System.Collections.Generic;
using System;
using ColorMC.Core.Http.Download;

namespace ColorMC.Core.Path;

public static class LibrariesPath
{
    private const string Name = "libraries";
    public static string BaseDir { get; private set; }
    public static string NativeDir { get; private set; }

    public static void Init(string dir)
    {
        BaseDir = dir + "/" + Name;
        NativeDir = $"{BaseDir}/native-{SystemInfo.Os}";

        Directory.CreateDirectory(BaseDir);
        Directory.CreateDirectory(NativeDir);
    }

    public static List<DownloadItem> Check(GameArgObj obj)
    {
        var list = new List<DownloadItem>();

        var list1 = GameDownload.MakeGameLibs(obj);
        foreach (var item in list1)
        {
            if (!File.Exists(item.Local))
            {
                list.Add(item);
                continue;
            }
            using var stream = new FileStream(item.Local, FileMode.Open, FileAccess.ReadWrite,
                FileShare.ReadWrite);
            var sha1 = Sha1.GenSha1(stream);
            if (item.SHA1 != sha1)
            {
                list.Add(item);
            }
        }

        foreach(var item in list1)
        {
            if (item.Later != null)
            {
                item.Later();
            }
        }

        return list;
    }

    public static List<DownloadItem>? CheckForge(GameSettingObj obj)
    {
        var v2 = CheckRule.GameLaunchVersion(obj.Version);
        if (v2)
        {
            ReadyForgeWrapper();
        }

        var forge = VersionPath.GetForgeObj(obj.Version, obj.LoaderInfo.Version);
        if (forge == null)
            return null;

        var list = new List<DownloadItem>();
        var list1 = GameDownload.MakeForgeLibs(forge, obj.Version, obj.LoaderInfo.Version);
        foreach (var item in list1)
        {
            if (!File.Exists(item.Local))
            {
                list.Add(item);
                continue;
            }
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
                    Url = UrlHelp.DownloadFabric(BaseClient.Source) + name.Item1,
                    Name = name.Item2,
                    Local = $"{LibrariesPath.BaseDir}/{name.Item1}"
                });
                continue;
            }
        }

        return list;
    }

    public static string ForgeWrapper => BaseDir + "/io/github/zekerzhayard/ForgeWrapper/mmc3/ForgeWrapper-mmc3.jar";
    public static void ReadyForgeWrapper()
    {
        var file = new FileInfo(ForgeWrapper);
        if (!file.Exists)
        {
            if (!Directory.Exists(file.DirectoryName))
            {
                Directory.CreateDirectory(file.DirectoryName!);
            }
            File.WriteAllBytes(file.FullName, Resource1.ForgeWrapper_mmc3);
        }
    }
}
