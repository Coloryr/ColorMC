using Avalonia.Threading;
using AvaloniaEdit.Document;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Model.Items;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.NetFrp;

public partial class NetFrpModel : MenuModel
{
    private readonly List<string> _isOut = new();

    [ObservableProperty]
    private bool _isRuning;

    [ObservableProperty]
    private TextDocument _text = new();

    public string? Temp { get; private set; } = "";

    private Process? _process;
    private readonly object Lock = new();

    private string _remoteIP;
    private string _localIP;

    public void SetProcess(Process process, NetFrpLocalModel model, string ip)
    {
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

        _process.Exited += _process_Exited;
        _process.OutputDataReceived += _process_OutputDataReceived;
        _process.ErrorDataReceived += _process_ErrorDataReceived;
        _process.Start();
        _process.BeginErrorReadLine();
        _process.BeginOutputReadLine();

        Text = new();

        IsRuning = true;
    }

    private void _process_Exited(object? sender, EventArgs e)
    {
        IsRuning = false;
    }

    private void _process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        Log(e.Data);
    }

    private void _process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        Log(e.Data);
        if (e.Data?.Contains("TCP 类型隧道启动成功") == true
            || e.Data?.Contains("Your TCP proxy is available now") == true)
        {
            _isOut.Add(_localIP);
            Dispatcher.UIThread.Post(() =>
            {
                Model.ShowReadInfo(App.Lang("NetFrpWindow.Tab3.Info1"), _remoteIP, null);
            });
        }
    }

    [RelayCommand]
    public void Stop()
    {
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
                OnPropertyChanged("Insert");
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
}
