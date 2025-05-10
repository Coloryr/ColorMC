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
    public const string NameCon = "ShareCon";

    /// <summary>
    /// 已经映射的地址
    /// </summary>
    private readonly List<string> _isOut = [];

    /// <summary>
    /// 是否运行中
    /// </summary>
    [ObservableProperty]
    private bool _isRuning;
    /// <summary>
    /// 是否成功启动
    /// </summary>
    [ObservableProperty]
    private bool _isOk;
    /// <summary>
    /// 进程日志
    /// </summary>
    [ObservableProperty]
    private TextDocument _text = new();

    /// <summary>
    /// 日志缓存
    /// </summary>
    public string? Temp { get; private set; } = "";

    /// <summary>
    /// 映射进程
    /// </summary>
    private Process? _process;
    /// <summary>
    /// 同步锁
    /// </summary>
    private readonly object Lock = new();

    /// <summary>
    /// 远程地址
    /// </summary>
    private string _remoteIP;
    /// <summary>
    /// 本地地址
    /// </summary>
    private string _localIP;
    /// <summary>
    /// 是否已经通知成功
    /// </summary>
    private bool _isSend;
    /// <summary>
    /// 是否停止中
    /// </summary>
    private bool _isStoping;

    /// <summary>
    /// 开始的本地映射
    /// </summary>
    private NetFrpLocalModel _now;

    /// <summary>
    /// 是否成功映射
    /// </summary>
    /// <param name="value"></param>
    partial void OnIsOkChanged(bool value)
    {
        RemoveClick();
        SetTab3Click();
    }
    /// <summary>
    /// 是否运行映射
    /// </summary>
    /// <param name="value"></param>
    partial void OnIsRuningChanged(bool value)
    {
        RemoveClick();
        SetTab3Click();
    }

    /// <summary>
    /// 设置映射进程
    /// </summary>
    /// <param name="process">进程</param>
    /// <param name="model">本地地址</param>
    /// <param name="ip">远程地址</param>
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

    /// <summary>
    /// 进程退出
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Process_Exited(object? sender, EventArgs e)
    {
        IsRuning = false;

        _now.IsStart = false;
    }

    /// <summary>
    /// 进程日志
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        Log(e.Data);
    }

    /// <summary>
    /// 进程日志
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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
                Model.InputWithReadInfo(App.Lang("NetFrpWindow.Tab3.Info1"), _remoteIP, false, true, false, null);
            });
        }
    }

    /// <summary>
    /// 开始共享映射
    /// </summary>
    private async void Share()
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
        var res1 = await DialogHost.Show(model, NameCon);
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

    /// <summary>
    /// 停止映射进程
    /// </summary>
    private async void Stop()
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

    /// <summary>
    /// 映射日志
    /// </summary>
    /// <param name="data"></param>
    private void Log(string? data)
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

    /// <summary>
    /// 设置标题按钮
    /// </summary>
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
