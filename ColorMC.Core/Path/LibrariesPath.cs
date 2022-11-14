using ColorMC.Core.Http.Downloader;
using ColorMC.Core.Http;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Game;
using ColorMC.Core.Utils;
using System.Collections.Generic;
using System;

namespace ColorMC.Core.Path;

public static class LibrariesPath
{
    private const string Name = "libraries";
    public static string BaseDir { get; private set; }

    public static void Init(string dir)
    {
        BaseDir = dir + "/" + Name;

        Directory.CreateDirectory(BaseDir);
    }

    public static List<GameArgObj.Libraries> Check(GameArgObj obj)
    {
        var list = new List<GameArgObj.Libraries>();
        foreach (var item in obj.libraries)
        {
            if (!CheckRule.CheckAllow(item.rules))
                continue;
            string file = $"{BaseDir}/{item.downloads.artifact.path}";
            if (!File.Exists(file))
            {
                list.Add(item);
                continue;
            }
            using var stream = new FileStream(file, FileMode.Open, FileAccess.ReadWrite,
                FileShare.ReadWrite);
            var sha1 = Sha1.GenSha1(stream);
            if (item.downloads.artifact.sha1 != sha1)
            {
                list.Add(item);
            }
        }

        return list;
    }

    public static List<ForgeLaunchObj.Libraries>? CheckForge(GameSettingObj obj)
    {
        var v2 = CheckRule.GameLaunchVersion(obj.Version);
        if (v2)
        {
            ReadyForgeWrapper();
        }

        var forge = VersionPath.GetForgeObj(obj.Version, obj.LoaderInfo.Version);
        if (forge == null)
            return null;

        var list = new List<ForgeLaunchObj.Libraries>();

        foreach (var item in forge.libraries)
        {
            if (item.name.StartsWith("net.minecraftforge:forge:"))
            {
                string file = $"{BaseDir}/net/minecraftforge/forge/" +
                    PathC.MakeForgeName(obj.Version, obj.LoaderInfo.Version);
                if (!File.Exists(file))
                {
                    list.Add(item);
                    continue;
                }
                using var stream = new FileStream(file, FileMode.Open, FileAccess.ReadWrite,
                    FileShare.ReadWrite);
                var sha1 = Sha1.GenSha1(stream);
                if (item.downloads.artifact.sha1 != sha1)
                {
                    list.Add(item);
                }
            }
            else
            {
                string file = $"{BaseDir}/{item.downloads.artifact.path}";
                if (!File.Exists(file))
                {
                    list.Add(item);
                    continue;
                }
                using var stream = new FileStream(file, FileMode.Open, FileAccess.ReadWrite,
                    FileShare.ReadWrite);
                var sha1 = Sha1.GenSha1(stream);
                if (item.downloads.artifact.sha1 != sha1)
                {
                    list.Add(item);
                }
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
                    list.Add(item);
                    continue;
                }
                using var stream = new FileStream(file, FileMode.Open, FileAccess.ReadWrite,
                    FileShare.ReadWrite);
                var sha1 = Sha1.GenSha1(stream);
                if (item.downloads.artifact.sha1 != sha1)
                {
                    list.Add(item);
                }
            }
        }

        return list;
    }

    public static List<FabricLoaderObj.Libraries>? CheckFabric(GameSettingObj obj)
    {
        var fabric = VersionPath.GetFabricObj(obj.Version, obj.LoaderInfo.Version);
        if (fabric == null)
            return null;

        var list = new List<FabricLoaderObj.Libraries>();

        foreach (var item in fabric.libraries)
        {
            var name = PathC.ToName(item.name);
            string file = $"{BaseDir}/{name.Item1}";
            if (!File.Exists(file))
            {
                list.Add(item);
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
