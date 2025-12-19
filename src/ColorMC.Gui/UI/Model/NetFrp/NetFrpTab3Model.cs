using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using ColorMC.Core.Net.Motd;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Net.Apis;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
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
            Window.Show(LangUtils.Get("NetFrpWindow.Tab3.Text15"));
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
                var dialog = new InputModel(Window.WindowId)
                {
                    Text1 = LangUtils.Get("NetFrpWindow.Tab3.Text8"),
                    Text2 = _remoteIP,
                    TextReadonly = true
                };
                Window.ShowDialog(dialog);
            });
        }
    }

    /// <summary>
    /// 开始共享映射
    /// </summary>
    private async void Share()
    {
        var model = new FrpShareModel(Window.WindowId);
        _ = ushort.TryParse(_localIP, out var port);

        var info = await ServerMotd.GetServerInfoAsync("localhost", port);
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
            Window.Show(LangUtils.Get("NetFrpWindow.Tab3.Text16"));
            return;
        }

        if (IsRuning == false)
        {
            Window.Show(LangUtils.Get("NetFrpWindow.Tab3.Text15"));
            return;
        }

        var res = await Window.ShowChoice(LangUtils.Get("NetFrpWindow.Tab3.Text10"));
        if (!res)
        {
            return;
        }

        var user = UserManager.GetLastUser();
        if (user?.AuthType != AuthType.OAuth)
        {
            await Window.ShowWait(LangUtils.Get("NetFrpWindow.Tab4.Text8"));
            WindowClose();
            return;
        }
        var dialog = Window.ShowProgress(LangUtils.Get("NetFrpWindow.Tab4.Text6"));
        var res2 = await UserBinding.TestLogin(user, CancellationToken.None);
        Window.CloseDialog(dialog);
        if (!res)
        {
            await Window.ShowWait(res2.Data!);
            WindowClose();
            return;
        }

        dialog = Window.ShowProgress(LangUtils.Get("NetFrpWindow.Tab3.Text12"));
        res = await ColorMCCloudAPI.PutCloudServerAsync(user.AccessToken, _remoteIP, model);
        Window.CloseDialog(dialog);
        if (!res)
        {
            Window.Show(LangUtils.Get("NetFrpWindow.Tab3.Text14"));
        }
        else
        {
            Window.Notify(LangUtils.Get("NetFrpWindow.Tab3.Text11"));
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

        var dialog = Window.ShowProgress(LangUtils.Get("NetFrpWindow.Tab3.Text13"));
        await Task.Run(() =>
        {
            _process.Kill(true);
            _process.Close();
            _process.Dispose();
            _process = null;
            _isStoping = false;
        });
        Window.CloseDialog(dialog);
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
            var res = await Window.ShowChoice(LangUtils.Get("NetFrpWindow.Tab3.Text9"));
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
            Window.SetChoiseCall(_name, Share, Stop);
            Window.SetChoiseContent(_name, LangUtils.Get("NetFrpWindow.Tab3.Text3"), LangUtils.Get("NetFrpWindow.Tab3.Text2"));
        }
        else if (IsOk)
        {
            Window.SetChoiseCall(_name, Share);
            Window.SetChoiseContent(_name, LangUtils.Get("NetFrpWindow.Tab3.Text3"));
        }
        else if (IsRuning)
        {
            Window.SetChoiseCall(_name, Stop);
            Window.SetChoiseContent(_name, LangUtils.Get("NetFrpWindow.Tab3.Text2"));
        }
        else
        {
            RemoveClick();
        }
    }
}
