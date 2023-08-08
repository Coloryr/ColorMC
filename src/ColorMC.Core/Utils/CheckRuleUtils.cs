using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Objs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Utils;

public static class CheckRuleUtils
{
    /// <summary>
    /// 检查是否允许
    /// </summary>
    public static bool CheckAllow(List<GameArgObj.Libraries.Rules> list)
    {
        bool download = true;
        if (list == null)
        {
            return true;
        }

        foreach (var item2 in list)
        {
            var action = item2.action;
            if (action == "allow")
            {
                if (item2.os == null)
                {
                    download = true;
                    continue;
                }
                var os = item2.os.name;

                if (os == "osx" && SystemInfo.Os == OsType.MacOS)
                {
                    download = true;
                }
                else if (os == "windows" && SystemInfo.Os == OsType.Windows)
                {
                    download = true;
                }
                else if (os == "linux" && SystemInfo.Os == OsType.Linux)
                {
                    download = true;
                }
                else
                {
                    download = false;
                }
            }
            else if (action == "disallow")
            {
                if (item2.os == null)
                {
                    download = false;
                    continue;
                }
                var os = item2.os.name;

                if (os == "osx" && SystemInfo.Os == OsType.MacOS)
                {
                    download = false;
                }
                else if (os == "windows" && SystemInfo.Os == OsType.Windows)
                {
                    download = false;
                }
                else if (os == "linux" && SystemInfo.Os == OsType.Linux)
                {
                    download = false;
                }
                else
                {
                    download = true;
                }
            }
        }

        return download;
    }

    /// <summary>
    /// 是否V2版本
    /// </summary>
    public static bool GameLaunchVersion(GameArgObj version)
    {
        return version.minimumLauncherVersion > 18;
    }

    /// <summary>
    /// 是否是1.17以上版本
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    public static bool IsGameLaunchVersion117(string version)
    {
        Version version1 = new(version);
        return version1.Minor >= 17;
    }

    public static bool IsGameLaunchVersion120(string version)
    {
        Version version1 = new(version);
        return version1.Minor >= 20;
    }
}

