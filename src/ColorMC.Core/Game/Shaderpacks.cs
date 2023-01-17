using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Core.Game;

public static class Shaderpacks
{
    public static List<ShaderpackObj> GetShaderpacks(this GameSettingObj game)
    {
        var list = new List<ShaderpackObj>();
        var dir = game.GetResourcepacksPath();

        DirectoryInfo info = new(dir);
        if (!info.Exists)
            return list;

        ParallelOptions options = new()
        {
            MaxDegreeOfParallelism = 5
        };
        Parallel.ForEach(info.GetFiles(), options, (item) =>
        {
            if (item.Extension is not (".zip" or ".disable"))
                return;
            var obj1 = new ShaderpackObj()
            {
                Disable = item.Extension is ".disable",
                Local = Path.GetFullPath(item.FullName)
            };
            list.Add(obj1);
        });

        return list;
    }
    public static void Disable(this ShaderpackObj pack)
    {
        if (pack.Disable)
            return;

        var file = new FileInfo(pack.Local);
        pack.Disable = true;
        pack.Local = Path.GetFullPath($"{file.DirectoryName}/{file.Name.Replace(".zip", ".disable")}");
        File.Move(file.FullName, pack.Local);
    }

    public static void Enable(this ShaderpackObj pack)
    {
        if (!pack.Disable)
            return;

        var file = new FileInfo(pack.Local);
        pack.Disable = false;
        pack.Local = Path.GetFullPath($"{file.DirectoryName}/{file.Name.Replace(".disable", ".zip")}");
        File.Move(file.FullName, pack.Local);
    }
}
