using ColorMC.Core.Objs.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Utils;

public static class CheckRule
{
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

    public static bool GameLaunchVersion(string version)
    {
        Version version1 = new(version);
        return version1.Minor > 12;
    }
}
