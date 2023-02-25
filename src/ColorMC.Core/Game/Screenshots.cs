using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using System.IO;

namespace ColorMC.Core.Game;

public static class Screenshots
{
    /// <summary>
    /// 获取屏幕截图
    /// </summary>
    /// <param name="game"></param>
    /// <returns></returns>
    public static List<string> GetScreenshots(this GameSettingObj game)
    {
        var list = new List<string>();
        var dir = game.GetScreenshotsPath();
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
            return list;
        }
            
        list.AddRange(Directory.GetFiles(dir));
        return list;
    }

    /// <summary>
    /// 清理屏幕截图
    /// </summary>
    /// <param name="game"></param>
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
