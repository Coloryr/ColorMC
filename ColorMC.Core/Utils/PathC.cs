using ColorMC.Core.Path;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Utils;

public static class PathC
{
    public static (string, string) ToName(string input)
    {
        var arg = input.Split(':');
        var arg1 = arg[0].Split('.');
        string path = "";
        for (int a = 0; a < arg1.Length; a++)
        {
            path += arg1[a] + '/';
        }
        path += $"{arg[1]}/{arg[2]}/{arg[1]}-{arg[2]}.jar";
        string name = $"{arg[1]}-{arg[2]}.jar";

        return (path, name);
    }

    public static string MakeForgeName(string mc, string version)
    {
        return $"{mc}-{version}/forge-{mc}-{version}-" +
            $"{(CheckRule.ForgeVersionCheck(mc) ? "launcher" : "universal")}.jar";
    }
}
