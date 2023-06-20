using ColorMC.Core.LaunchPath;
using ColorMC.Core.Nbt;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.Utils;

public static class GameCountUtils
{
    private const string Name = "count.dat";
    private static string Local;
    private static bool IsSave;
    private static bool IsRun;

    private static readonly object Lock = new();
    private readonly static Dictionary<string, DateTime> TimeList = new();
    private readonly static Dictionary<string, TimeSpan> SpanTimeList = new();

    public static CountObj Count { get; private set; }

    public static void Init(string local)
    {
        Local = Path.GetFullPath(local + Name);

        IsRun = true;
        new Thread(Run).Start();

        Read();
    }

    private static void Read()
    {
        if (File.Exists(Local))
        {
            try
            {
                if (NbtBase.Read(Local) is NbtCompound nbt)
                {
                    Count = new()
                    {
                        LaunchCount = (nbt.TryGet("LaunchCount") as NbtLong)!.Value,
                        LaunchDoneCount = (nbt.TryGet("LaunchDoneCount") as NbtLong)!.Value,
                        LaunchErrorCount = (nbt.TryGet("LaunchErrorCount") as NbtLong)!.Value,
                        AllTime = TimeSpan.FromTicks((nbt.TryGet("AllTime") as NbtLong)!.Value),
                        GameRuns = new(),
                        LaunchLogs = new()
                    };

                    var list = nbt.TryGet("GameRuns") as NbtList;
                    foreach (var item in list!.Cast<NbtCompound>())
                    {
                        var key = (item.TryGet("Key") as NbtString)!.Value;
                        var list1 = item.TryGet("List") as NbtList;
                        var list2 = new List<CountObj.GameTime>();
                        foreach (var item1 in list1!.Cast<NbtCompound>())
                        {
                            var start = (item1.TryGet("StartTime") as NbtLong)!.Value;
                            var stop = (item1.TryGet("StopTime") as NbtLong)!.Value;
                            list2.Add(new()
                            {
                                StartTime = new DateTime(start),
                                StopTime = new DateTime(stop)
                            });
                        }
                        Count.GameRuns.Add(key, list2);
                    }

                    list = nbt.TryGet("LaunchLogs") as NbtList;
                    foreach (var item in list!.Cast<NbtCompound>())
                    {
                        var key = (item.TryGet("Key") as NbtString)!.Value;
                        var list1 = item.TryGet("List") as NbtList;
                        var list2 = new List<CountObj.LaunchLog>();
                        foreach (var item1 in list1!.Cast<NbtCompound>())
                        {
                            var time = (item1.TryGet("Time") as NbtLong)!.Value;
                            var error = (item1.TryGet("Error") as NbtByte)!.Value;
                            list2.Add(new()
                            {
                                Time = new DateTime(time),
                                Error = error == 1
                            });
                        }
                        Count.LaunchLogs.Add(key, list2);
                    }
                }
            }
            catch (Exception e)
            {
                string text = App.GetLanguage("Gui.Error27");
                Logs.Error(text, e);
                App.ShowError(text, e);
            }
        }

        if (Count == null)
        {
            Count = new()
            {
                GameRuns = new(),
                LaunchLogs = new()
            };
            Save();
        }
    }

    private static void Run()
    {
        int a = 0;
        while (IsRun)
        {
            if (BaseBinding.RunGames.Count > 0)
            {
                foreach (var item in new Dictionary<string, DateTime>(TimeList))
                {
                    var game = InstancesPath.GetGame(item.Key);
                    if (game == null)
                        continue;

                    var time = DateTime.Now;
                    var time1 = item.Value;
                    var span = time - time1;

                    lock (TimeList)
                    {
                        if (TimeList.ContainsKey(item.Key))
                        {
                            TimeList[item.Key] = time;
                        }
                    }

                    lock (SpanTimeList)
                    {
                        if (SpanTimeList.ContainsKey(item.Key))
                        {
                            SpanTimeList[item.Key] += span;
                        }
                    }

                    game.LaunchData.GameTime += span;
                    Count.AllTime += span;
                    if (a >= 10)
                    {
                        a = 0;
                        game.SaveLaunchData();
                    }
                }
                a++;
            }
            Thread.Sleep(1000);
        }
    }

    public static void Save()
    {
        lock (Lock)
        {
            if (IsSave)
                return;

            IsSave = true;
        }
        NbtCompound nbt = new()
        {
            { "LaunchCount", new NbtLong() { Value = Count.LaunchCount } },
            { "LaunchDoneCount", new NbtLong() { Value = Count.LaunchDoneCount } },
            { "LaunchErrorCount", new NbtLong() { Value = Count.LaunchErrorCount } },
            { "AllTime", new NbtLong(){ Value = Count.AllTime.Ticks } }
        };

        var list = new NbtList() { InNbtType = NbtType.NbtCompound };
        foreach (var item in Count.GameRuns)
        {
            NbtCompound com = new()
            {
                { "Key", new NbtString() { Value = item.Key } },
            };
            var list1 = new NbtList() { InNbtType = NbtType.NbtCompound };
            foreach (var item1 in item.Value)
            {
                list1.Add(new NbtCompound()
                {
                    { "StartTime", new NbtLong() { Value = item1.StartTime.Ticks } },
                    { "StopTime", new NbtLong() { Value = item1.StopTime.Ticks } }
                });
            }
            com.Add("List", list1);
            list.Add(com);
        }
        nbt.Add("GameRuns", list);

        list = new NbtList() { InNbtType = NbtType.NbtCompound };
        foreach (var item in Count.LaunchLogs)
        {
            NbtCompound com = new()
            {
                { "Key", new NbtString(){ Value = item.Key } },
            };
            var list1 = new NbtList() { InNbtType = NbtType.NbtCompound };
            foreach (var item1 in item.Value)
            {
                list1.Add(new NbtCompound()
                {
                    { "Time", new NbtLong() { Value = item1.Time.Ticks } },
                    { "Error", new NbtByte() { Value = item1.Error ? (byte)1 : (byte)0 } }
                });
            }
            com.Add("List", list1);
            list.Add(com);
        }
        nbt.Add("LaunchLogs", list);

        nbt.Gzip = true;

        nbt.Save(Local);

        IsSave = false;
    }

    public static void LaunchDone(string uuid)
    {
        var now = DateTime.Now;
        lock (TimeList)
        {
            if (TimeList.ContainsKey(uuid))
            {
                TimeList[uuid] = now;
            }
            else
            {
                TimeList.Add(uuid, now);
            }

            if (SpanTimeList.ContainsKey(uuid))
            {
                SpanTimeList[uuid] = new TimeSpan(0);
            }
            else
            {
                SpanTimeList.Add(uuid, new TimeSpan(0));
            }
        }

        lock (Count)
        {
            Count.LaunchCount++;
            Count.LaunchDoneCount++;
            var time = new CountObj.GameTime()
            {
                Now = true,
                StartTime = now,
                StopTime = now
            };
            if (Count.GameRuns.TryGetValue(uuid, out var list))
            {
                list.Add(time);
            }
            else
            {
                Count.GameRuns.Add(uuid, new() { time });
            }

            var log = new CountObj.LaunchLog()
            {
                Time = now,
                Error = false
            };
            if (Count.LaunchLogs.TryGetValue(uuid, out var list1))
            {
                list1.Add(log);
            }
            else
            {
                Count.LaunchLogs.Add(uuid, new() { log });
            }

            Task.Run(Save);
        }
    }

    public static void LaunchError(string uuid)
    {
        lock (Count)
        {
            Count.LaunchCount++;
            Count.LaunchErrorCount++;
            var log = new CountObj.LaunchLog()
            {
                Time = DateTime.Now,
                Error = false
            };
            if (Count.LaunchLogs.TryGetValue(uuid, out var list1))
            {
                list1.Add(log);
            }
            else
            {
                Count.LaunchLogs.Add(uuid, new() { log });
            }

            Task.Run(Save);
        }
    }

    public static void Close()
    {
        IsRun = false;
    }

    public static void GameClose(string uuid)
    {
        var time = DateTime.Now;
        lock (TimeList)
        {
            TimeList.Remove(uuid);
        }

        lock (SpanTimeList)
        {
            if (SpanTimeList.Remove(uuid, out var span))
            {
                var game = InstancesPath.GetGame(uuid);
                if (game != null)
                {
                    game.LaunchData.LastPlay = span;
                    game.SaveLaunchData();
                }
            }
        }

        lock (Count)
        {
            if (Count.GameRuns.TryGetValue(uuid, out var list))
            {
                var item = list.FirstOrDefault(a => a.Now);
                if (item != null)
                {
                    item.Now = false;
                    item.StopTime = time;
                }
            }

            Task.Run(Save);
        }
    }
}
