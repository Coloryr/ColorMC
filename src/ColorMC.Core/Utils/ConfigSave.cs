using ColorMC.Core.Helpers;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace ColorMC.Core.Utils;

/// <summary>
/// 配置文件保存
/// </summary>
public static class ConfigSave
{
    public record ConfigSaveObj
    {
        public string Name;
        public object Obj;
        public string Local;
    }

    private static readonly ConcurrentBag<ConfigSaveObj> s_saveQue = new();

    private static Thread t_thread;
    private static bool s_run;

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        ColorMCCore.Stop += Stop;

        t_thread = new(Run)
        {
            Name = "ColorMC_Save"
        };
        s_run = true;
        t_thread.Start();
    }

    /// <summary>
    /// 停止
    /// </summary>
    private static void Stop()
    {
        s_run = false;

        Save();
    }

    /// <summary>
    /// 开始执行
    /// </summary>
    private static void Run()
    {
        int count = 0;
        while (s_run)
        {
            Thread.Sleep(100);
            if (count < 10)
            {
                count++;
                continue;
            }

            count = 0;
            if (!s_saveQue.Any())
            {
                continue;
            }

            Save();
        }
    }

    private static void Save()
    {
        Dictionary<string, ConfigSaveObj> list = new();
        lock (s_saveQue)
        {
            while (s_saveQue.TryTake(out var item))
            {
                if (list.ContainsKey(item.Name))
                {
                    list[item.Name] = item;
                }
                else
                {
                    list.Add(item.Name, item);
                }
            }
            s_saveQue.Clear();
        }

        foreach (var item in list.Values)
        {
            var info = new FileInfo(item.Local);
            info.Directory?.Create();
            try
            {
                File.WriteAllText(item.Local,
                    JsonConvert.SerializeObject(item.Obj, Formatting.Indented));
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.Get("Core.Config.Error2"), e);
            }
        }
    }

    /// <summary>
    /// 添加保存项目
    /// </summary>
    /// <param name="obj"></param>
    public static void AddItem(ConfigSaveObj obj)
    {
        lock (s_saveQue)
        {
            s_saveQue.Add(obj);
        }
    }
}