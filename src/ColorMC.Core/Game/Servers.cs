using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using fNbt;

namespace ColorMC.Core.Game;

public static class Servers
{
    /// <summary>
    /// 获取服务器储存
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <returns>服务器列表</returns>
    public static List<ServerInfoObj> GetServerInfo(this GameSettingObj game)
    {
        List<ServerInfoObj> list = new();
        string file = game.GetServersFile();
        if (!File.Exists(file))
            return list;
        var myFile = new NbtFile();
        myFile.LoadFromFile(file);
        var myCompoundTag = myFile.RootTag as NbtCompound;
        if (myCompoundTag == null)
            return list;

        var nbtList = myCompoundTag.Get<NbtList>("servers");
        if (nbtList == null)
            return list;
        foreach (NbtCompound item in nbtList.Cast<NbtCompound>())
        {
            list.Add(ToServerInfo(item));
        }
        return list;
    }

    /// <summary>
    /// 添加服务器
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="name">名字</param>
    /// <param name="ip">地址</param>
    public static void AddServer(this GameSettingObj game, string name, string ip)
    {
        var list = game.GetServerInfo();
        list.Add(new ServerInfoObj()
        {
            Name = name,
            IP = ip
        });
        game.SaveServer(list);
    }

    /// <summary>
    /// 保存服务器列表
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="list">服务器列表</param>
    public static void SaveServer(this GameSettingObj game, List<ServerInfoObj> list)
    {
        NbtCompound tag = new("");
        NbtList list1 = new("servers");
        foreach (var item in list)
        {
            NbtCompound tag1 = new()
            {
                new NbtString("name", item.Name),
                new NbtString("ip", item.IP)
            };
            if (!string.IsNullOrWhiteSpace(item.Icon))
            {
                tag1.Add(new NbtString("icon", item.Icon));
            }
            tag1.Add(new NbtByte("acceptTextures",
                item.AcceptTextures ? (byte)1 : (byte)0));
            list1.Add(tag1);
        }

        tag.Add(list1);
        string file = game.GetServersFile();
        var myFile = new NbtFile(tag);
        myFile.SaveToFile(file, NbtCompression.None);
    }

    private static ServerInfoObj ToServerInfo(NbtCompound tag)
    {
        var info = new ServerInfoObj()
        {
            Name = tag.Get<NbtString>("name").Value,
            IP = tag.Get<NbtString>("ip").Value
        };

        var data = tag.Get<NbtString>("icon");
        if (data != null)
        {
            info.Icon = data.Value;
        }
        var data1 = tag["acceptTextures"];
        if (data1 != null)
        {
            info.AcceptTextures = (data1 as NbtByte).Value == 1;
        }

        return info;
    }
}
