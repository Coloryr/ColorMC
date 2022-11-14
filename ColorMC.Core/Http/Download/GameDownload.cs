using ColorMC.Core.Http.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Game;
using ColorMC.Core.Path;
using ColorMC.Core.Utils;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ColorMC.Core.Http.Download;

public enum DownloadState
{ 
    Init, GetInfo, End
}

public static class GameDownload
{
    public static async Task<(DownloadState State, List<DownloadItem>? List)> Download(VersionObj.Versions obj)
    {
        var list = new List<DownloadItem>();

        var obj1 = await Get.GetGame(obj.url);
        if (obj1 == null)
            return (DownloadState.Init, null);

        VersionPath.AddGame(obj1);
        var obj2 = await Get.GetAssets(obj1.assetIndex.url);
        if (obj2 == null)
            return (DownloadState.GetInfo, null);

        AssetsPath.AddIndex(obj2, obj1);
        list.Add(new()
        {
            Name = $"{obj.id}.jar",
            Url = UrlHelp.Download(obj1.downloads.client.url, BaseClient.Source),
            Local = $"{VersionPath.BaseDir}/{obj.id}.jar",
            SHA1 = obj1.downloads.client.sha1
        });

        list.AddRange(MakeGameLibs(obj1));

        foreach (var item1 in obj2.objects)
        {
            list.Add(new()
            {
                Name = item1.Key,
                Url = UrlHelp.DownloadAssets(item1.Value.hash, BaseClient.Source),
                Local = $"{AssetsPath.ObjectsDir}/{item1.Value.hash[..2]}/{item1.Value.hash}",
                SHA1 = item1.Value.hash
            });
        }

        return (DownloadState.End, list);
    }

    public static List<DownloadItem> MakeGameLibs(GameArgObj obj)
    {
        var list = new List<DownloadItem>();
        foreach (var item1 in obj.libraries)
        {
            bool download = CheckRule.CheckAllow(item1.rules);
            if (!download)
                continue;

            if (item1.downloads.artifact != null)
            {
                list.Add(new()
                {
                    Name = item1.name,
                    Url = UrlHelp.DownloadLibraries(item1.downloads.artifact.url, BaseClient.Source),
                    Local = $"{LibrariesPath.BaseDir}/{item1.downloads.artifact.path}",
                    SHA1 = item1.downloads.artifact.sha1
                });
            }

            if (item1.downloads.classifiers != null)
            {
                var lib = SystemInfo.Os switch
                {
                    OsType.Windows => item1.downloads.classifiers.natives_windows,
                    OsType.Linux => item1.downloads.classifiers.natives_linux,
                    OsType.MacOS => item1.downloads.classifiers.natives_osx,
                    _ => null
                };

                if (lib != null)
                {
                    list.Add(new()
                    {
                        Name = item1.name,
                        Url = UrlHelp.DownloadLibraries(lib.url, BaseClient.Source),
                        Local = $"{LibrariesPath.BaseDir}/{lib.path}",
                        SHA1 = lib.sha1,
                        Later = ()=> UnpackNative($"{LibrariesPath.BaseDir}/{lib.path}")
                    });
                }
            }
        }

        return list;
    }

    public static void UnpackNative(string local) 
    {
        using ZipFile zFile = new(local);
        foreach (ZipEntry e in zFile)
        {
            if (e.Name.StartsWith("META-INF"))
                continue;
            if (e.IsFile)
            {
                using var stream1 = new FileStream(LibrariesPath.NativeDir + "/" + e.Name, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                using var stream = zFile.GetInputStream(e);
                stream.CopyTo(stream1);
            }
        }
    }

    public static Task<(DownloadState State, List<DownloadItem>? List)> DownloadForge(GameSettingObj obj)
    {
        return DownloadForge(obj.Version, obj.LoaderInfo.Version);
    }

    public static async Task<(DownloadState State, List<DownloadItem>? List)> DownloadForge(string mc, string version)
    {
        var list = new List<DownloadItem>();
        try
        {
            await DownloadForgeInster(mc, version);
        }
        catch
        {
            return (DownloadState.Init, null);
        }

        string name = $"forge-{mc}-{version}";
        using ZipFile zFile = new($"{VersionPath.ForgeDir}/{name}-installer.jar");
        using MemoryStream stream1 = new();
        using MemoryStream stream2 = new();
        bool find = false;
        bool find1 = false;
        foreach (ZipEntry e in zFile)
        {
            if (e.IsFile && e.Name == "version.json")
            {
                using var stream = zFile.GetInputStream(e);
                await stream.CopyToAsync(stream1);
                find = true;
            }
            else if (e.IsFile && e.Name == "install_profile.json")
            {
                using var stream = zFile.GetInputStream(e);
                await stream.CopyToAsync(stream2);
                find1 = true;
            }
        }

        if (!find)
        {
            CoreMain.DownloadState?.Invoke(CoreRunState.Error);
            return (DownloadState.GetInfo, null);
        }

        byte[] array1 = stream1.ToArray();
        byte[] array2 = stream2.ToArray();

        
        if (find1)
        {
            ForgeInstallObj info1;
            var data1 = Encoding.UTF8.GetString(array2);
            try
            {
                info1 = JsonConvert.DeserializeObject<ForgeInstallObj>(data1)!;
                File.WriteAllBytes($"{VersionPath.ForgeDir}/{name}-install.json", stream2.ToArray());
            }
            catch (Exception e)
            {
                Logs.Error("读取forge安装信息错误", e);
                return (DownloadState.GetInfo, null);
            }

            foreach (var item1 in info1.libraries)
            {
                list.Add(new()
                {
                    Url = UrlHelp.DownloadForgeLib(item1.downloads.artifact.url,
                             BaseClient.Source),
                    Name = item1.name,
                    Local = $"{LibrariesPath.BaseDir}/{item1.downloads.artifact.path}",
                    SHA1 = item1.downloads.artifact.sha1
                });
            }
        }

        ForgeLaunchObj info;
        try
        {
            var data = Encoding.UTF8.GetString(stream1.ToArray());
            info = JsonConvert.DeserializeObject<ForgeLaunchObj>(data)!;
            File.WriteAllBytes($"{VersionPath.ForgeDir}/{name}.json", stream1.ToArray());
        }
        catch (Exception e)
        {
            Logs.Error("读取forge信息错误", e);
            return (DownloadState.GetInfo, null);
        }

        list.AddRange(MakeForgeLibs(info, mc, version));

        return (DownloadState.End, list);
    }

    public static List<DownloadItem> MakeForgeLibs(ForgeLaunchObj info, string mc, string version) 
    {
        var list = new List<DownloadItem>();
        foreach (var item1 in info.libraries)
        {
            if (item1.name.StartsWith("net.minecraftforge:forge:")
                && string.IsNullOrWhiteSpace(item1.downloads.artifact.url))
            {
                list.Add(new()
                {
                    Url = UrlHelp.DownloadForgeJar(mc, version, BaseClient.Source),
                    Name = item1.name,
                    Local = $"{LibrariesPath.BaseDir}/net/minecraftforge/forge/" +
                            PathC.MakeForgeName(mc, version),
                    SHA1 = item1.downloads.artifact.sha1
                });
            }
            else
            {
                list.Add(new()
                {
                    Url = UrlHelp.DownloadForgeLib(item1.downloads.artifact.url,
                        BaseClient.Source),
                    Name = item1.name,
                    Local = $"{LibrariesPath.BaseDir}/{item1.downloads.artifact.path}",
                    SHA1 = item1.downloads.artifact.sha1
                });
            }
        }

        return list;
    }

    public static Task<(DownloadState State, List<DownloadItem>? List)> DownloadFabric(GameSettingObj obj)
    {
        return DownloadFabric(obj.Version, obj.LoaderInfo.Version);
    }

    public static async Task<(DownloadState State, List<DownloadItem>? List)> DownloadFabric(string mc, string? version = null)
    {
        var list = new List<DownloadItem>();
        var meta = await Get.GetFabricMeta(BaseClient.Source);
        if (meta == null)
        {
            return (DownloadState.Init, null);
        }

        FabircMetaObj.Loader? fabric;

        if (version != null)
        {
            fabric = meta.loader.Where(a => a.version == version).FirstOrDefault();
        }
        else
        {
            fabric = meta.loader.Where(a => a.stable == true).FirstOrDefault();
        }
        if (fabric == null)
        {
            return (DownloadState.GetInfo, null);
        }

        version = fabric.version;

        CoreMain.DownloadState?.Invoke(CoreRunState.GetInfo);

        FabricLoaderObj? meta1 = await Get.GetFabricLoader(mc, version, BaseClient.Source);
        if (meta1 == null)
        {
            return (DownloadState.GetInfo, null);
        }

        File.WriteAllText($"{VersionPath.FabricDir}/{meta1.id}.json",
            JsonConvert.SerializeObject(meta1));

        foreach (var item in meta1.libraries)
        {
            var name = PathC.ToName(item.name);
            list.Add(new()
            {
                Url = UrlHelp.DownloadFabric(BaseClient.Source) + name.Item1,
                Name = name.Item2,
                Local = $"{LibrariesPath.BaseDir}/{name.Item1}"
            });

        }

        return (DownloadState.End, list);
    }

    public static async Task DownloadForgeInster(string mc, string version)
    {
        string name = $"forge-{mc}-{version}";
        string url = UrlHelp.DownloadForge(mc, version, BaseClient.Source);

        DownloadItem item = new()
        {
            Url = url,
            Name = name + "-installer",
            Local = $"{VersionPath.ForgeDir}/{name}-installer.jar",
        };

        await DownloadThread.Download(item);
    }
}
