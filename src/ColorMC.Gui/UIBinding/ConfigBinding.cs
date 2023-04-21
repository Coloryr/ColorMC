using ColorMC.Core.Helpers;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Utils.LaunchSetting;
using System.Threading.Tasks;

namespace ColorMC.Gui.UIBinding;

public static class ConfigBinding
{
    public static bool LoadAuthDatabase(string dir)
    {
        return AuthDatabase.LoadData(dir);
    }

    public static bool LoadConfig(string dir)
    {
        return ConfigUtils.Load(dir, true);
    }

    public static bool LoadGuiConfig(string dir)
    {
        var res = GuiConfigUtils.Load(dir, true);
        if (res)
        {
            ColorSel.Instance.Load();
            FontSel.Instance.Load();
        }

        return res;
    }

    public static (ConfigObj, GuiConfigObj) GetAllConfig()
    {
        return (ConfigUtils.Config, GuiConfigUtils.Config);
    }

    public static void SetRgb(bool enable)
    {
        GuiConfigUtils.Config.RGB = enable;

        GuiConfigUtils.Save();
        ColorSel.Instance.Load();
    }

    public static void SetRgb(int v1, int v2)
    {
        GuiConfigUtils.Config.RGBS = v1;
        GuiConfigUtils.Config.RGBV = v2;
        GuiConfigUtils.Save();
        ColorSel.Instance.Load();
    }

    public static void SetColor(string main, string back, string back1, string font1, string font2, string back2, string back3, string font3, string font4)
    {
        GuiConfigUtils.Config.ColorMain = main;
        GuiConfigUtils.Config.ColorLight.ColorBack = back;
        GuiConfigUtils.Config.ColorLight.ColorTranBack = back1;
        GuiConfigUtils.Config.ColorLight.ColorFont1 = font1;
        GuiConfigUtils.Config.ColorLight.ColorFont2 = font2;
        GuiConfigUtils.Config.ColorDark.ColorBack = back2;
        GuiConfigUtils.Config.ColorDark.ColorTranBack = back3;
        GuiConfigUtils.Config.ColorDark.ColorFont1 = font3;
        GuiConfigUtils.Config.ColorDark.ColorFont2 = font4;
        GuiConfigUtils.Save();
        ColorSel.Instance.Load();
    }

    public static void ResetColor()
    {
        SetColor(
            ColorSel.MainColorStr,
            ColorSel.BackLigthColorStr,
            ColorSel.Back1LigthColorStr,
            ColorSel.ButtonLightFontStr,
            ColorSel.FontLigthColorStr,
            ColorSel.BackDarkColorStr,
            ColorSel.Back1DarkColorStr,
            ColorSel.ButtonDarkFontStr,
            ColorSel.FontDarkColorStr
        );
    }

    public static void DeleteGuiImageConfig()
    {
        App.RemoveImage();
        GuiConfigUtils.Config.BackImage = null;
        GuiConfigUtils.Save();
        App.OnPicUpdate();
    }

    public static async Task SetBackPic(string dir, int data)
    {
        GuiConfigUtils.Config.BackEffect = data;
        GuiConfigUtils.Config.BackImage = dir;
        GuiConfigUtils.Save();

        await App.LoadImage();

        App.OnPicUpdate();
    }

    public static async Task SetBackLimit(bool enable, int pix)
    {
        GuiConfigUtils.Config.BackLimitValue = pix;
        GuiConfigUtils.Config.BackLimit = enable;
        GuiConfigUtils.Save();

        await App.LoadImage();

        App.OnPicUpdate();
    }

    public static void SetBackTran(int data)
    {
        GuiConfigUtils.Config.BackTran = data;
        GuiConfigUtils.Save();

        App.OnPicUpdate();
    }

    public static void SetBl(bool open, int type)
    {
        GuiConfigUtils.Config.WindowTranType = type;
        GuiConfigUtils.Config.WindowTran = open;
        GuiConfigUtils.Save();

        App.OnPicUpdate();
    }

    public static void SetHttpConfig(HttpObj obj)
    {
        if (DownloadManager.State != CoreRunState.End)
            return;

        ConfigUtils.Config.Http = obj;
        ConfigUtils.Save();

        BaseClient.Init();
    }

    public static void SetJvmArgConfig(JvmArgObj obj)
    {
        ConfigUtils.Config.DefaultJvmArg = obj;
        ConfigUtils.Save();

        BaseClient.Init();
    }

    public static void SetWindowSettingConfig(WindowSettingObj obj)
    {
        ConfigUtils.Config.Window = obj;
        ConfigUtils.Save();

        BaseClient.Init();
    }

    public static void SetServerCustom(ServerCustom obj)
    {
        GuiConfigUtils.Config.ServerCustom = obj;
        GuiConfigUtils.Save();

        App.MainWindow?.Load();

        ColorSel.Instance.Load();
    }

    public static void SetGameCheckConfig(GameCheckObj obj)
    {
        ConfigUtils.Config.GameCheck = obj;
        ConfigUtils.Save();
    }

    public static void SetFont(string? name, bool def)
    {
        GuiConfigUtils.Config.FontName = name;
        GuiConfigUtils.Config.FontDefault = def;

        GuiConfigUtils.Save();

        FontSel.Instance.Load();
    }

    public static void ResetConfig()
    {
        GuiConfigUtils.Config = GuiConfigUtils.MakeDefaultConfig();

        GuiConfigUtils.Save();
    }

    public static void SetLaunchCloseConfig(bool value)
    {
        GuiConfigUtils.Config.CloseBeforeLaunch = value;

        GuiConfigUtils.Save();
    }

    public static void SetWindowMode(bool value)
    {
        GuiConfigUtils.Config.WindowMode = value;

        GuiConfigUtils.Save();
    }

    public static bool WindowMode()
    {
        return GuiConfigUtils.Config.WindowMode || SystemInfo.Os == OsType.Android;
    }

    public static void SetLanguage(LanguageType type)
    {
        ConfigUtils.Config.Language = type;
        ConfigUtils.Save();

        LanguageHelper.Change(type);
    }

    public static void SetColorType(ColorType type)
    {
        GuiConfigUtils.Config.ColorType = type;
        GuiConfigUtils.Save();

        App.ColorChange();
        ColorSel.Instance.Load();
        App.OnPicUpdate();
    }
}
