using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Nbt;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Game;

/// <summary>
/// 服务器列表存储操作
/// </summary>
public static class Servers
{
    /// <summary>
    /// 获取服务器储存
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <returns>服务器列表</returns>
    public static async Task<List<ServerInfoObj>> GetServerInfosAsync(this GameSettingObj game)
    {
        var file = game.GetServersFile();
        if (!File.Exists(file))
        {
            return [];
        }

        var list = new List<ServerInfoObj>();

        try
        {
            if (await NbtBase.Read<NbtCompound>(file) is not { } tag)
            {
                return [];
            }

            var nbtList = tag.TryGet<NbtList>("servers")!;
            foreach (var item in nbtList)
            {
                if (item is NbtCompound tag1)
                {
                    list.Add(ToServerInfo(tag1));
                }
            }
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Game.Error7"), e);
        }
        return list;
    }

    /// <summary>
    /// 添加服务器
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="name">名字</param>
    /// <param name="ip">地址</param>
    public static async Task AddServerAsync(this GameSettingObj game, string name, string ip)
    {
        var list = await game.GetServerInfosAsync();
        list.Add(new ServerInfoObj()
        {
            Name = name,
            IP = ip
        });
        game.SaveServer(list);
    }

    /// <summary>
    /// 删除服务器
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="name">名字</param>
    /// <param name="ip">地址</param>
    public static async Task RemoveServerAsync(this GameSettingObj game, string name, string ip)
    {
        var list = (await game.GetServerInfosAsync()).ToList();
        foreach (var item in list)
        {
            if (item.Name == name && item.IP == ip)
            {
                list.Remove(item);
                break;
            }
        }
        game.SaveServer(list);
    }

    /// <summary>
    /// 保存服务器列表
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="list">服务器列表</param>
    public static void SaveServer(this GameSettingObj game, IEnumerable<ServerInfoObj> list)
    {
        var nbtTag = new NbtCompound();

        var list1 = new NbtList
        {
            InNbtType = NbtType.NbtCompound
        };
        foreach (var item in list)
        {
            var tag1 = new NbtCompound()
            {
                {"name",  new NbtString(){ Value = item.Name } },
                {"ip",  new NbtString(){ Value = item.IP } }
            };

            if (!string.IsNullOrWhiteSpace(item.Icon))
            {
                tag1.Add("icon", new NbtString() { Value = item.Icon });
            }
            tag1.Add("acceptTextures", new NbtByte()
            { Value = item.AcceptTextures ? (byte)1 : (byte)0 });
            list1.Add(tag1);
        }

        nbtTag.Add("servers", list1);
        nbtTag.Save(game.GetServersFile());
    }

    /// <summary>
    /// 转换服务器储存
    /// </summary>
    /// <param name="tag">NBT标签</param>
    /// <returns>服务器储存</returns>
    private static ServerInfoObj ToServerInfo(NbtCompound tag)
    {
        var info = new ServerInfoObj
        {
            Name = tag.TryGet<NbtString>("name")!.Value,
            IP = tag.TryGet<NbtString>("ip")!.Value,
            Icon = (tag.TryGet("icon") as NbtString)?.Value,
            AcceptTextures = tag.TryGet("acceptTextures") is NbtByte { Value: 1 }
        };

        return info;
    }
}
