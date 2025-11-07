using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Core.LaunchPath;

public static class InnerPath
{
    public static string Inner { get; private set; }

    public static void Init()
    {
        //存在用户文件夹
        Inner = SystemInfo.Os == OsType.MacOS ?
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ColorMC") :
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ColorMC");
        if (!Directory.Exists(Inner))
        {
            Directory.CreateDirectory(Inner);
        }
    }
}
