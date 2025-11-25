using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using ColorMC.Core.GuiHandle;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI;

public class TopModPackGui : IModPackGui
{
    private bool _isRun;
    private bool _haveUpdate;

    private int _size, _all;
    private BaseModel _model;

    public TopModPackGui(BaseModel model)
    {
        _model = model;
        DispatcherTimer.Run(Run, TimeSpan.FromMilliseconds(100));
    }

    public void SetNow(int value, int all)
    {
        _model.ProgressUpdate((double)value / all * 100);
    }

    public void SetNowProcess(int value, int all)
    {
        _size = value;
        _all = all;
        _haveUpdate = true;
    }

    public void SetNowSub(int value, int all)
    {
        //_model.ProgressUpdate((double)value / all);
    }

    public void SetStateText(ModpackState state)
    {
        _model.ProgressUpdate(state.GetName());
    }

    public void SetText(string? text)
    {
        //_info.Info = text;
    }

    private bool Run()
    {
        if (_haveUpdate)
        {
            _model.ProgressUpdate((double)_size / _all * 100);
        }
        _haveUpdate = false;
        return _isRun;
    }

    public void Stop()
    {
        _isRun = false;
    }
}

public class ZipGui : IZipGui
{
    private readonly string _text = LanguageUtils.Get("AddJavaWindow.Text11");

    private readonly BaseModel _model;

    private bool _isRun;
    private bool _haveUpdate;

    private string _name;
    private int _size, _all;

    public ZipGui(BaseModel model)
    {
        _model = model;
        _isRun = true;
        DispatcherTimer.Run(Run, TimeSpan.FromMilliseconds(100));
    }

    public void Unzip()
    {
        Dispatcher.UIThread.Post(() =>
        {
            _model.ProgressUpdate(_text);
        });
    }

    public async Task<bool> FileRename(string? text)
    {
        _model.ProgressClose();
        var test = await _model.ShowAsync(string.Format(LanguageUtils.Get("App.Text33"), text));
        _model.Progress();
        return test;
    }

    public void ZipUpdate(string text, int size, int all)
    {
        if (text.Length > 40)
        {
            text = "..." + text[^40..];
        }
        _name = text;
        _all = all;
        _size = size;

        _haveUpdate = true;
    }

    private bool Run()
    {
        if (_haveUpdate)
        {
            _model.ProgressUpdate($"{_text} {_name} {_size}/{_all}");
            _model.ProgressUpdate((double)_size / _all * 100);
        }
        _haveUpdate = false;
        return _isRun;
    }

    public void Stop()
    {
        _isRun = false;
    }

    public void Done()
    {
        _haveUpdate = false;
    }
}

public class OverGameGui(BaseModel model) : IOverGameGui
{
    public async Task<bool> GameOverwirte(GameSettingObj obj)
    {
        model.ProgressClose();
        var test = await model.ShowAsync(
            string.Format(LanguageUtils.Get("AddGameWindow.Text3"), obj.Name));
        model.Progress();
        return test;
    }

    public Task<bool> InstanceNameReplace()
    {
        return model.ShowAsync(LanguageUtils.Get("AddGameWindow.Text4"));
    }
}

public class CreateGameGui(BaseModel model) : ICreateInstanceGui
{
    public async Task<bool> GameOverwirte(GameSettingObj obj)
    {
        model.ProgressClose();
        var test = await model.ShowAsync(
            string.Format(LanguageUtils.Get("AddGameWindow.Text3"), obj.Name));
        model.Progress();
        return test;
    }

    public Task<bool> InstanceNameReplace()
    {
        return model.ShowAsync(LanguageUtils.Get("AddGameWindow.Text4"));
    }

    public void ModPackState(CoreRunState state)
    {
        if (state == CoreRunState.Read)
        {
            model.Progress(LanguageUtils.Get("AddGameWindow.Tab2.Text4"));
        }
        else if (state == CoreRunState.Init)
        {
            model.ProgressUpdate(LanguageUtils.Get("AddGameWindow.Tab2.Text5"));
        }
        else if (state == CoreRunState.GetInfo)
        {
            model.ProgressUpdate(-1);
            model.ProgressUpdate(LanguageUtils.Get("AddGameWindow.Tab2.Text6"));
        }
        else if (state == CoreRunState.Download)
        {
            model.ProgressUpdate(-1);
            if (!ConfigBinding.WindowMode())
            {
                model.ProgressUpdate(LanguageUtils.Get("AddGameWindow.Tab2.Text7"));
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
                model.Progress(LanguageUtils.Get("AddGameWindow.Tab2.Text7"));
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
            return model.ShowAsync(LanguageUtils.Get("App.Text36"));
        });
    }

    public Task<bool> ServerPackUpgrade(string text)
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
        {
            return model.TextAsync(LanguageUtils.Get("App.Text35"), text ?? "");
        });
    }
}

public class ModpackGui : IModPackGui
{
    private readonly FileItemDownloadModel _info;

    private bool _isRun;
    private bool _haveUpdate;

    private int _size, _all;

    public ModpackGui(FileItemDownloadModel info)
    {
        _info = info;
        DispatcherTimer.Run(Run, TimeSpan.FromMilliseconds(100));
    }

    public void SetNow(int value, int all)
    {
        _info.Now = (double)value / all;
    }

    public void SetNowProcess(int value, int all)
    {
        _size = value;
        _all = all;
        _haveUpdate = true;
    }

    public void SetNowSub(int value, int all)
    {
        _info.NowSub = (double)value / all;
    }

    public void SetStateText(ModpackState state)
    {
        _info.Info = state.GetName();
        if (state is ModpackState.GetInfo or ModpackState.Unzip or ModpackState.DownloadFile)
        {
            _info.ShowSub = true;
        }
        else
        {
            _info.ShowSub = false;
        }
    }

    public void SetText(string? text)
    {
        _info.Info = text;
    }

    private bool Run()
    {
        if (_haveUpdate)
        {
            _info.NowSub = (double)_size / _all;
        }
        _haveUpdate = false;
        return _isRun;
    }

    public void Stop()
    {
        _isRun = false;
    }
}