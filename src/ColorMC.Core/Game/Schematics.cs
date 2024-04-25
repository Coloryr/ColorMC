using System.Collections.Concurrent;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Nbt;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Game;

/// <summary>
/// 结构文件操作
/// </summary>
public static class Schematic
{
    public const string Name1 = ".litematic";
    public const string Name2 = ".schematic";

    /// <summary>
    /// 读取结构文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>列表</returns>
    public static async Task<ConcurrentBag<SchematicObj>> GetSchematicsAsync(this GameSettingObj obj)
    {
        var list = new ConcurrentBag<SchematicObj>();
        var path = obj.GetSchematicsPath();

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            return list;
        }

        var items = Directory.GetFiles(path);
        await Parallel.ForEachAsync(items, async (item, cancel) =>
        {
            var info = new FileInfo(item);
            if (info.Extension.Equals(Name1, StringComparison.OrdinalIgnoreCase))
            {
                list.Add(await ReadLitematicAsync(item));
            }
            else if (info.Extension.Equals(Name2, StringComparison.OrdinalIgnoreCase))
            {
                list.Add(await ReadSchematicAsync(item));
            }
        });

        return list;
    }

    /// <summary>
    /// 删除结构文件
    /// </summary>
    /// <param name="obj">结构文件</param>
    public static void Delete(this SchematicObj obj)
    {
        PathHelper.Delete(obj.Local);
    }

    /// <summary>
    /// 添加结构文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="file">文件列表</param>
    /// <returns>是否添加成功</returns>
    public static bool AddSchematic(this GameSettingObj obj, List<string> file)
    {
        var path = obj.GetSchematicsPath();
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        var ok = true;

        Parallel.ForEach(file, (item) =>
        {
            var name = Path.GetFileName(item);
            var path1 = Path.GetFullPath(path + "/" + name);
            if (File.Exists(path1))
            {
                return;
            }

            try
            {
                PathHelper.CopyFile(item, path1);
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

        return true;
    }

    /// <summary>
    /// 读取结构文件
    /// </summary>
    /// <param name="file">文件</param>
    /// <returns>数据</returns>
    private static async Task<SchematicObj> ReadSchematicAsync(string file)
    {
        try
        {
            if (await NbtBase.Read<NbtCompound>(file) is not { } tag)
            {
                return new()
                {
                    Local = file,
                    Broken = true
                };
            }

            return new()
            {
                Name = Path.GetFileName(file),
                Height = tag.TryGet<NbtShort>("Height")!.Value,
                Length = tag.TryGet<NbtShort>("Length")!.Value,
                Width = tag.TryGet<NbtShort>("Width")!.Value,
                Broken = false,
                Local = file
            };
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Game.Error12"), e);
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
    private static async Task<SchematicObj> ReadLitematicAsync(string file)
    {
        try
        {
            if (await NbtBase.Read<NbtCompound>(file) is not { } tag)
            {
                return new()
                {
                    Local = file,
                    Broken = true
                };
            }

            var com1 = tag.TryGet<NbtCompound>("Metadata")!;

            var item = new SchematicObj()
            {
                Name = com1.TryGet<NbtString>("Name")!.Value,
                Author = com1.TryGet<NbtString>("Author")!.Value,
                Description = com1.TryGet<NbtString>("Description")!.Value,
                Broken = false,
                Local = file
            };

            var pos = com1.TryGet<NbtCompound>("EnclosingSize")!;
            if (pos != null)
            {
                item.Height = pos.TryGet<NbtInt>("y")!.Value;
                item.Length = pos.TryGet<NbtInt>("x")!.Value;
                item.Width = pos.TryGet<NbtInt>("z")!.Value;
            }

            return item;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Game.Error12"), e);
            return new()
            {
                Local = file,
                Broken = true
            };
        }
    }
}
