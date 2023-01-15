using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;

namespace ColorMC.Core.Game;

public static class Screenshots
{
    public static List<string> GetScreenshots(this GameSettingObj game)
    {
        var list = new List<string>();
        var dir = game.GetScreenshotsPath();
        if (Directory.Exists(dir))
        {
            list.AddRange(Directory.GetFiles(dir));
        }
        return list;
    }

    public static void ClearScreenshots(this GameSettingObj game)
    {
        var dir = game.GetScreenshotsPath();
        if (Directory.Exists(dir))
        {
            foreach (var item in Directory.GetFiles(dir))
            {
                File.Delete(item);
            }
        }
    }
}
