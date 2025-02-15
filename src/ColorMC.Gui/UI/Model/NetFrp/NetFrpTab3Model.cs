using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using ColorMC.Core.Net.Motd;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.NetFrp;

public partial class NetFrpModel
{
    private readonly List<string> _isOut = [];

    [ObservableProperty]
    private bool _isRuning;
    [ObservableProperty]
    private bool _isOk;

    [ObservableProperty]
    private TextDocument _text = new();

    public string? Temp { get; private set; } = "";

    private Process? _process;
    private readonly object Lock = new();

    private string _remoteIP;
    private string _localIP;
    private bool _isSend;
    private bool _isStoping;

    private NetFrpLocalModel _now;

    partial void OnIsOkChanged(bool value)
    {
        RemoveClick();
        SetTab3Click();
    }

    partial void OnIsRuningChanged(bool value)
    {
        RemoveClick();
        SetTab3Click();
    }

    private void SetProcess(Process process, NetFrpLocalModel model, string ip)
    {
        _now = model;
        if (_process != null)
        {
            Stop();
        }

        _isSend = false;
        _process = process;
        _remoteIP = ip;
        _localIP = model.Port;

        if (SystemInfo.Os == OsType.Windows)
        {
            _process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
            _process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
        }

        _process.Exited += Process_Exited;
        _process.OutputDataReceived += Process_OutputDataReceived;
        _process.ErrorDataReceived += Process_ErrorDataReceived;
        try
        {
            _process.Start();
            _process.BeginErrorReadLine();
            _process.BeginOutputReadLine();
            IsRuning = true;
        }
        catch
        {
            IsRuning = false;
            Model.Show(App.Lang("NetFrpWindow.Tab3.Error2"));
        }

        Text = new();
    }

    private void Process_Exited(object? sender, EventArgs e)
    {
        IsRuning = false;

        _now.IsStart = false;
    }

    private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        Log(e.Data);
    }

    private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (_isSend)
        {
            return;
        }
        Log(e.Data);
        if (e.Data?.Contains("TCP 隧道启动成功") == true
            || e.Data?.Contains("Your TCP proxy is available now") == true
            || e.Data?.Contains("来连接服务, 或使用IP地址(不推荐)") == true
            || e.Data?.Contains("或使用 IP 地址连接") == true
            || e.Data?.Contains(" start proxy success") == true)
        {
            _isOut.Add(_localIP);
            _isSend = true;
            Dispatcher.UIThread.Post(() =>
            {
                IsOk = true;
                Model.InputWithReadInfo(App.Lang("NetFrpWindow.Tab3.Info1"), _remoteIP, null);
            });
        }
    }

    public async void Share()
    {
        var model = new FrpShareModel();
        _ = ushort.TryParse(_localIP, out var port);

        var info = await ServerMotd.GetServerInfo("localhost", port);
        var version = "";
        if (info?.Version?.Name is { } version1)
        {
            version = version1;
        }
        await model.Init(version);
        var res1 = await DialogHost.Show(model, "ShareCon");
        if (res1 is not true)
        {
            return;
        }

        if (model.Text?.Length > 80)
        {
            Model.Show(App.Lang("NetFrpWindow.Tab3.Error3"));
            return;
        }

        if (IsRuning == false)
        {
            Model.Show(App.Lang("NetFrpWindow.Tab3.Error2"));
            return;
        }

        var res = await Model.ShowAsync(App.Lang("NetFrpWindow.Tab3.Info3"));
        if (!res)
        {
            return;
        }

        var user = UserBinding.GetLastUser();
        if (user?.AuthType != AuthType.OAuth)
        {
            Model.ShowWithOk(App.Lang("NetFrpWindow.Tab4.Error1"), WindowClose);
            return;
        }
        Model.Progress(App.Lang("NetFrpWindow.Tab4.Info2"));
        res = await UserBinding.TestLogin(user);
        Model.ProgressClose();
        if (!res)
        {
            Model.ShowWithOk(App.Lang("NetFrpWindow.Tab4.Error2"), WindowClose);
            return;
        }

        Model.Progress(App.Lang("NetFrpWindow.Tab3.Info5"));
        res = await WebBinding.ShareIP(user.AccessToken, _remoteIP, model);
        Model.ProgressClose();
        if (!res)
        {
            Model.Show(App.Lang("NetFrpWindow.Tab3.Error1"));
        }
        else
        {
            Model.Notify(App.Lang("NetFrpWindow.Tab3.Info4"));
        }
    }

    public async void Stop()
    {
        if (_isStoping)
        {
            return;
        }
        _isStoping = true;
        IsOk = false;
        IsRuning = false;

        if (_process == null || _process.HasExited)
        {
            return;
        }

        _now.IsStart = false;

        Model.Progress(App.Lang("NetFrpWindow.Tab3.Info6"));
        await Task.Run(() =>
        {
            _process.Kill(true);
            _process.Close();
            _process.Dispose();
            _process = null;
            _isStoping = false;
        });
        Model.ProgressClose();
    }

    public void Log(string? data)
    {
        lock (Lock)
        {
            Temp = data + Environment.NewLine;
            Dispatcher.UIThread.Invoke(() =>
            {
                Text.Insert(Text.TextLength, Temp);
            });
            Temp = "";
        }
    }

    public async Task<bool> Closing()
    {
        if (_process != null)
        {
            var res = await Model.ShowAsync(App.Lang("NetFrpWindow.Tab3.Info2"));
            if (res)
            {
                Stop();
                return false;
            }

            return true;
        }

        return false;
    }

    public void SetTab3Click()
    {
        if (IsOk && IsRuning)
        {
            Model.SetChoiseCall(_name, Share, Stop);
            Model.SetChoiseContent(_name, App.Lang("NetFrpWindow.Tab3.Text3"), App.Lang("NetFrpWindow.Tab3.Text2"));
        }
        else if (IsOk)
        {
            Model.SetChoiseCall(_name, Share);
            Model.SetChoiseContent(_name, App.Lang("NetFrpWindow.Tab3.Text3"));
        }
        else if (IsRuning)
        {
            Model.SetChoiseCall(_name, Stop);
            Model.SetChoiseContent(_name, App.Lang("NetFrpWindow.Tab3.Text2"));
        }
        else
        {
            RemoveClick();
        }
    }
}
