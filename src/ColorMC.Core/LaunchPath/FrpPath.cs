using ColorMC.Core.Config;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.LaunchPath;

public static class FrpPath
{
    public const string Name1 = "frp";
    public static string BaseDir { get; private set; }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行路径</param>
    public static void Init(string dir)
    {
        BaseDir = dir;

        if (!BaseDir.EndsWith("/") && !BaseDir.EndsWith("\\"))
        {
            BaseDir += "/";
        }

        BaseDir += Name1;

        Directory.CreateDirectory(BaseDir);
    }

    public static string GetSakuraFrpLocal(string ver)
    {
        return $"{BaseDir}/SakuraFrp/{ver}/frpc.exe";
    }
}
