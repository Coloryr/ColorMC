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

    private static readonly ConcurrentBag<ConfigSaveObj> SaveQue = new();

    private static Thread thread;
    private static bool run;

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        ColorMCCore.Stop += Stop;

        thread = new(Run)
        {
            Name = "ColorMC-Save"
        };
        run = true;
        thread.Start();
    }

    /// <summary>
    /// 停止
    /// </summary>
    private static void Stop()
    {
        run = false;
        thread.Join();

        Save();
    }

    /// <summary>
    /// 开始执行
    /// </summary>
    private static void Run()
    {
        while (run)
        {
            Thread.Sleep(1000);
            if (!SaveQue.Any())
                continue;

            Save();
        }
    }

    private static void Save()
    {
        Dictionary<string, ConfigSaveObj> list = new();
        lock (SaveQue)
        {
            list.Clear();
            while (SaveQue.TryTake(out var item))
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
            SaveQue.Clear();
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
        lock (SaveQue)
        {
            SaveQue.Add(obj);
        }
    }
}