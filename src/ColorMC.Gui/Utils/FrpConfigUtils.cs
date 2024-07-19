using System;
using System.IO;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using Newtonsoft.Json;

namespace ColorMC.Gui.Utils;

public static class FrpConfigUtils
{
    public static FrpConfigObj Config { get; set; }

    private static string s_local;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行路径</param>
    public static void Init(string dir)
    {
        s_local = dir + "frp.json";

        Load(s_local, false);
    }

    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <param name="local">路径</param>
    /// <returns>是否加载成功</returns>
    public static bool Load(string local, bool exit)
    {
        if (File.Exists(local))
        {
            try
            {
                Config = JsonConvert.DeserializeObject<FrpConfigObj>(File.ReadAllText(local))!;
            }
            catch (Exception e)
            {
                if (exit)
                {
                    return false;
                }
                Logs.Error(App.Lang("Config.Error1"), e);
            }

            if (Config == null)
            {
                if (exit)
                {
                    return false;
                }

                Config = MakeDefaultConfig();

                SaveNow();
                return true;
            }

            bool save = false;

            if (Config.SakuraFrp == null)
            {
                if (exit)
                {
                    return false;
                }

                Config.SakuraFrp = new();
                save = true;
            }

            if (save)
            {
                Logs.Info(LanguageHelper.Get("Core.Config.Info2"));
                Save();
            }
        }
        else
        {
            Config = MakeDefaultConfig();

            SaveNow();
        }

        return true;
    }

    /// <summary>
    /// 立即保存
    /// </summary>
    public static void SaveNow()
    {
        Logs.Info(LanguageHelper.Get("Core.Config.Info2"));
        File.WriteAllText(s_local, JsonConvert.SerializeObject(Config));
    }

    /// <summary>
    /// 保存配置文件
    /// </summary>
    public static void Save()
    {
        ConfigSave.AddItem(new()
        {
            Name = "frp.json",
            Local = s_local,
            Obj = Config
        });
    }

    public static FrpConfigObj MakeDefaultConfig()
    {
        return new()
        {
            SakuraFrp = new()
        };
    }
}
