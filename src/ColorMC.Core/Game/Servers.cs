using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using SharpNBT;

namespace ColorMC.Core.Game;

public static class Servers
{
    public static async Task<List<ServerInfoObj>> GetServerInfo(this GameSettingObj game)
    {
        List<ServerInfoObj> list = new();
        string file = game.GetServersFile();
        if (!File.Exists(file))
            return list;
        CompoundTag tag = await NbtFile.ReadAsync(file, FormatOptions.Java, CompressionType.AutoDetect);
        var nbtList = tag["servers"] as ListTag;
        if (nbtList == null)
            return list;
        foreach (CompoundTag item in nbtList.Cast<CompoundTag>())
        {
            list.Add(ToServerInfo(item));
        }
        return list;
    }

    public static async Task AddServer(this GameSettingObj game, string name, string ip)
    {
        var list = await game.GetServerInfo();
        list.Add(new ServerInfoObj()
        {
            Name = name,
            IP = ip
        });
        await game.SaveServer(list);
    }

    public static async Task SaveServer(this GameSettingObj game, List<ServerInfoObj> list)
    {
        CompoundTag tag = new(null);
        ListTag list1 = new("servers", TagType.Compound);
        foreach (var item in list)
        {
            CompoundTag tag1 = new(null)
            {
                new StringTag("name", item.Name),
                new StringTag("ip", item.IP)
            };
            if (!string.IsNullOrWhiteSpace(item.Icon))
            {
                tag1.Add(new StringTag("icon", item.Icon));
            }
            tag1.Add(new BoolTag("acceptTextures", item.AcceptTextures));
            list1.Add(tag1);
        }

        tag.Add(list1);
        string file = game.GetServersFile();
        await NbtFile.WriteAsync(file, tag, FormatOptions.Java, CompressionType.AutoDetect);
    }

    private static ServerInfoObj ToServerInfo(CompoundTag tag)
    {
        var info = new ServerInfoObj()
        {
            Name = (tag["name"] as StringTag).Value,
            IP = (tag["ip"] as StringTag).Value
        };

        var data = tag["icon"] as StringTag;
        if (data != null)
        {
            info.Icon = data.Value;
        }
        var data1 = tag["acceptTextures"];
        if (data1 != null)
        {
            info.AcceptTextures = (data1 as BoolTag).Value;
        }

        return info;
    }
}
