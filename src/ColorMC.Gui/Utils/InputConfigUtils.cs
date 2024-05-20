﻿using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Input;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using Newtonsoft.Json;

namespace ColorMC.Gui.Utils;

public static class InputConfigUtils
{
    public static readonly Dictionary<string, InputControlObj> Configs = [];

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
                { Guid.NewGuid().ToString().ToLower()[..8], new()
                    { InputKey = 1, Start = -4000, End = -32768, BackCancel = false, Key = Key.W } },
                { Guid.NewGuid().ToString().ToLower()[..8], new()
                    { InputKey = 1, Start = 4000, End = 32767, BackCancel = false, Key = Key.S } },
                { Guid.NewGuid().ToString().ToLower()[..8], new()
                    { InputKey = 0, Start = -4000, End = -32768, BackCancel = false, Key = Key.A } },
                { Guid.NewGuid().ToString().ToLower()[..8], new()
                    { InputKey = 0, Start = 4000, End = 32767, BackCancel = false, Key = Key.D } },
                { Guid.NewGuid().ToString().ToLower()[..8], new()
                    { InputKey = 5, Start = 2000, End = 32767, BackCancel = false, MouseButton = MouseButton.Right } },
                { Guid.NewGuid().ToString().ToLower()[..8], new()
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

    public static void Remove(InputControlObj obj)
    {
        Configs.Remove(obj.UUID);

        PathHelper.Delete(s_local + "/" + obj.UUID + ".json");
    }
}
