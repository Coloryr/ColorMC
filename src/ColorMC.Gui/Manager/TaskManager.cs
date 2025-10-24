using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.Manager;

/// <summary>
/// 任务管理
/// </summary>
public static class TaskManager
{
    private record EventLink
    {
        /// <summary>
        /// 任务结果
        /// </summary>
        public object? Value;
        /// <summary>
        /// 等待链
        /// </summary>
        public List<Semaphore> Semaphores = [];
    }

    /// <summary>
    /// 互斥任务列表
    /// </summary>
    private static readonly ConcurrentDictionary<string, EventLink> s_mutexTasks = [];

    /// <summary>
    /// 开启一个互斥任务
    /// </summary>
    /// <param name="key">任务名</param>
    /// <returns>是否已经有正在运行的任务</returns>
    public static Task<object?>? StartMutexTask(string key)
    {
        lock (s_mutexTasks)
        {
            if (s_mutexTasks.TryGetValue(key, out var value))
            {
                return MakeMutex(key);
            }

            s_mutexTasks.TryAdd(key, new EventLink());
        }

        return null;
    }

    /// <summary>
    /// 关闭互斥任务
    /// </summary>
    /// <param name="key">任务键</param>
    /// <param name="value">任务结果</param>
    /// <exception cref="Exception">若没有任务则报错</exception>
    public static void StopMutexTask(string key, object? value)
    {
        if (!s_mutexTasks.TryGetValue(key, out var link))
        {
            throw new Exception($"Event key: {key} is not exist");
        }

        link.Value = value;
        foreach (var item in link.Semaphores)
        {
            item.Release();
        }
        s_mutexTasks.Remove(key, out _);
    }

    /// <summary>
    /// 创建任务锁
    /// </summary>
    /// <param name="key">任务键</param>
    /// <returns></returns>
    private static Task<object?>? MakeMutex(string key)
    {
        var sem = new Semaphore(0, 2);
        s_mutexTasks[key].Semaphores.Add(sem);

        return Task.Run(() =>
        {
            sem.WaitOne();
            if (s_mutexTasks.TryGetValue(key, out var value))
            {
                return value.Value;
            }

            return null;
        });
    }
}
