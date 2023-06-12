using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using ColorMC.Gui.UI.Windows;
using ColorMC.Core.Objs;
using System.Timers;
using ColorMC.Gui.UIBinding;
using System.Collections.ObjectModel;
using AvaloniaEdit.Utils;

namespace ColorMC.Gui.UI.Model.GameLog;

public partial class GameLogTabModel : ObservableObject
{
    private readonly IUserControl Con;
    public GameSettingObj Obj { get; init; }

    public ObservableCollection<string> FileList { get; init; } = new();

    [ObservableProperty]
    private TextDocument text;

    [ObservableProperty]
    private bool isGameRun;
    [ObservableProperty]
    private bool isWordWrap = true;
    [ObservableProperty]
    private bool isAuto = true;
    [ObservableProperty]
    private string? file;

    public string Temp { get; private set; } = "";
    private readonly Timer timer;

    public GameLogTabModel(IUserControl con, GameSettingObj obj)
    {
        Con = con;
        Obj = obj;

        text = new();

        timer = new(TimeSpan.FromMilliseconds(100));
        timer.BeginInit();
        timer.Elapsed += Timer_Elapsed;
        timer.EndInit();

        if (!obj.Empty)
        {
            isGameRun = BaseBinding.IsGameRun(Obj);

            Load();
            Load1();

            File = "";
        }
    }

    async partial void OnFileChanged(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (BaseBinding.GameLogs.ContainsKey(Obj.UUID))
            {
                Text = new(BaseBinding.GameLogs[Obj.UUID].ToString());
            }
            else
            {
                Text = new();
            }
            OnPropertyChanged("Top");
            return;
        }

        var window = Con.Window;
        window.ProgressInfo.Show("正在读取中");
        var data = await GameBinding.ReadLog(Obj, value);
        window.ProgressInfo.Close();
        if (data == null)
        {
            window.OkInfo.Show("读取失败");
            return;
        }

        Text = new(data);
        OnPropertyChanged("Top");
    }

    [RelayCommand]
    public void Load1()
    {
        FileList.Clear();
        FileList.Add("");
        FileList.AddRange(GameBinding.GetLogList(Obj));
    }

    [RelayCommand]
    public void Stop()
    {
        BaseBinding.StopGame(Obj);
        IsGameRun = false;
    }

    [RelayCommand]
    public async void Launch()
    {
        IsGameRun = true;
        var res = await GameBinding.Launch(Obj);
        if (!res.Item1)
        {
            var window = Con.Window;
            window.OkInfo.Show(res.Item2!);
        }
        Load();
    }

    public void Load()
    {
        IsGameRun = BaseBinding.IsGameRun(Obj);
    }

    public void SetNotAuto()
    {
        IsAuto = false;
    }

    public void Log(string data)
    {
        if (!timer.Enabled)
        {
            timer.Start();
        }
        if (string.IsNullOrWhiteSpace(File))
        {
            lock (Temp)
            {
                Temp += data + Environment.NewLine;
            }
        }
    }

    public void Clear()
    {
        if (!string.IsNullOrWhiteSpace(Temp))
        {
            Text.Text = "";
        }
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        lock (Temp)
        {
            if (!string.IsNullOrWhiteSpace(Temp))
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    OnPropertyChanged("Insert");
                });
                Temp = "";
            }
        }

        if (IsAuto)
        {
            OnPropertyChanged("End");
        }
    }
}
