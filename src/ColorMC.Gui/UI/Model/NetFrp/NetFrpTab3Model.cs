using Avalonia.Threading;
using AvaloniaEdit.Document;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

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

        App.FrpProcess = process;
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
        _process.Start();
        _process.BeginErrorReadLine();
        _process.BeginOutputReadLine();

        Text = new();

        IsRuning = true;
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
        Log(e.Data);
        if (e.Data?.Contains("TCP 类型隧道启动成功") == true
            || e.Data?.Contains("Your TCP proxy is available now") == true
            || e.Data?.Contains("来连接服务, 或使用IP地址(不推荐)") == true)
        {
            _isOut.Add(_localIP);
            Dispatcher.UIThread.Post(() =>
            {
                IsOk = true;
                Model.ShowReadInfo(App.Lang("NetFrpWindow.Tab3.Info1"), _remoteIP, null);
            });
        }
    }

    public async void Share()
    {
        var res = await Model.ShowWait(App.Lang("NetFrpWindow.Tab3.Info3"));
        if (!res)
        {
            return;
        }
        var user = UserBinding.GetLastUser();
        if (user == null || user.AuthType != AuthType.OAuth)
        {
            Model.Show(App.Lang("NetFrpWindow.Tab4.Error1"));
            return;
        }
        Model.Progress(App.Lang("NetFrpWindow.Tab3.Info5"));
        res = await WebBinding.ShareIP(user.AccessToken, _remoteIP);
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

    public void Stop()
    {
        IsOk = false;
        IsRuning = false;

        if (_process == null || _process.HasExited)
        {
            return;
        }

        _process.Kill(true);
        _process.Close();
        _process.Dispose();
        App.FrpProcess = null;
        _process = null;
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
            var res = await Model.ShowWait(App.Lang(App.Lang("NetFrpWindow.Tab3.Info2")));
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
    }
}
