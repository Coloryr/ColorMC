using System.Collections.Generic;
using System.IO;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Config;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs.Config;

namespace ColorMC.Gui.Utils;

/// <summary>
/// 游戏实例与界面相关存储
/// </summary>
public static class GameGuiSetting
{
    /// <summary>
    /// 界面设置
    /// </summary>
    private readonly static Dictionary<string, GameGuiSettingObj> s_datas = [];

    /// <summary>
    /// 获取游戏实例对应的界面设置
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns></returns>
    public static GameGuiSettingObj ReadConfig(GameSettingObj obj)
    {
        if (s_datas.TryGetValue(obj.UUID, out var setting))
        {
            return setting;
        }

        var path = obj.GetBasePath();
        var dir = Path.Combine(path, GuiNames.NameGameGuiConfigFile);

        if (File.Exists(dir))
        {
            try
            {
                using var data = PathHelper.OpenRead(dir);
                var obj1 = JsonUtils.ToObj(data, JsonGuiType.GameGuiSettingObj);
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

    /// <summary>
    /// 保存游戏实例对应的界面设置
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <param name="config">配置</param>
    public static void WriteConfig(GameSettingObj obj, GameGuiSettingObj config)
    {
        if (!s_datas.TryAdd(obj.UUID, config))
        {
            s_datas[obj.UUID] = config;
        }

        var dir = Path.Combine(obj.GetBasePath(), GuiNames.NameGameGuiConfigFile);

        ConfigSave.AddItem(ConfigSaveObj.Build("GameLogSetting:" + obj.UUID, dir, config, JsonGuiType.GameGuiSettingObj));
    }

    //创建基础配置
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
