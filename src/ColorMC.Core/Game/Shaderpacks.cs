using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Core.Game;

public static class Shaderpacks
{
    /// <summary>
    /// 获取游戏实例资源包
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <returns>资源包列表</returns>
    public static List<ShaderpackObj> GetShaderpacks(this GameSettingObj game)
    {
        var list = new List<ShaderpackObj>();
        var dir = game.GetResourcepacksPath();

        DirectoryInfo info = new(dir);
        if (!info.Exists)
            return list;

        Parallel.ForEach(info.GetFiles(), (item) =>
        {
            if (item.Extension is not ".zip")
                return;
            var obj1 = new ShaderpackObj()
            {
                Local = Path.GetFullPath(item.FullName)
            };
            list.Add(obj1);
        });

        return list;
    }

    public static void Delete(this ShaderpackObj pack)
    {
        File.Delete(pack.Local);
    }

    public static void AddShaderpack(this GameSettingObj obj, string file)
    {
        var dir = obj.GetResourcepacksPath();
        Directory.CreateDirectory(dir);
        var name = Path.GetFileName(file);
        File.Copy(file, Path.GetFullPath(dir + "/" + name));
    }

    //public static void Disable(this ShaderpackObj pack)
    //{
    //    if (pack.Disable)
    //        return;

    //    var file = new FileInfo(pack.Local);
    //    pack.Disable = true;
    //    pack.Local = Path.GetFullPath($"{file.DirectoryName}/{file.Name
    //        .Replace(".zip", ".disable")}");
    //    File.Move(file.FullName, pack.Local);
    //}

    //public static void Enable(this ShaderpackObj pack)
    //{
    //    if (!pack.Disable)
    //        return;

    //    var file = new FileInfo(pack.Local);
    //    pack.Disable = false;
    //    pack.Local = Path.GetFullPath($"{file.DirectoryName}/{file.Name
    //        .Replace(".disable", ".zip")}");
    //    File.Move(file.FullName, pack.Local);
    //}
}
