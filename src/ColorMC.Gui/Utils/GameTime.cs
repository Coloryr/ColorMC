using ColorMC.Core.LaunchPath;
using ColorMC.Gui.UIBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.Utils;

public static class GameTime
{
    private readonly static Dictionary<string, DateTime> TimeList = new();
    private static bool IsRun;
    public static void Init()
    {
        IsRun = true;
        new Thread(Run).Start();
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

                    lock (TimeList)
                    {
                        if (TimeList.ContainsKey(item.Key))
                        {
                            TimeList[item.Key] = time;
                        }
                    }

                    game.LaunchData.GameTime += time - time1;
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

    public static void Launch(string uuid)
    {
        lock (TimeList)
        {
            if (TimeList.ContainsKey(uuid))
            {
                TimeList[uuid] = DateTime.Now;
            }
            else
            {
                TimeList.Add(uuid, DateTime.Now);
            }
        }
    }

    public static void Close()
    {
        IsRun = false;
    }

    public static void GameClose(string uuid)
    {
        lock (TimeList)
        {
            TimeList.Remove(uuid);
        }
    }
}
