using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace ColorMC.Gui.Utils;

/// <summary>
/// Frp配置文件管理
/// </summary>
public static class FrpConfigUtils
{
    /// <summary>
    /// Frp映射相关配置文件
    /// </summary>
    public static FrpConfigObj Config { get; set; }

    /// <summary>
    /// 配置文件存储路径
    /// </summary>
    private static string s_local;

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        s_local = Path.Combine(ColorMCGui.BaseDir, GuiNames.NameFrpConfigFile);

        Load(s_local, false);
    }

    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <param name="local">路径</param>
    /// <param name="exit">加载失败后直接返回</param>
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

            if (Config.OpenFrp == null)
            {
                if (exit)
                {
                    return false;
                }

                Config.OpenFrp = new();
                save = true;
            }

            if (Config.SelfFrp == null)
            {
                if (exit)
                {
                    return false;
                }

                Config.SelfFrp = [];
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
            File = s_local,
            Obj = Config
        });
    }

    /// <summary>
    /// 创建一个默认的Frp配置文件
    /// </summary>
    /// <returns>Frp配置</returns>
    public static FrpConfigObj MakeDefaultConfig()
    {
        return new()
        {
            SakuraFrp = new(),
            OpenFrp = new(),
            SelfFrp = []
        };
    }

    /// <summary>
    /// 添加一个自定义Frp
    /// </summary>
    /// <param name="obj"></param>
    public static void AddSelfFrp(FrpSelfObj obj)
    {
        Config.SelfFrp.Add(obj);
        Save();
    }

    /// <summary>
    /// 删除一个自定义Frp
    /// </summary>
    /// <param name="obj"></param>
    public static void RemoveSelfFrp(FrpSelfObj obj)
    {
        if (Config.SelfFrp.Remove(obj) || Config.SelfFrp.RemoveAll(item => item.IsSame(obj)) > 0)
        {
            Save();
        }
    }

    /// <summary>
    /// 修改一个自定义Frp
    /// </summary>
    /// <param name="obj"></param>
    public static void EditSelfFrp(FrpSelfObj obj)
    {
        var item = Config.SelfFrp.FirstOrDefault(item => item.Name == obj.Name);
        if (item != null)
        {
            item.IP = obj.IP;
            item.Key = obj.Key;
            item.NetPort = obj.NetPort;

            Save();
        }
    }
}
