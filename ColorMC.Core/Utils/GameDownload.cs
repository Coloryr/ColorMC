using ColorMC.Core.Http;
using ColorMC.Core.Http.Download;
using ColorMC.Core.Objs.Game;
using ColorMC.Core.Path;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Utils;

public static class GameDownload
{
    public static async Task Download(VersionObj.Versions obj) 
    {
        CoreMain.DownloadState?.Invoke(DownloadState.Init);
        DownloadManager.Clear();
        CoreMain.DownloadState?.Invoke(DownloadState.GetInfo);
        var obj1 = await GetGame.Get(obj.url);
        if (obj1 == null)
            return;
        VersionPath.AddGame(obj1);
        var obj2 = await GetAssets.Get(obj1.assetIndex.url);
        if (obj2 == null)
            return;
        AssetsPath.AddIndex(obj2, obj.id);
        DownloadItem item = new()
        {
            Name = $"{obj.id}.jar",
            Url = UrlHelp.Download(obj1.downloads.client.url, BaseClient.Source),
            Local = $"{VersionPath.BaseDir}/{obj.id}.jar",
            SHA1 = obj1.downloads.client.sha1
        };
        DownloadManager.AddItem(item);
        CoreMain.DownloadStateUpdate?.Invoke(item);
        foreach (var item1 in obj1.libraries)
        {
            bool download = true;
            if (item1.rules != null)
            {
                foreach (var item2 in item1.rules)
                {
                    var action = item2.action;
                    var os = item2.os.name;
                    var nowOS = SystemInfo.Os;
                    if (os == "osx" && nowOS != OsType.MacOS)
                    {
                        download = false;
                    }
                    else if (os == "windows" && nowOS != OsType.Windows)
                    {
                        download = false;
                    }
                    else if (os == "linux" && nowOS != OsType.Linux)
                    {
                        download = false;
                    }
                    else
                    {
                        download = true;
                    }
                }
            }
            if (!download)
                continue;

            item = new()
            {
                Name = item1.name,
                Url = UrlHelp.DownloadLibraries(item1.downloads.artifact.url, BaseClient.Source),
                Local = $"{LibrariesPath.BaseDir}/{item1.downloads.artifact.path}",
                SHA1 = item1.downloads.artifact.sha1
            };
            DownloadManager.AddItem(item);
            CoreMain.DownloadStateUpdate?.Invoke(item);
        }

        foreach (var item1 in obj2.objects)
        {
            item = new()
            {
                Name = item1.Key,
                Url = UrlHelp.DownloadAssets(item1.Value.hash, BaseClient.Source),
                Local = $"{AssetsPath.ObjectsDir}/{item1.Value.hash[0..1]}/{item1.Value.hash}"
            };
            DownloadManager.AddItem(item);
            CoreMain.DownloadStateUpdate?.Invoke(item);
        }
    }

    public static async Task DownloadForge(string version) 
    {
        
    }
}
