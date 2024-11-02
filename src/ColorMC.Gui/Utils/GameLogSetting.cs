using System.Collections.Generic;
using System.IO;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using Newtonsoft.Json;

namespace ColorMC.Gui.Utils;

public static class GameLogSetting
{
    public const string Name = "logsetting.json";

    private static Dictionary<string, GameLogSettingObj> s_datas;

    public static GameLogSettingObj ReadConfig(GameSettingObj obj)
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
                var obj1 = JsonConvert.DeserializeObject<GameLogSettingObj>(data!);
                if (obj1 != null)
                {
                    s_datas.Add(obj.UUID, obj1);
                    return obj1;
                }
            }
            catch
            {

            }
        }

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

    public static void WriteConfig(GameSettingObj obj, GameLogSettingObj config)
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
}
