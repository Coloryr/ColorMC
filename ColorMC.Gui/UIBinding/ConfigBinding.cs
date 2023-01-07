using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        return GuiConfigUtils.Load(dir, true);
    }

    public static (ConfigObj, GuiConfigObj) GetAllConfig()
    {
        return (ConfigUtils.Config, GuiConfigUtils.Config);
    }

    public static void DeleteGuiImageConfig()
    {
        App.RemoveImage();
        GuiConfigUtils.Config.BackImage = null;
        GuiConfigUtils.Save();
        App.AddCurseForgeWindow?.Update();
        App.AddGameWindow?.Update();
        App.DownloadWindow?.Update();
        App.MainWindow?.Update();
        App.SettingWindow?.Update();
    }

    public static async Task SetGuiConfig(GuiConfigObj obj)
    {
        if (!await App.LoadImage(obj.BackImage, obj.BackEffect))
        {
            App.RemoveImage();
        }

        GuiConfigUtils.Config = obj;
        GuiConfigUtils.Save();
        App.AddCurseForgeWindow?.Update();
        App.AddGameWindow?.Update();
        App.DownloadWindow?.Update();
        App.MainWindow?.Update();
        App.SettingWindow?.Update();
    }

    public static void SetHttpConfig(HttpObj obj)
    {
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
}
