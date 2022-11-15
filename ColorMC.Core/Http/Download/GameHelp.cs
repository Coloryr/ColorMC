using ColorMC.Core.Http.Downloader;
using ColorMC.Core.Objs.Game;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Http.Download;

public static class GameHelp
{
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

                if (lib == null)
                {
                    if (SystemInfo.Os == OsType.Windows)
                    {
                        if (SystemInfo.SystemArch == ArchEnum.x32)
                        {
                            lib = item1.downloads.classifiers.natives_windows_32;
                        }
                        else
                        {
                            lib = item1.downloads.classifiers.natives_windows_64;
                        }
                    }
                }

                if (lib != null)
                {
                    list.Add(new()
                    {
                        Name = item1.name + "-native" + SystemInfo.Os,
                        Url = UrlHelp.DownloadLibraries(lib.url, BaseClient.Source),
                        Local = $"{LibrariesPath.BaseDir}/{lib.path}",
                        SHA1 = lib.sha1,
                        Later = (test) => ForgeHelp.UnpackNative(obj.id, test)
                    });
                }
            }
        }

        return list;
    }
}
