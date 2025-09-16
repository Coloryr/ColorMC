using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Core.Game;

/// <summary>
/// 屏幕截图相关操作
/// </summary>
public static class GameScreenshots
{
    /// <summary>
    /// 获取屏幕截图
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <returns>截图列表</returns>
    public static List<ScreenshotObj> GetScreenshots(this GameSettingObj game)
    {
        var dir = game.GetScreenshotsPath();
        if (!Directory.Exists(dir))
        {
            return [];
        }

        var list = new List<ScreenshotObj>();
        foreach (var item in Directory.GetFiles(dir))
        {
            list.Add(new()
            {
                File = item,
                Name = Path.GetFileName(item)
            });
        }
        return list;
    }

    /// <summary>
    /// 清理屏幕截图
    /// </summary>
    /// <param name="game">游戏实例</param>
    public static void ClearScreenshots(this GameSettingObj game)
    {
        var dir = game.GetScreenshotsPath();
        if (!Directory.Exists(dir))
        {
            return;
        }

        foreach (var item in Directory.GetFiles(dir))
        {
            PathHelper.Delete(item);
        }
    }

    /// <summary>
    /// 删除屏幕截图
    /// </summary>
    /// <param name="item">截图文件</param>
    public static Task Delete(ScreenshotObj item)
    {
        return PathHelper.MoveToTrashAsync(item.File);
    }
}
