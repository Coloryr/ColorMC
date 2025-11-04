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
public static class GameServers
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
            if (await NbtBase.ReadAsync<NbtCompound>(file) is not { } tag)
            {
                return [];
            }

            var nbtList = tag.TryGet<NbtList>("servers")!;
            foreach (var item in nbtList)
            {
                if (item is NbtCompound tag1)
                {
                    var obj = ToServerInfo(tag1);
                    obj.Game = game;
                    list.Add(obj);
                }
            }
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new GameServerReadErrorEventArgs(game, e));
        }
        return list;
    }

    /// <summary>
    /// 添加服务器
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="name">名字</param>
    /// <param name="ip">地址</param>
    public static async Task<bool> AddServerAsync(this GameSettingObj game, string name, string ip)
    {
        try
        {
            var list = await game.GetServerInfosAsync();
            list.Add(new ServerInfoObj()
            {
                Name = name,
                IP = ip
            });
            await game.SaveServerAsync(list);
            return true;
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new GameServerAddErrorEventArgs(game, name, ip, e));
            return false;
        }
    }

    /// <summary>
    /// 删除服务器
    /// </summary>
    /// <param name="obj">服务器</param>
    /// <returns></returns>
    public static async Task<bool> DeleteAsync(this ServerInfoObj obj)
    {
        try
        {
            await RemoveServerAsync(obj.Game, obj.Name, obj.IP);
            return true;
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new GameServerDeleteErrorEventArgs(obj, e));
            return false;
        }
    }

    /// <summary>
    /// 删除服务器
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="name">名字</param>
    /// <param name="ip">地址</param>
    private static async Task RemoveServerAsync(this GameSettingObj game, string name, string ip)
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
        await game.SaveServerAsync(list);
    }

    /// <summary>
    /// 保存服务器列表
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <param name="list">服务器列表</param>
    private static async Task SaveServerAsync(this GameSettingObj game, IEnumerable<ServerInfoObj> list)
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
            { ValueByte = item.AcceptTextures ? (byte)1 : (byte)0 });
            list1.Add(tag1);
        }

        nbtTag.Add("servers", list1);
        await nbtTag.SaveAsync(game.GetServersFile());
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
            AcceptTextures = tag.TryGet("acceptTextures") is NbtByte { ValueByte: 1 }
        };

        return info;
    }
}
