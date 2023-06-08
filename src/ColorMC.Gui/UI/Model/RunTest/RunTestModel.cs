using Avalonia.Threading;
using AvaloniaEdit.Document;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ColorMC.Gui.UI.Model.RunTest;

public partial class RunTestModel : ObservableObject
{
    private readonly IUserControl Con;
    public GameSettingObj Obj { get; init; }

    [ObservableProperty]
    private TextDocument text;

    [ObservableProperty]
    private bool isGameRun;
    [ObservableProperty]
    private bool isWordWrap = true;
    [ObservableProperty]
    private bool isAuto = true;

    public string Temp { get; private set; } = "";
    private readonly Timer timer;

    public RunTestModel(IUserControl con, GameSettingObj obj)
    {
        Con = con;
        Obj = obj;

        text = new();

        timer = new(TimeSpan.FromMilliseconds(100));
        timer.BeginInit();
        timer.Elapsed += Timer_Elapsed;
        timer.EndInit();

        isGameRun = BaseBinding.IsGameRun(Obj);
        if (BaseBinding.GameLogs.ContainsKey(obj.UUID))
        {
            text.Text = BaseBinding.GameLogs[obj.UUID].ToString();
            Dispatcher.UIThread.Post(() =>
            {
                OnPropertyChanged("Top");
            });
        }

        Load();
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
        lock (Temp)
        {
            Temp += data + Environment.NewLine;
        }
    }

    public void Clear()
    {
        Text.Text = "";
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
            OnPropertyChanged("Top");
        }
    }
}
