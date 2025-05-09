using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Input;
using ColorMC.Core;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs.Config;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.Joystick;

/// <summary>
/// 手柄绑定配置文件
/// </summary>
public static class JoystickConfig
{
    /// <summary>
    /// 配置文件列表
    /// </summary>
    public static readonly Dictionary<string, InputControlObj> Configs = [];

    /// <summary>
    /// 配置存储路径
    /// </summary>
    private static string s_local;

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        s_local = Path.Combine(ColorMCGui.BaseDir, GuiNames.NameJoyDir);

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
                using var stream = PathHelper.OpenRead(local);
                config = JsonUtils.ToObj(stream, JsonGuiType.InputControlObj);
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("Config.Error3"), e);
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
        ConfigSave.AddItem(ConfigSaveObj.Build(obj.UUID + Names.NameJsonExt, Path.Combine(s_local, $"{obj.UUID}{Names.NameJsonExt}"), obj, JsonGuiType.InputControlObj));
    }

    /// <summary>
    /// 保存控制配置
    /// </summary>
    /// <param name="obj"></param>
    public static void PutConfig(InputControlObj obj)
    {
        if (!Configs.TryAdd(obj.UUID, obj))
        {
            Configs[obj.UUID] = obj;
        }
    }

    /// <summary>
    /// 创建一个默认的配置
    /// </summary>
    /// <returns>手柄配置</returns>
    public static InputControlObj MakeInputControl()
    {
        return new()
        {
            UUID = FuntionUtils.NewUUID(),
            Keys = new()
            {
                { 0, new() { Key = Key.Q } },
                { 1, new() { Key = Key.F3 } },
                { 2, new() { Key = Key.F } },
                { 3, new() { Key = Key.E } },
                { 4, new() { Key = Key.Escape } },
                { 7, new() { Key = Key.LeftShift } },
                { 8, new() { MouseButton = MouseButton.Middle } },
                { 9, new() { Key = Key.Space } },
                { 10, new() { MouseButton = MouseButton.Left } }
            },
            AxisKeys = new()
            {
                { GenUUID(), new()
                    { InputKey = 1, Start = -4000, End = -32768, BackCancel = false, Key = Key.W } },
                { GenUUID(), new()
                    { InputKey = 1, Start = 4000, End = 32767, BackCancel = false, Key = Key.S } },
                { GenUUID(), new()
                    { InputKey = 0, Start = -4000, End = -32768, BackCancel = false, Key = Key.A } },
                { GenUUID(), new()
                    { InputKey = 0, Start = 4000, End = 32767, BackCancel = false, Key = Key.D } },
                { GenUUID(), new()
                    { InputKey = 5, Start = 2000, End = 32767, BackCancel = false, MouseButton = MouseButton.Right } },
                { GenUUID(), new()
                    { InputKey = 4, Start = 2000, End = 32767, BackCancel = false, Key = Key.LeftCtrl } },
            },
            CursorAxis = 0,
            RotateAxis = 1,
            CursorDeath = 3000,
            RotateDeath = 3000,
            CursorRate = 0.5f,
            RotateRate = 0.5f,
            DownRate = 800f,
            ItemCycle = true,
            ItemCycleLeft = 13,
            ItemCycleRight = 14
        };
    }

    /// <summary>
    /// 生成一个UUID
    /// </summary>
    /// <returns></returns>
    private static string GenUUID()
    {
        return FuntionUtils.NewUUID()[..8];
    }

    /// <summary>
    /// 删除配置
    /// </summary>
    /// <param name="obj">手柄配置</param>
    public static void Remove(InputControlObj obj)
    {
        Configs.Remove(obj.UUID);

        PathHelper.Delete(Path.Combine(s_local, $"{obj.UUID}{Names.NameJsonExt}"));
    }
}
