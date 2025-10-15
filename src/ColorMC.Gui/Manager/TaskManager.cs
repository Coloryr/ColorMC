using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.Manager;

public static class TaskManager
{
    private record EventLink
    {
        public object? Value;
        public List<Semaphore> Semaphores = [];
    }

    private static readonly ConcurrentDictionary<string, EventLink> s_mutexTasks = [];

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
