using System;
using System.Collections.Generic;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.Manager;

/// <summary>
/// 文本获取 
/// </summary>
public static class LangMananger
{
    /// <summary>
    /// 存储的本地化翻译
    /// </summary>
    private static readonly Dictionary<string, List<WeakReference<IObserver<string>>>> s_langList = [];

    /// <summary>
    /// 添加一个本地化获取UI绑定
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="observer">UI更新器</param>
    /// <returns></returns>
    public static IDisposable Add(string key, IObserver<string> observer)
    {
        if (s_langList.TryGetValue(key, out var list))
        {
            list.Add(new(observer));
        }
        else
        {
            list = [new(observer)];
            s_langList.Add(key, list);
        }
        var value = LanguageUtils.Get(key);
        observer.OnNext(value);
        return new Unsubscribe(list, observer);
    }

    private class Unsubscribe(List<WeakReference<IObserver<string>>> observers, IObserver<string> observer) : IDisposable
    {
        public void Dispose()
        {
            foreach (var item in observers.ToArray())
            {
                if (!item.TryGetTarget(out var target)
                    || target == observer)
                {
                    observers.Remove(item);
                }
            }
        }
    }

    /// <summary>
    /// 更新UI本地化
    /// </summary>
    public static void Reload()
    {
        foreach (var item in s_langList)
        {
            var value = LanguageUtils.Get(item.Key);
            foreach (var item1 in item.Value)
            {
                if (item1.TryGetTarget(out var target))
                {
                    target.OnNext(value);
                }
            }
        }
    }

    /// <summary>
    /// 清理UI绑定器
    /// </summary>
    public static void Remove()
    {
        foreach (var item in s_langList.Values)
        {
            foreach (var item1 in item.ToArray())
            {
                if (!item1.TryGetTarget(out _))
                {
                    item.Remove(item1);
                }
            }
        }
    }
}