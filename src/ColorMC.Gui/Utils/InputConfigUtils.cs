using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ColorMC.Gui.Utils;

public static class InputConfigUtils
{
    public static readonly Dictionary<string, InputControlObj> Configs = [];

    public static InputControlObj? NowConfig;

    private static string s_local;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行路径</param>
    public static void Init(string dir)
    {
        s_local = dir + "/inputs";

        Directory.CreateDirectory(s_local);

        var list = Directory.GetFiles(s_local);
        foreach (var item in list)
        {
            var obj = Load(item);
            if (obj != null)
            {
                Configs.TryAdd(obj.UUID, obj);
            }
        }

        var uuid = GuiConfigUtils.Config.Input.NowConfig;
        if (!string.IsNullOrWhiteSpace(uuid))
        {
            Configs.TryGetValue(uuid, out NowConfig);
        }
    }

    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <param name="local">路径</param>
    /// <returns>是否加载成功</returns>
    public static InputControlObj? Load(string local)
    {
        InputControlObj? config = null;
        if (File.Exists(local))
        {
            try
            {
                config = JsonConvert.DeserializeObject<InputControlObj>(File.ReadAllText(local))!;
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("Gui.Error50"), e);
            }

            if (config == null || config.Keys == null
                || config.AxisKeys == null)
            {
                return null;
            }
        }

        return config;
    }

    /// <summary>
    /// 保存配置文件
    /// </summary>
    public static void Save(InputControlObj obj)
    {
        ConfigSave.AddItem(new()
        {
            Name = obj.UUID + ".json",
            Local = s_local + "/" + obj.UUID + ".json",
            Obj = obj
        });
    }

    public static void PutConfig(InputControlObj obj)
    {
        if (!Configs.TryAdd(obj.UUID, obj))
        {
            Configs[obj.UUID] = obj;
        }
    }

    public static InputControlObj MakeInputControl()
    {
        return new()
        {
            UUID = Guid.NewGuid().ToString().ToLower(),
            Keys = [],
            AxisKeys = [],
            CursorAxis = 0,
            RotateAxis = 1,
            CursorDeath = 200,
            RotateDeath = 200,
            CursorRate = 0.5f,
            RotateRate = 0.5f
        };
    }

    public static void Remove(InputControlObj obj)
    {
        Configs.Remove(obj.UUID);

        PathHelper.Delete(s_local + "/" + obj.UUID + ".json");
    }
}
