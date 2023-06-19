using ColorMC.Core.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    private static readonly Dictionary<string, ConfigSaveObj> SaveQue = new();

    private static readonly object Lock = new();

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

        lock (Lock)
        {
            foreach (var item in SaveQue.Values)
            {
                File.WriteAllText(item.Local,
                    JsonConvert.SerializeObject(item.Obj, Formatting.Indented));
            }

            SaveQue.Clear();
        }
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

            lock (Lock)
            {
                foreach (var item in SaveQue.Values)
                {
                    if (new FileInfo(item.Local)?.Directory?.Exists == true)
                    {
                        try
                        {
                            File.WriteAllText(item.Local,
                                JsonConvert.SerializeObject(item.Obj, Formatting.Indented));
                        }
                        catch (Exception e)
                        {
                            Logs.Error(LanguageHelper.GetName("Core.Config.Error2"), e);
                        }
                    }
                }
                SaveQue.Clear();
            }
        }
    }

    /// <summary>
    /// 添加保存项目
    /// </summary>
    /// <param name="obj"></param>
    public static void AddItem(ConfigSaveObj obj)
    {
        lock (Lock)
        {
            if (!SaveQue.ContainsKey(obj.Name))
            {
                SaveQue.Add(obj.Name, obj);
            }
        }
    }
}