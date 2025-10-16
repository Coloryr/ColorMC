using System;
using System.Collections.Generic;

namespace ColorMC.Gui.Manager;

public static class EventManager
{
    private static readonly List<EventHandler<string>> s_GameIconChange = [];
    private static readonly List<EventHandler<string>> s_GameNameChange = [];
    private static readonly List<EventHandler<string>> s_GameDelete = [];

    /// <summary>
    /// 游戏实例图标修改
    /// </summary>
    public static event EventHandler<string> GameIconChange
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
    public static event EventHandler<string> GameNameChange
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
    public static event EventHandler<string> GameDelete
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

    public static void OnGameIconChange(string uuid)
    {
        foreach (var item in s_GameIconChange.ToArray())
        {
            item.Invoke(null, uuid);
        }
    }

    public static void OnGameNameChange(string uuid)
    {
        foreach (var item in s_GameNameChange.ToArray())
        {
            item.Invoke(null, uuid);
        }
    }

    public static void OnGameDelete(string uuid)
    {
        foreach (var item in s_GameDelete.ToArray())
        {
            item.Invoke(null, uuid);
        }
    }
}
