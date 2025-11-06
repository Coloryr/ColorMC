using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using ColorMC.Core.GuiHandel;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI;

public class ZipGui(BaseModel model) : IZipGui
{
    private readonly string _text = LanguageUtils.Get("AddJavaWindow.Info5");

    public void Unzip()
    {
        Dispatcher.UIThread.Post(() =>
        {
            model.ProgressUpdate(_text);
        });
    }

    public async Task<bool> FileRename(string? text)
    {
        model.ProgressClose();
        var test = await model.ShowAsync(string.Format(LanguageUtils.Get("App.Text33"), text));
        model.Progress();
        return test;
    }

    public void ZipUpdate(string text, int size, int all)
    {
        if (text.Length > 40)
        {
            text = "..." + text[^40..];
        }
        Dispatcher.UIThread.Post(() =>
        {
            model.ProgressUpdate($"{_text} {text} {size}/{all}");
            model.ProgressUpdate((double)size / all * 100);
        });
    }
}

public class OverGameGui(BaseModel model) : IOverGameGui
{
    public async Task<bool> GameOverwirte(GameSettingObj obj)
    {
        model.ProgressClose();
        var test = await model.ShowAsync(
            string.Format(LanguageUtils.Get("AddGameWindow.Info2"), obj.Name));
        model.Progress();
        return test;
    }

    public Task<bool> InstanceNameReplace()
    {
        return model.ShowAsync(LanguageUtils.Get("Core.Info43"));
    }
}

public class CreateGameGui(BaseModel model) : ICreateInstanceGui
{
    public async Task<bool> GameOverwirte(GameSettingObj obj)
    {
        model.ProgressClose();
        var test = await model.ShowAsync(
            string.Format(LanguageUtils.Get("AddGameWindow.Info2"), obj.Name));
        return test;
    }

    public Task<bool> InstanceNameReplace()
    {
        return model.ShowAsync(LanguageUtils.Get("Core.Info43"));
    }

    public void ModPackState(CoreRunState state)
    {
        if (state == CoreRunState.Read)
        {
            model.Progress(LanguageUtils.Get("AddGameWindow.Tab2.Info1"));
        }
        else if (state == CoreRunState.Init)
        {
            model.ProgressUpdate(LanguageUtils.Get("AddGameWindow.Tab2.Info2"));
        }
        else if (state == CoreRunState.GetInfo)
        {
            model.ProgressUpdate(LanguageUtils.Get("AddGameWindow.Tab2.Info3"));
        }
        else if (state == CoreRunState.Download)
        {
            model.ProgressUpdate(-1);
            if (!ConfigBinding.WindowMode())
            {
                model.ProgressUpdate(LanguageUtils.Get("AddGameWindow.Tab2.Info4"));
            }
            else
            {
                model.ProgressClose();
            }
        }
        else if (state == CoreRunState.DownloadDone)
        {
            if (ConfigBinding.WindowMode())
            {
                model.Progress(LanguageUtils.Get("AddGameWindow.Tab2.Info4"));
            }
        }
    }

    public void StateUpdate(int now, int count)
    {
        Dispatcher.UIThread.Post(() =>
        {
            model.ProgressUpdate((double)now / count * 100);
        });
    }
}

public class UpdateLaunchGui(BaseModel model) : IUpdateGui
{
    public void StateUpdate(GameSettingObj obj, LaunchState state)
    {
        if (state == LaunchState.CheckServerPack)
        {
            model.ProgressUpdate(LanguageUtils.Get("App.Text28"));
        }
        else if (state == LaunchState.DownloadServerPack)
        {
            model.ProgressUpdate(LanguageUtils.Get("App.Text29"));
        }
        else if (state == LaunchState.DownloadServerPackDone)
        {
            model.ProgressUpdate(LanguageUtils.Get("App.Text30"));
        }
    }
}

public class LauncherGui(BaseModel model) : ILaunchGui
{
    public Task<bool> LaunchProcess(bool pre)
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
                     model.ShowAsync(pre ? LanguageUtils.Get("App.Text17") : LanguageUtils.Get("App.Text18")));
    }

    public void StateUpdate(GameSettingObj obj, LaunchState state)
    {
        Dispatcher.UIThread.Post(() =>
        {
            string text = LanguageUtils.Get(state switch
            {
                LaunchState.Loging => "App.Text6",
                LaunchState.Checking => "App.Text7",
                LaunchState.Downloading => "App.Text13",
                LaunchState.JvmPrepare => "App.Text14",
                LaunchState.LaunchPre => "App.Text19",
                LaunchState.LaunchPost => "App.Text20",
                LaunchState.LoadServerPack => "App.Text31",
                LaunchState.CheckServerPack => "App.Text28",
                LaunchState.DownloadServerPack => "App.Text29",
                LaunchState.DownloadServerPackDone => "App.Text30",
                _ => ""
            });
            if (GuiConfigUtils.Config.CloseBeforeLaunch)
            {
                if (state == LaunchState.End)
                {
                    model.ProgressClose();
                }
                model.ProgressUpdate(text);
            }
            else
            {
                model.SubTitle = text;
            }
        });
    }

    public Task<bool> LoginFail(LoginObj obj)
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
        {
            return model.ShowAsync(string.Format(
                LanguageUtils.Get("App.Text16"), obj.UserName));
        });
    }

    public void NoJava(int version)
    {
        Dispatcher.UIThread.Post(() =>
        {
            WindowManager.ShowSetting(SettingType.SetJava, version);
        });
    }

    public Task<bool> RequestDownload()
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
        {
            return model.ShowAsync(LanguageUtils.Get("BaseBinding.Info3"));
        });
    }

    public Task<bool> ServerPackUpgrade(string text)
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
        {
            return model.TextAsync(LanguageUtils.Get("BaseBinding.Info2"), text ?? "");
        });
    }
}
