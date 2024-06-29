using System;
using System.IO;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Utils.LaunchSetting;
using Newtonsoft.Json;

namespace ColorMC.Gui.Utils;

/// <summary>
/// GUI配置文件
/// </summary>
public static class GuiConfigUtils
{
    public static GuiConfigObj Config { get; set; }

    private static string s_local;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行路径</param>
    public static void Init(string dir)
    {
        s_local = dir + "gui.json";

        Load(s_local);
    }

    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <param name="local">路径</param>
    /// <param name="quit">加载失败是否退出</param>
    /// <returns>是否加载成功</returns>
    public static bool Load(string local, bool quit = false)
    {
        if (File.Exists(local))
        {
            try
            {
                Config = JsonConvert.DeserializeObject<GuiConfigObj>(File.ReadAllText(local))!;
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("Gui.Error17"), e);
            }

            if (Config == null)
            {
                if (quit)
                {
                    return false;
                }

                Config = MakeDefaultConfig();

                SaveNow();
                return true;
            }

            bool save = false;

            if (Config.ServerCustom == null)
            {
                Config.ServerCustom = MakeServerCustomConfig();
                save = true;
            }
            if (Config.Render == null
                || Config.Render.Windows == null
                || Config.Render.X11 == null)
            {
                Config.Render = MakeRenderConfig();
                save = true;
            }
            if (Config.Live2D == null)
            {
                Config.Live2D = MakeLive2DConfig();
                save = true;
            }
            if (Config.Gui == null)
            {
                Config.Gui = MakeGuiSettingConfig();
                save = true;
            }
            if (Config.Style == null)
            {
                Config.Style = MakeStyleSettingConfig();
                save = true;
            }
            if (Config.Input == null)
            {
                Config.Input = new();
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
            Name = "gui.json",
            Local = s_local,
            Obj = Config
        });
    }

    public static StyleSetting MakeStyleSettingConfig()
    {
        return new()
        {
            AmTime = 500
        };
    }

    public static Live2DSetting MakeLive2DConfig()
    {
        return new()
        {
            Width = 30,
            Height = 50
        };
    }

    public static Render MakeRenderConfig()
    {
        return new()
        {
            Windows = new()
            {
                ShouldRenderOnUIThread = null
            },
            X11 = new()
            {
                UseDBusMenu = null,
                UseDBusFilePicker = null,
                OverlayPopups = null
            }
        };
    }

    public static GuiConfigObj MakeDefaultConfig()
    {
        return new()
        {
            ColorMain = ColorSel.MainColorStr,
            RGBS = 100,
            RGBV = 100,
            ServerCustom = MakeServerCustomConfig(),
            FontDefault = true,
            Render = MakeRenderConfig(),
            BackLimitValue = 50,
            EnableBG = false,
            BackImage = "",
            Live2D = MakeLive2DConfig(),
            Gui = MakeGuiSettingConfig(),
            Style = MakeStyleSettingConfig(),
            Input = new()
        };
    }

    public static ServerCustom MakeServerCustomConfig()
    {
        return new()
        {
            MotdColor = "White",
            MotdBackColor = "Black",
            Volume = 30
        };
    }

    public static MainWindowSetting MakeGuiSettingConfig()
    {
        return new()
        {

        };
    }
}