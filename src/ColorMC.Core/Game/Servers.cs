using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using NbtLib;
using System.Collections.Concurrent;

namespace ColorMC.Core.Game;

public static class Servers
{
    /// <summary>
    /// 获取服务器储存
    /// </summary>
    /// <param name="game">游戏实例</param>
    /// <returns>服务器列表</returns>
    public static ConcurrentBag<ServerInfoObj> GetServerInfos(this GameSettingObj game)
    {
        ConcurrentBag<ServerInfoObj> list = new();
        string file = game.GetServersFile();
        if (!File.Exists(file))
            return list;

        try
        {
            using var inputStream = File.OpenRead(file);
            var tag = NbtConvert.ParseNbtStream(inputStream);

            var nbtList = (NbtListTag)tag["servers"]!;
            foreach (var item in nbtList)
            {
                if (item is NbtCompoundTag tag1)
                {
                    list.Add(ToServerInfo(tag1));
                }
            }
        }
        catch (Exception e)
        {
            Logs.Error("server load error", e);
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
        var list = game.GetServerInfos();
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
    public static void SaveServer(this GameSettingObj game, IEnumerable<ServerInfoObj> list)
    {
        var nbtTag = new NbtCompoundTag();

        NbtListTag list1 = new(NbtTagType.Compound);
        foreach (var item in list)
        {
            NbtCompoundTag tag1 = new()
            {
                ["name"] = new NbtStringTag(item.Name),
                ["ip"] = new NbtStringTag(item.IP)
            };

            if (!string.IsNullOrWhiteSpace(item.Icon))
            {
                tag1.Add("icon", new NbtStringTag(item.Icon));
            }
            tag1.Add("acceptTextures", new NbtByteTag(
                item.AcceptTextures ? (sbyte)1 : (sbyte)0));
            list1.Add(tag1);
        }

        nbtTag.Add("servers", list1);
        string file = game.GetServersFile();
        using var outputStream = NbtConvert.CreateNbtStream(nbtTag);
        using var stream = File.Create(file);
        outputStream.CopyTo(stream);
    }

    /// <summary>
    /// 转换服务器储存
    /// </summary>
    /// <param name="tag">NBT标签</param>
    /// <returns>服务器储存</returns>
    private static ServerInfoObj ToServerInfo(NbtCompoundTag tag)
    {
        var info = new ServerInfoObj
        {
            Name = ((NbtStringTag)tag["name"]).Payload,
            IP = ((NbtStringTag)tag["ip"]).Payload,
            Icon = tag.TryGetValue("icon", out var data)
            ? ((NbtStringTag)data).Payload : null,
            AcceptTextures = tag.TryGetValue("acceptTextures", out var data1)
&& ((NbtByteTag)data1).Payload == 1
        };

        return info;
    }
}
