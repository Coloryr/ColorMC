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
    private static readonly List<Action> s_skinChange = [];
    private static readonly List<Action> s_bgChange = [];
    private static readonly List<Action<bool>> s_lastUserChange = [];
    private static readonly List<Action> s_lockUserChange = [];

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
    /// 皮肤修改
    /// </summary>
    public static event Action SkinChange
    {
        add
        {
            s_skinChange.Add(value);
        }
        remove
        {
            s_skinChange.Remove(value);
        }
    }

    /// <summary>
    /// 背景图修改
    /// </summary>
    public static event Action BGChange
    {
        add
        {
            s_bgChange.Add(value);
        }
        remove
        {
            s_bgChange.Remove(value);
        }
    }

    /// <summary>
    /// 选中的账户修改
    /// </summary>
    public static event Action<bool> LastUserChange
    {
        add
        {
            s_lastUserChange.Add(value);
        }
        remove
        {
            s_lastUserChange.Remove(value);
        }
    }

    /// <summary>
    /// 锁定账户类型修改
    /// </summary>
    public static event Action LockUserChange
    {
        add
        {
            s_lockUserChange.Add(value);
        }
        remove
        {
            s_lockUserChange.Remove(value);
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

    public static void OnSkinChange()
    {
        foreach (var item in s_skinChange.ToArray())
        {
            item.Invoke();
        }
    }

    public static void OnBGChange()
    {
        foreach (var item in s_bgChange.ToArray())
        {
            item.Invoke();
        }
    }

    public static void OnLastUserChange(bool user)
    {
        foreach (var item in s_lastUserChange.ToArray())
        {
            item.Invoke(user);
        }
    }

    public static void OnLockUserChange()
    {
        foreach (var item in s_lockUserChange.ToArray())
        {
            item.Invoke();
        }
    }
}
