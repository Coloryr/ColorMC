using System.Collections.Concurrent;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Game;

/// <summary>
/// 光影包相关操作
/// </summary>
public static class GameShaderpacks
{
    /// <summary>
    /// 获取游戏实例光影包
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <returns>光影包列表</returns>
    public static async Task<List<ShaderpackObj>> GetShaderpacksAsync(this GameSettingObj game)
    {
        var dir = game.GetShaderpacksPath();
        var info = new DirectoryInfo(dir);
        if (!info.Exists)
        {
            return [];
        }

        var list = new ConcurrentBag<ShaderpackObj>();
        await Task.Run(() =>
        {
            Parallel.ForEach(info.GetFiles(), (item) =>
            {
                if (item.Extension is not Names.NameZipExt)
                {
                    return;
                }
                var obj1 = new ShaderpackObj()
                {
                    Local = item.FullName,
                    Name = Path.GetFileName(item.FullName)
                };
                list.Add(obj1);
            });
        });

        var list1 = list.ToList();
        list1.Sort(ShaderpackObjComparer.Instance);

        return list1;
    }

    /// <summary>
    /// 删除光影包
    /// </summary>
    /// <param name="pack"></param>
    public static Task DeleteAsync(this ShaderpackObj pack)
    {
        return PathHelper.MoveToTrashAsync(pack.Local);
    }

    /// <summary>
    /// 添加光影包
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="file">文件列表</param>
    /// <returns>是否添加成功</returns>
    public static async Task<bool> AddShaderpackAsync(this GameSettingObj obj, List<string> file)
    {
        var dir = obj.GetShaderpacksPath();
        Directory.CreateDirectory(dir);
        bool ok = true;

        foreach (var item in file)
        {
            var name1 = Path.Combine(dir, Path.GetFileName(item));

            await Task.Run(() =>
            {
                try
                {
                    PathHelper.CopyFile(item, name1);
                }
                catch (Exception e)
                {
                    ColorMCCore.OnError(new GameShaderpackAddErrorEventArgs(obj, item, e));
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
