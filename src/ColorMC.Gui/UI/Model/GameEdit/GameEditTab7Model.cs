using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using AvaloniaEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.LaunchPath;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;
using System.Timers;
using Avalonia.Threading;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditTab7Model : GameEditTabModel
{
    [ObservableProperty]
    private TextDocument text;

    [ObservableProperty]
    private bool isGameRun;
    [ObservableProperty]
    private bool isWordWrap = true;
    [ObservableProperty]
    private bool isAuto = true;

    public string temp { get; private set; } = "";
    private Timer timer;

    public GameEditTab7Model(IUserControl con, GameSettingObj obj) : base(con, obj)
    {
        text = new();

        timer = new(TimeSpan.FromMilliseconds(100));
        timer.BeginInit();
        timer.Elapsed += Timer_Elapsed;
        timer.EndInit();
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
        var res = await GameBinding.Launch(Obj, false);
        if (!res.Item1)
        {
            var window = App.FindRoot(this);
            window.OkInfo.Show(res.Item2!);
        }
        IsGameRun = false;
    }

    public void GameStateChange()
    {
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
        lock (temp)
        {
            temp += data + Environment.NewLine;
        }
    }

    public void Clear()
    {
        Text.Text = "";
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        lock (temp)
        {
            if (!string.IsNullOrWhiteSpace(temp))
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    OnPropertyChanged("Insert");
                });
                temp = "";
            }
        }

        if (IsAuto)
        {
            OnPropertyChanged("Top");
        }
    }
}