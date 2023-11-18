using System;
using System.Collections.Generic;

namespace ColorMC.Gui.Utils.LaunchSetting;

/// <summary>
/// 文本获取 
/// </summary>
public static class Localizer
{
    private static readonly Dictionary<string, List<IObserver<string>>> s_langList = [];

    public static IDisposable Add(string key, IObserver<string> observer)
    {
        if (s_langList.TryGetValue(key, out var list))
        {
            list.Add(observer);
        }
        else
        {
            list = [observer];
            s_langList.Add(key, list);
        }
        var value = App.Lang(key);
        observer.OnNext(value);
        return new Unsubscribe(list, observer);
    }

    private class Unsubscribe(List<IObserver<string>> observers, IObserver<string> observer) : IDisposable
    {
        public void Dispose()
        {
            observers?.Remove(observer);
        }
    }

    public static void Reload()
    {
        foreach (var item in s_langList)
        {
            var value = App.Lang(item.Key);
            foreach (var item1 in item.Value)
            {
                item1.OnNext(value);
            }
        }
    }
}