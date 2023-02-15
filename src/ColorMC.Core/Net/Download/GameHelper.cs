using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Net.Download;

public static class GameHelper
{
    /// <summary>
    /// 创建游戏运行库项目
    /// </summary>
    /// <param name="obj">游戏数据</param>
    public static List<DownloadItem> MakeGameLibs(GameArgObj obj)
    {
        var list = new List<DownloadItem>();
        var list1 = new List<string>();
        foreach (var item1 in obj.libraries)
        {
            bool download = CheckRule.CheckAllow(item1.rules);
            if (!download)
                continue;

            if (item1.downloads.artifact != null)
            {
                if (!list1.Contains(item1.downloads.artifact.sha1))
                {
                    list.Add(new()
                    {
                        Name = item1.name,
                        Url = UrlHelper.DownloadLibraries(item1.downloads.artifact.url, BaseClient.Source),
                        Local = $"{LibrariesPath.BaseDir}/{item1.downloads.artifact.path}",
                        SHA1 = item1.downloads.artifact.sha1
                    });

                    list1.Add(item1.downloads.artifact.sha1);
                }
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
                    if (list1.Contains(lib.sha1))
                        continue;

                    list.Add(new()
                    {
                        Name = item1.name + "-native" + SystemInfo.Os,
                        Url = UrlHelper.DownloadLibraries(lib.url, BaseClient.Source),
                        Local = $"{LibrariesPath.BaseDir}/{lib.path}",
                        SHA1 = lib.sha1,
                        Later = (test) => ForgeHelper.UnpackNative(obj.id, test)
                    });

                    list1.Add(lib.sha1);
                }
            }
        }

        return list;
    }
}
