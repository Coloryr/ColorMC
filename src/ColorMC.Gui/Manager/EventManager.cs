using System;
using System.Collections.Generic;

namespace ColorMC.Gui.Manager;

public static class EventManager
{
    private static readonly List<EventHandler<Guid>> s_GameIconChange = [];
    private static readonly List<EventHandler<Guid>> s_GameNameChange = [];
    private static readonly List<EventHandler<Guid>> s_GameDelete = [];

    /// <summary>
    /// 游戏实例图标修改
    /// </summary>
    public static event EventHandler<Guid> GameIconChange
    {
        add
        {
            s_GameIconChange.Add(new(value));
        }
        remove
        {
            s_GameIconChange.Remove(new(value));
        }
    }

    /// <summary>
    /// 游戏实例名字修改
    /// </summary>
    public static event EventHandler<Guid> GameNameChange
    {
        add
        {
            s_GameNameChange.Add(new(value));
        }
        remove
        {
            s_GameNameChange.Remove(new(value));
        }
    }

    /// <summary>
    /// 游戏实例删除
    /// </summary>
    public static event EventHandler<Guid> GameDelete
    {
        add
        {
            s_GameDelete.Add(new(value));
        }
        remove
        {
            s_GameDelete.Remove(new(value));
        }
    }

    public static void OnGameIconChange(Guid uuid)
    {
        foreach (var item in s_GameIconChange.ToArray())
        {
            item.Invoke(null, uuid);
        }
    }

    public static void OnGameNameChange(Guid uuid)
    {
        foreach (var item in s_GameNameChange.ToArray())
        {
            item.Invoke(null, uuid);
        }
    }

    public static void OnGameDelete(Guid uuid)
    {
        foreach (var item in s_GameDelete.ToArray())
        {
            item.Invoke(null, uuid);
        }
    }
}
