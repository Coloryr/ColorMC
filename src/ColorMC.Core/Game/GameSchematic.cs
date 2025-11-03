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
public static class GameSchematic
{
    /// <summary>
    /// 读取结构文件
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>结构文件列表</returns>
    public static async Task<List<SchematicObj>> GetSchematicsAsync(this GameSettingObj obj)
    {
        var path = obj.GetSchematicsPath();

        if (!Directory.Exists(path))
        {
            return [];
        }

        var list = new ConcurrentBag<SchematicObj>();
        var items = Directory.GetFiles(path);
#if false
        await Parallel.ForEachAsync(items, new ParallelOptions()
        {
            MaxDegreeOfParallelism = 1
        }, async (item, cancel) =>
#else
        await Parallel.ForEachAsync(items, async (item, cancel) =>
#endif
        {
            try
            {
                var info = new FileInfo(item);
                if (info.Extension.Equals(Names.NameLitematicExt, StringComparison.OrdinalIgnoreCase))
                {
                    list.Add(await ReadLitematicAsync(item));
                }
                else if (info.Extension.Equals(Names.NameSchematicExt, StringComparison.OrdinalIgnoreCase))
                {
                    var item1 = await ReadSchematicAsync(item);
                    item1.Type = SchematicType.Minecraft;
                    list.Add(item1);
                }
                else if (info.Extension.Equals(Names.NameSchemExt, StringComparison.OrdinalIgnoreCase))
                {
                    var item1 = await ReadSchematicAsync(item);
                    item1.Type = SchematicType.WorldEdit;
                    list.Add(item1);
                }
                else if (info.Extension.Equals(Names.NameNbtExt, StringComparison.OrdinalIgnoreCase))
                {
                    list.Add(await ReadNBTAsync(item));
                }
            }
            catch (Exception e)
            {
                ColorMCCore.OnError(new GameSchematicReadErrorEventArgs(obj, item, e));
                list.Add(new SchematicObj
                { 
                    Local = item,
                    Broken = true
                });
            }
        });

        var list1 = list.ToList();
        list1.Sort(SchematicObjComparer.Instance);

        return list1;
    }

    /// <summary>
    /// 删除结构文件
    /// </summary>
    /// <param name="obj">结构文件</param>
    public static Task DeleteAsync(this SchematicObj obj)
    {
        return PathHelper.MoveToTrashAsync(obj.Local);
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
            var path1 = Path.Combine(path, Path.GetFileName(item));
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
                ColorMCCore.OnError(new GameSchematicAddErrorEventArgs(obj, item, e));
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
    /// 机械动力蓝图
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    private static async Task<SchematicObj> ReadNBTAsync(string file)
    {
        if (await NbtBase.ReadAsync<NbtCompound>(file) is not { } tag 
            || tag.TryGet<NbtList>("size") is not { } size
            || size.Count != 3)
        {
            return new()
            {
                Local = file,
                Broken = true
            };
        }

        var item = new SchematicObj
        {
            Name = Path.GetFileName(file),
            Broken = false,
            Local = file,
            Width = (size[0] as NbtInt)!.Value,
            Height = (size[1] as NbtInt)!.Value,
            Length = (size[2] as NbtInt)!.Value,
            Type = SchematicType.Create
        };

        return item;
    }

    /// <summary>
    /// 读取结构文件
    /// </summary>
    /// <param name="file">文件</param>
    /// <returns>数据</returns>
    private static async Task<SchematicObj> ReadSchematicAsync(string file)
    {
        if (await NbtBase.ReadAsync<NbtCompound>(file) is not { } tag)
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

    /// <summary>
    /// 读取结构文件
    /// </summary>
    /// <param name="file">文件</param>
    /// <returns>数据</returns>
    private static async Task<SchematicObj> ReadLitematicAsync(string file)
    {
        if (await NbtBase.ReadAsync<NbtCompound>(file) is not { } tag)
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
            Local = file,
            Type = SchematicType.Litematic
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
}
