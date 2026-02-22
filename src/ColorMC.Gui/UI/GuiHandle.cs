using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using ColorMC.Core.GuiHandle;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.User;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI;

public class LoginGui(WindowModel window) : ILoginGui
{
    /// <summary>
    /// 登录账户选择
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public async Task<int> SelectAuth(List<string> items)
    {
        var dialog = new SelectModel(window.WindowId)
        {
            Text = LangUtils.Get("UserWindow.Text36"),
            Items = [.. items]
        };
        var res = await window.ShowDialogWait(dialog);
        if (res is not true)
        {
            return -1;
        }
        return dialog.Index;
    }
}

public class LoginOAuthGui(UsersModel model, ProgressModel? progress) : ILoginOAuthGui
{
    public CancellationToken Token => model.Token;

    private InputModel _showDialog;

    public void LoginOAuthCode(string? url, string code)
    {
        Close();
        _showDialog = new InputModel(model.Window.WindowId)
        {
            Text1 = string.Format(LangUtils.Get("UserWindow.Text15"), url),
            Text2 = string.Format(LangUtils.Get("UserWindow.Text16"), code),
            Text2Visable = true,
            TextReadonly = true,
            CancelVisible = false,
            CancelEnable = false,
            ConfirmEnable = false,
            ChoiseCall = model.SetCancel,
            ChoiseText = LangUtils.Get("Button.Cancel"),
            ChoiseVisible = true
        };
        model.Window.ShowDialog(_showDialog);
        BaseBinding.OpenUrl($"{url}?otc={code}");
        var top = model.Window.GetTopLevel();
        if (top == null)
        {
            return;
        }
        BaseBinding.CopyTextClipboard(top, code);
    }

    public void Close()
    {
        if (_showDialog != null)
        {
            model.Window.CloseDialog(_showDialog);
        }
    }

    public void LoginOAuthState(AuthState state)
    {
        progress?.Text = string.Format(LangUtils.Get("UserWindow.Text22"), state.GetName());
    }
}

public class TopModPackGui : IAddGui
{
    private readonly ProgressModel _progress;

    private bool _isRun;
    private bool _haveUpdate;

    private int _size, _all;

    public TopModPackGui(ProgressModel progress)
    {
        _progress = progress;
        _isRun = true;
        DispatcherTimer.Run(Run, TimeSpan.FromMilliseconds(100));
    }

    public void SetNow(int value, int all)
    {
        _progress.Indeterminate = false;
        _progress.Value = (double)value / all * 100;
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

    public void SetState(AddState state)
    {
        _progress.Text = state.GetName();
    }

    public void SetSubText(string? text)
    {
        //_info.Info = text;
    }

    private bool Run()
    {
        if (_haveUpdate)
        {
            if (_all == 0)
            {
                _all = 1;
            }
            var temp = (double)_size / _all * 100;
            if (temp > 100)
            {
                temp = 100;
            }
            _progress.Value = 100;
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
    private readonly string _text = LangUtils.Get("AddJavaWindow.Text11");

    private readonly WindowModel _model;
    private readonly ProgressModel _progress;

    private bool _isRun;
    private bool _haveUpdate;

    private string _name;
    private int _size, _all;

    public ZipGui(WindowModel model, ProgressModel progress)
    {
        _model = model;
        _progress = progress;
        _isRun = true;
        DispatcherTimer.Run(Run, TimeSpan.FromMilliseconds(100));
    }

    public void Unzip()
    {
        Dispatcher.UIThread.Post(() =>
        {
            _progress.Text = _text;
        });
    }

    public Task<bool> FileRename(string? text)
    {
        return _model.ShowChoice(string.Format(LangUtils.Get("App.Text33"), text));
    }

    public void ZipUpdate(string text, int size, int all)
    {
        UIUtils.StringCut(ref text);
        _name = text;
        _all = all;
        _size = size;

        _haveUpdate = true;
    }

    private bool Run()
    {
        if (_haveUpdate)
        {
            _progress.Text = $"{_text} {_name} {_size}/{_all}";
            _progress.Value = (double)_size / _all * 100;
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

public class OverGameGui(WindowModel model) : IOverGameGui
{
    public Task<bool> GameOverwirte(GameSettingObj obj)
    {
        return model.ShowChoice(string.Format(LangUtils.Get("AddGameWindow.Text3"), obj.Name));
    }

    public Task<bool> InstanceNameReplace()
    {
        return model.ShowChoice(LangUtils.Get("AddGameWindow.Text4"));
    }
}

public class UpdateLaunchGui(ProgressModel progress) : IUpdateGui
{
    public void StateUpdate(GameSettingObj obj, LaunchState state)
    {
        if (state == LaunchState.CheckServerPack)
        {
            progress.Text = LangUtils.Get("App.Text28");
        }
        else if (state == LaunchState.DownloadServerPack)
        {
            progress.Text = LangUtils.Get("App.Text29");
        }
        else if (state == LaunchState.DownloadServerPackDone)
        {
            progress.Text = LangUtils.Get("App.Text30");
        }
    }
}

public class LauncherGui(WindowModel model, ProgressModel? progress) : ILaunchGui
{
    public Task<bool> LaunchProcess(bool pre)
    {
        return Dispatcher.UIThread.InvokeAsync(() =>
        {
            return model.ShowChoice(pre ? LangUtils.Get("App.Text17") : LangUtils.Get("App.Text18"));
        });
    }

    public void StateUpdate(GameSettingObj obj, LaunchState state)
    {
        Dispatcher.UIThread.Post(() =>
        {
            string text = LangUtils.Get(state switch
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
            if (progress != null)
            {
                progress.Text = text;
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
            return model.ShowChoice(string.Format(
                LangUtils.Get("App.Text16"), obj.UserName));
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
            return model.ShowChoice(LangUtils.Get("App.Text36"));
        });
    }

    public async Task<bool> ServerPackUpgrade(string text)
    {
        var dialog = new LongTextModel(model.WindowId)
        {
            Text1 = LangUtils.Get("App.Text36"),
            Text2 = text ?? "",
            CancelEnable = true
        };

        return await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            return await model.ShowDialogWait(dialog) is true;
        });
    }
}

public class ResourceGui : IAddGui
{
    private readonly FileItemDownloadModel _info;

    private bool _isRun;
    private bool _haveUpdate;

    private int _size, _all;

    public ResourceGui(FileItemDownloadModel info)
    {
        _info = info;
        _isRun = true;
        DispatcherTimer.Run(Run, TimeSpan.FromMilliseconds(100));

        _info.Info = LangUtils.Get("AddResourceWindow.Text36");
    }

    public void SetNow(int value, int all)
    {

    }

    public void SetNowProcess(int value, int all)
    {
        _size = value;
        _all = all;
        _haveUpdate = true;
    }

    public void SetNowSub(int value, int all)
    {

    }

    public void SetState(AddState state)
    {

    }

    public void SetSubText(string? text)
    {

    }

    private bool Run()
    {
        if (_haveUpdate)
        {
            if (_all == 0)
            {
                _all = 1;
            }
            var temp = (double)_size / _all * 100;
            if (temp > 100)
            {
                temp = 100;
            }
            _info.Now = temp;
        }
        _haveUpdate = false;
        return _isRun;
    }

    public void Stop()
    {
        _isRun = false;
    }
}

public class ModPackGui : IAddGui
{
    private readonly FileItemDownloadModel _info;

    private bool _isRun;
    private bool _haveUpdate;

    private string? _text;

    private int _size, _all;

    public ModPackGui(FileItemDownloadModel info)
    {
        _info = info;
        _isRun = true;
        DispatcherTimer.Run(Run, TimeSpan.FromMilliseconds(100));
    }

    public void SetNow(int value, int all)
    {
        _info.Now = (double)value / all * 100;
    }

    public void SetNowProcess(int value, int all)
    {
        _size = value;
        _all = all;
        _haveUpdate = true;
    }

    public void SetNowSub(int value, int all)
    {
        _size = value;
        _all = all;
        _haveUpdate = true;
    }

    public void SetState(AddState state)
    {
        _info.Info = state.GetName();
        if (state is AddState.DownloadPack or AddState.GetInfo
            or AddState.Unzip or AddState.DownloadFile)
        {
            _info.ShowSub = true;
        }
        else
        {
            _info.ShowSub = false;
        }

        if (state == AddState.GetInfo)
        {
            _info.SubInfo = LangUtils.Get("App.Text115");
            _haveUpdate = false;
        }
        else if (state == AddState.DownloadFile)
        {
            _info.SubInfo = LangUtils.Get("AddGameWindow.Tab2.Text7");
            _haveUpdate = false;
        }
    }

    public void SetSubText(string? text)
    {
        _text = text;
        _haveUpdate = true;
    }

    private bool Run()
    {
        if (_haveUpdate)
        {
            if (_all == 0)
            {
                _all = 1;
            }
            var temp = (double)_size / _all * 100;
            if (temp > 100)
            {
                temp = 100;
            }
            _info.NowSub = temp;
            _info.SubInfo = _text;
        }
        _haveUpdate = false;
        return _isRun;
    }

    public void Stop()
    {
        _isRun = false;
    }
}