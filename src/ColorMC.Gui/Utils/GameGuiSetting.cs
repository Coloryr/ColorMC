using System.Collections.Generic;
using System.IO;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using Newtonsoft.Json;

namespace ColorMC.Gui.Utils;

public static class GameGuiSetting
{
    public const string Name = "guisetting.json";

    private readonly static Dictionary<string, GameGuiSettingObj> s_datas = [];

    public static GameGuiSettingObj ReadConfig(GameSettingObj obj)
    {
        if (s_datas.TryGetValue(obj.UUID, out var setting))
        {
            return setting;
        }

        var path = obj.GetBasePath();
        var dir = Path.GetFullPath($"{path}/{Name}");

        if (File.Exists(dir))
        {
            try
            {
                var data = PathHelper.ReadText(dir);
                var obj1 = JsonConvert.DeserializeObject<GameGuiSettingObj>(data!);
                if (obj1 != null)
                {
                    obj1.Log ??= MakeLog();
                    obj1.Mod ??= MakeMod();
                    obj1.ModName ??= [];
                    s_datas.Add(obj.UUID, obj1);
                    return obj1;
                }
            }
            catch
            {

            }
        }

        return Make();
    }

    public static void WriteConfig(GameSettingObj obj, GameGuiSettingObj config)
    {
        if (!s_datas.TryAdd(obj.UUID, config))
        {
            s_datas[obj.UUID] = config;
        }

        var dir = Path.GetFullPath($"{obj.GetBasePath()}/{Name}");

        ConfigSave.AddItem(new()
        {
            Local = dir,
            Name = "GameLogSetting:" + obj.UUID,
            Obj = config
        });
    }

    private static GameGuiSettingObj Make()
    {
        return new()
        {
            Log = MakeLog(),
            Mod = MakeMod(),
            ModName = []
        };
    }

    private static GameLogSettingObj MakeLog()
    {
        return new()
        {
            Auto = true,
            EnableDebug = true,
            EnableError = true,
            EnableInfo = true,
            EnableNone = true,
            EnableWarn = true,
            WordWrap = true
        };
    }

    private static GameModSettingObj MakeMod()
    {
        return new()
        {
            EnableModId = true,
            EnableLoader = true,
            EnableSide = true,
            EnableVersion = true,
            EnableName = true
        };
    }
}
