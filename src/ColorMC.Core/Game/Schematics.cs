using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using fNbt;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Game;

public static class Schematic
{
    private const string Name1 = ".litematic";
    private const string Name2 = ".schematic";

    public static async Task<ConcurrentBag<object>> GetSchematics(this GameSettingObj obj)
    {
        var list = new ConcurrentBag<object>();
        var path = obj.GetSchematicsPath();
        if (!Directory.Exists(path))
            return list;

        var items = Directory.GetFiles(path);
        await Parallel.ForEachAsync(items, async (item, cancel) =>
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

    private static SchematicObj ReadAsSchematic(string file)
    {
        var nbt = new NbtFile(file);

        if (nbt.RootTag is not NbtCompound com)
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
            Height = com.Get<NbtShort>("Height").ShortValue,
            Length = com.Get<NbtShort>("Length").ShortValue,
            Width = com.Get<NbtShort>("Width").ShortValue,
            Broken = false,
            Local = file
        };
    }

    private static SchematicObj ReadAsLitematic(string file)
    {
        var nbt = new NbtFile(file);

        if (nbt.RootTag is not NbtCompound com
            || com.Get<NbtCompound>("Metadata") is not NbtCompound com1)
        {
            return new()
            {
                Local = file,
                Broken = true
            };
        }

        var item = new SchematicObj()
        {
            Name = com1.Get<NbtString>("Name").Value,
            Author = com1.Get<NbtString>("Author").Value,
            Description = com1.Get<NbtString>("Description").Value,
            Broken = false,
            Local = file
        };

        var pos = com1.Get<NbtCompound>("EnclosionSize");
        if (pos != null)
        {
            item.Height = pos.Get<NbtInt>("y").IntValue;
            item.Length = pos.Get<NbtInt>("x").IntValue;
            item.Width = pos.Get<NbtInt>("z").IntValue;
        }

        return item;
    }

    public static void Delete(this SchematicObj obj)
    {
        File.Delete(obj.Local);
    }

    public static void AddSchematic(this GameSettingObj obj, string file)
    {
        var path = obj.GetSchematicsPath();
        Directory.CreateDirectory(path);
        var name = Path.GetFileName(file);
        File.Copy(file, Path.GetFullPath(path + "/" + name));
    }
}
