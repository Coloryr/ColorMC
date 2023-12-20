using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Game;

/// <summary>
/// 光影包相关操作
/// </summary>
public static class Shaderpacks
{
    /// <summary>
    /// 获取游戏实例光影包
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <returns>光影包列表</returns>
    public static async Task<List<ShaderpackObj>> GetShaderpacksAsync(this GameSettingObj game)
    {
        var list = new List<ShaderpackObj>();
        var dir = game.GetShaderpacksPath();

        var info = new DirectoryInfo(dir);
        if (!info.Exists)
        {
            info.Create();
            return list;
        }

        await Task.Run(() =>
        {
            Parallel.ForEach(info.GetFiles(), (item) =>
            {
                if (item.Extension is not ".zip")
                {
                    return;
                }
                var obj1 = new ShaderpackObj()
                {
                    Local = Path.GetFullPath(item.FullName),
                    Name = Path.GetFileName(item.FullName)
                };
                list.Add(obj1);
            });
        });

        return list;
    }

    /// <summary>
    /// 删除光影包
    /// </summary>
    /// <param name="pack"></param>
    public static void Delete(this ShaderpackObj pack)
    {
        PathHelper.Delete(pack.Local);
    }

    /// <summary>
    /// 添加光影包
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="file">文件列表</param>
    /// <returns>结果</returns>
    public static async Task<bool> AddShaderpackAsync(this GameSettingObj obj, List<string> file)
    {
        var dir = obj.GetShaderpacksPath();
        Directory.CreateDirectory(dir);
        bool ok = true;

        foreach (var item in file)
        {
            var name = Path.GetFileName(item);
            var name1 = Path.GetFullPath(dir + "/" + name);

            await Task.Run(() =>
            {
                try
                {
                    PathHelper.CopyFile(item, name1);
                }
                catch (Exception e)
                {
                    Logs.Error(LanguageHelper.Get("Core.Game.Error3"), e);
                    ok = false;
                }
            });
            if (!ok)
            {
                return false;
            }
        }
        return true;
    }
}
