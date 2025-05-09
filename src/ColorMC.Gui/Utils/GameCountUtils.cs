using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Nbt;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs.Config;

namespace ColorMC.Gui.Utils;

/// <summary>
/// 游戏统计
/// </summary>
public static class GameCountUtils
{
    private static string s_local;
    private static bool s_isSave;
    private static bool s_isRun;

    private static readonly object s_lock = new();
    private static readonly Dictionary<string, DateTime> s_timeList = [];
    private static readonly Dictionary<string, TimeSpan> s_spanTimeList = [];

    /// <summary>
    /// 统计数据
    /// </summary>
    public static CountObj Count { get; private set; }

    /// <summary>
    /// 初始化游戏统计
    /// </summary>
    public static void Init()
    {
        s_local = Path.Combine(ColorMCGui.BaseDir, GuiNames.NameCountDataFile);

        s_isRun = true;

        App.OnClose += Count_Stop;

        new Thread(Run)
        {
            Name = "ColorMC_Count"
        }.Start();

        Read();
    }

    private static void Count_Stop()
    {
        s_isRun = false;
    }

    /// <summary>
    /// 读统计数据
    /// </summary>
    private static async void Read()
    {
        if (!File.Exists(s_local))
        {
            Count = new()
            {
                GameRuns = [],
                LaunchLogs = []
            };
            Save();
            return;
        }
        try
        {
            if (await NbtBase.Read<NbtCompound>(s_local) is { } nbt)
            {
                Count = new()
                {
                    LaunchCount = nbt.TryGet<NbtLong>("LaunchCount")!.Value,
                    LaunchDoneCount = nbt.TryGet<NbtLong>("LaunchDoneCount")!.Value,
                    LaunchErrorCount = nbt.TryGet<NbtLong>("LaunchErrorCount")!.Value,
                    AllTime = TimeSpan.FromTicks(nbt.TryGet<NbtLong>("AllTime")!.Value),
                    GameRuns = [],
                    LaunchLogs = []
                };

                var list = nbt.TryGet<NbtList>("GameRuns")!;
                foreach (var item in list.Cast<NbtCompound>())
                {
                    var key = item.TryGet<NbtString>("Key")!.Value;
                    var list1 = item.TryGet<NbtList>("List");
                    var list2 = new List<CountObj.GameTime>();
                    foreach (var item1 in list1!.Cast<NbtCompound>())
                    {
                        var start = item1.TryGet<NbtLong>("StartTime")!.Value;
                        var stop = item1.TryGet<NbtLong>("StopTime")!.Value;
                        list2.Add(new()
                        {
                            StartTime = new DateTime(start),
                            StopTime = new DateTime(stop)
                        });
                    }
                    Count.GameRuns.Add(key, list2);
                }

                list = nbt.TryGet<NbtList>("LaunchLogs")!;
                foreach (var item in list.Cast<NbtCompound>())
                {
                    var key = item.TryGet<NbtString>("Key")!.Value;
                    var list1 = item.TryGet<NbtList>("List");
                    var list2 = new List<CountObj.LaunchLog>();
                    foreach (var item1 in list1!.Cast<NbtCompound>())
                    {
                        var time = item1.TryGet<NbtLong>("Time")!.Value;
                        var error = item1.TryGet<NbtByte>("Error")!.Value;
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
            Logs.Error(LanguageHelper.Get("App.Error2"), e);
        }

        if (Count == null)
        {
            Count = new()
            {
                GameRuns = [],
                LaunchLogs = []
            };
        }
    }

    /// <summary>
    /// 统计线程
    /// </summary>
    private static void Run()
    {
        int a = 0;
        while (s_isRun)
        {
            Thread.Sleep(100);

            foreach (var item in new Dictionary<string, DateTime>(s_timeList))
            {
                var game = InstancesPath.GetGame(item.Key);
                if (game == null)
                {
                    continue;
                }

                var time = DateTime.Now;
                var time1 = item.Value;
                var span = time - time1;

                lock (s_timeList)
                {
                    if (s_timeList.ContainsKey(item.Key))
                    {
                        s_timeList[item.Key] = time;
                    }
                }

                lock (s_spanTimeList)
                {
                    if (s_spanTimeList.ContainsKey(item.Key))
                    {
                        s_spanTimeList[item.Key] += span;
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
    }

    /// <summary>
    /// 保存统计数据
    /// </summary>
    public static void Save()
    {
        if (Count == null)
        {
            return;
        }

        lock (s_lock)
        {
            if (s_isSave)
                return;

            s_isSave = true;
        }
        var nbt = new NbtCompound()
        {
            { "LaunchCount", new NbtLong() { Value = Count.LaunchCount } },
            { "LaunchDoneCount", new NbtLong() { Value = Count.LaunchDoneCount } },
            { "LaunchErrorCount", new NbtLong() { Value = Count.LaunchErrorCount } },
            { "AllTime", new NbtLong(){ Value = Count.AllTime.Ticks } }
        };

        var list = new NbtList() { InNbtType = NbtType.NbtCompound };
        foreach (var item in Count.GameRuns)
        {
            var com = new NbtCompound()
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
            var com = new NbtCompound()
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

        nbt.ZipType = ZipType.GZip;

        nbt.Save(s_local);

        s_isSave = false;
    }

    /// <summary>
    /// 游戏实例启动完毕
    /// </summary>
    /// <param name="game">游戏实例</param>
    public static void LaunchDone(GameSettingObj game)
    {
        if (Count == null)
        {
            return;
        }
        var uuid = game.UUID;
        var now = DateTime.Now;
        lock (s_timeList)
        {
            if (!s_timeList.TryAdd(uuid, now))
            {
                s_timeList[uuid] = now;
            }

            if (s_spanTimeList.ContainsKey(uuid))
            {
                s_spanTimeList[uuid] = new TimeSpan(0);
            }
            else
            {
                s_spanTimeList.Add(uuid, new TimeSpan(0));
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
                Count.GameRuns.Add(uuid, [time]);
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
                Count.LaunchLogs.Add(uuid, [log]);
            }

            Task.Run(Save);
        }
    }

    /// <summary>
    /// 启动失败
    /// </summary>
    /// <param name="game">游戏实例</param>
    public static void LaunchError(string uuid)
    {
        if (Count == null)
        {
            return;
        }
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
                Count.LaunchLogs.Add(uuid, [log]);
            }

            Task.Run(Save);
        }
    }

    /// <summary>
    /// 游戏实例关闭
    /// </summary>
    /// <param name="game">游戏实例</param>
    public static void GameClose(GameSettingObj game)
    {
        if (Count == null)
        {
            return;
        }
        var uuid = game.UUID;
        var time = DateTime.Now;
        lock (s_timeList)
        {
            s_timeList.Remove(uuid);
        }

        lock (s_spanTimeList)
        {
            if (s_spanTimeList.Remove(uuid, out var span))
            {
                game.LaunchData.LastPlay = span;
                game.SaveLaunchData();
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
