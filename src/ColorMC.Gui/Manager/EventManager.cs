using System;
using System.Collections.Generic;
using ColorMC.Gui.Objs;

namespace ColorMC.Gui.Manager;

public static class EventManager
{
    private static readonly List<Action<Guid>> s_gameIconChange = [];
    private static readonly List<Action<Guid>> s_gameNameChange = [];
    private static readonly List<Action<Guid>> s_gameDelete = [];
    private static readonly List<Action<SourceItemObj>> s_modpackInstall = [];
    private static readonly List<Action<SourceItemObj, bool>> s_modpackStop = [];
    private static readonly List<Action<Guid, SourceItemObj>> s_resourceInstall = [];
    private static readonly List<Action<Guid, SourceItemObj, bool>> s_resourceStop = [];
    private static readonly List<Action<Guid>> s_throwResourceDownload = [];

    /// <summary>
    /// 游戏实例图标修改
    /// </summary>
    public static event Action<Guid> GameIconChange
    {
        add
        {
            s_gameIconChange.Add(value);
        }
        remove
        {
            s_gameIconChange.Remove(value);
        }
    }

    /// <summary>
    /// 游戏实例名字修改
    /// </summary>
    public static event Action<Guid> GameNameChange
    {
        add
        {
            s_gameNameChange.Add(value);
        }
        remove
        {
            s_gameNameChange.Remove(value);
        }
    }

    /// <summary>
    /// 游戏实例删除
    /// </summary>
    public static event Action<Guid> GameDelete
    {
        add
        {
            s_gameDelete.Add(value);
        }
        remove
        {
            s_gameDelete.Remove(value);
        }
    }

    /// <summary>
    /// 开始安装整合包
    /// </summary>
    public static event Action<SourceItemObj> ModpackInstall
    {
        add
        {
            s_modpackInstall.Add(value);
        }
        remove
        {
            s_modpackInstall.Remove(value);
        }
    }

    /// <summary>
    /// 结束安装整合包
    /// </summary>
    public static event Action<SourceItemObj, bool> ModpackStop
    {
        add
        {
            s_modpackStop.Add(value);
        }
        remove
        {
            s_modpackStop.Remove(value);
        }
    }

    /// <summary>
    /// 开始安装资源
    /// </summary>
    public static event Action<Guid, SourceItemObj> ResourceInstall
    {
        add
        {
            s_resourceInstall.Add(value);
        }
        remove
        {
            s_resourceInstall.Remove(value);
        }
    }

    /// <summary>
    /// 结束安装资源
    /// </summary>
    public static event Action<Guid, SourceItemObj, bool> ResourceStop
    {
        add
        {
            s_resourceStop.Add(value);
        }
        remove
        {
            s_resourceStop.Remove(value);
        }
    }

    /// <summary>
    /// 其他窗口关闭资源下载
    /// </summary>
    public static event Action<Guid> ThrowResourceDownload
    {
        add
        {
            s_throwResourceDownload.Add(value);
        }
        remove
        {
            s_throwResourceDownload.Remove(value);
        }
    }

    public static void OnGameIconChange(Guid uuid)
    {
        foreach (var item in s_gameIconChange.ToArray())
        {
            item.Invoke(uuid);
        }
    }

    public static void OnGameNameChange(Guid uuid)
    {
        foreach (var item in s_gameNameChange.ToArray())
        {
            item.Invoke(uuid);
        }
    }

    public static void OnGameDelete(Guid uuid)
    {
        foreach (var item in s_gameDelete.ToArray())
        {
            item.Invoke(uuid);
        }
    }

    public static void OnModpackInstall(SourceItemObj obj)
    {
        foreach (var item in s_modpackInstall.ToArray())
        {
            item.Invoke(obj);
        }
    }

    public static void OnModpackStop(SourceItemObj obj, bool res)
    {
        foreach (var item in s_modpackStop.ToArray())
        {
            item.Invoke(obj, res);
        }
    }

    public static void OnResourceInstall(Guid game, SourceItemObj obj)
    {
        foreach (var item in s_resourceInstall.ToArray())
        {
            item.Invoke(game, obj);
        }
    }

    public static void OnResourceStop(Guid game, SourceItemObj obj, bool res)
    {
        foreach (var item in s_resourceStop.ToArray())
        {
            item.Invoke(game, obj, res);
        }
    }

    public static void OnThrowResourceDownload(Guid game)
    {
        foreach (var item in s_throwResourceDownload.ToArray())
        {
            item.Invoke(game);
        }
    }
}
