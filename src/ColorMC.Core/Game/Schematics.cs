using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using NbtLib;
using System.Collections.Concurrent;

namespace ColorMC.Core.Game;

public static class Schematic
{
    public const string Name1 = ".litematic";
    public const string Name2 = ".schematic";

    /// <summary>
    /// 读取结构文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>列表</returns>
    public static ConcurrentBag<SchematicObj> GetSchematics(this GameSettingObj obj)
    {
        var list = new ConcurrentBag<SchematicObj>();
        var path = obj.GetSchematicsPath();

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            return list;
        }

        var items = Directory.GetFiles(path);
        Parallel.ForEach(items, (item, cancel) =>
        {
            var info = new FileInfo(item);
            if (info.Extension.ToLower() == Name1)
            {
                list.Add(ReadAsLitematic(item));
            }
            else if (info.Extension.ToLower() == Name2)
            {
                list.Add(ReadAsSchematic(item));
            }
        });

        return list;
    }

    /// <summary>
    /// 读取结构文件
    /// </summary>
    /// <param name="file">文件</param>
    /// <returns>数据</returns>
    private static SchematicObj ReadAsSchematic(string file)
    {
        try
        {
            using var inputStream = File.OpenRead(file);
            var tag = NbtConvert.ParseNbtStream(inputStream);

            return new()
            {
                Name = Path.GetFileName(file),
                Height = ((NbtShortTag)tag["Height"]).Payload,
                Length = ((NbtShortTag)tag["Length"]).Payload,
                Width = ((NbtShortTag)tag["Width"]).Payload,
                Broken = false,
                Local = file
            };
        }
        catch
        {
            return new()
            {
                Local = file,
                Broken = true
            };
        }
    }

    /// <summary>
    /// 读取结构文件
    /// </summary>
    /// <param name="file">文件</param>
    /// <returns>数据</returns>
    private static SchematicObj ReadAsLitematic(string file)
    {
        try
        {
            using var inputStream = File.OpenRead(file);
            var nbtData = NbtConvert.ParseNbtStream(inputStream);
            var com1 = (nbtData["Metadata"] as NbtCompoundTag)!;

            var item = new SchematicObj()
            {
                Name = ((NbtStringTag)com1["Name"]).Payload,
                Author = ((NbtStringTag)com1["Author"]).Payload,
                Description = ((NbtStringTag)com1["Description"]).Payload,
                Broken = false,
                Local = file
            };

            var pos = (com1["EnclosingSize"] as NbtCompoundTag)!;
            if (pos != null)
            {
                item.Height = ((NbtIntTag)pos["y"]).Payload;
                item.Length = ((NbtIntTag)pos["x"]).Payload;
                item.Width = ((NbtIntTag)pos["z"]).Payload;
            }

            return item;

        }
        catch
        {
            return new()
            {
                Local = file,
                Broken = true
            };
        }
    }

    /// <summary>
    /// 删除结构文件
    /// </summary>
    /// <param name="obj">结构文件</param>
    public static void Delete(this SchematicObj obj)
    {
        File.Delete(obj.Local);
    }

    /// <summary>
    /// 添加结构文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="file">文件列表</param>
    /// <returns>结果</returns>
    public static bool AddSchematic(this GameSettingObj obj, List<string> file)
    {
        var path = obj.GetSchematicsPath();
        Directory.CreateDirectory(path);
        bool ok = true;

        Parallel.ForEach(file, (item) =>
        {
            var name = Path.GetFileName(item);
            var path1 = Path.GetFullPath(path + "/" + name);
            if (File.Exists(path1))
                return;

            try
            {
                File.Copy(item, path1);
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.GetName("Core.Game.Error3"), e);
                ok = false;
            }
        });

        if (!ok)
            return false;

        return true;
    }
}
