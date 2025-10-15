using System;
using System.Collections.Generic;

namespace ColorMC.Gui.Manager;

public static class EventManager
{
    private static readonly List<WeakReference<EventHandler<string>>> s_GameIconChange = [];

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

    public static void OnGameIconChange(string uuid)
    {
        foreach (var item in s_GameIconChange.ToArray())
        {
            //item.Invoke(null, uuid);

            if (item.TryGetTarget(out var target))
            {
                target.Invoke(null, uuid);
            }
            else
            {
                s_GameIconChange.Remove(item);
            }
        }
    }
}
