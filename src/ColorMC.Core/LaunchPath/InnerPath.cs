using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Core.LaunchPath;

public static class InnerPath
{
    public static string Inner { get; private set; }

    static InnerPath()
    {
        //存在用户文件夹
        Inner = SystemInfo.Os == OsType.MacOs ?
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ColorMC") :
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ColorMC");
        if (!Directory.Exists(Inner))
        {
            Directory.CreateDirectory(Inner);
        }
    }
}
