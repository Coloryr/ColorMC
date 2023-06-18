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
using ColorMC.Core;

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
        window.ProgressInfo.Show(App.GetLanguage("GameLogWindow.Info1"));
        var data = await GameBinding.ReadLog(Obj, value);
        window.ProgressInfo.Close();
        if (data == null)
        {
            window.OkInfo.Show(App.GetLanguage("GameLogWindow.Info2"));
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
        if (IsGameRun)
            return;

        IsGameRun = true;

        ColorMCCore.GameLaunch = GameLunch;

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

    private void GameLunch(GameSettingObj obj, LaunchState state)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var window = Con.Window;
            switch (state)
            {
                case LaunchState.Login:
                    window.Head.Title1 = App.GetLanguage("MainWindow.Info8");
                    break;
                case LaunchState.Check:
                    window.Head.Title1 = App.GetLanguage("MainWindow.Info9");
                    break;
                case LaunchState.CheckVersion:
                    window.Head.Title1 = App.GetLanguage("MainWindow.Info10");
                    break;
                case LaunchState.CheckLib:
                    window.Head.Title1 = App.GetLanguage("MainWindow.Info11");
                    break;
                case LaunchState.CheckAssets:
                    window.Head.Title1 = App.GetLanguage("MainWindow.Info12");
                    break;
                case LaunchState.CheckLoader:
                    window.Head.Title1 = App.GetLanguage("MainWindow.Info13");
                    break;
                case LaunchState.CheckLoginCore:
                    window.Head.Title1 = App.GetLanguage("MainWindow.Info14");
                    break;
                case LaunchState.CheckMods:
                    window.Head.Title1 = App.GetLanguage("MainWindow.Info17");
                    break;
                case LaunchState.Download:
                    window.Head.Title1 = App.GetLanguage("MainWindow.Info15");
                    break;
                case LaunchState.JvmPrepare:
                    window.Head.Title1 = App.GetLanguage("MainWindow.Info16");
                    break;
                case LaunchState.End:
                    window.Head.Title1 = "";
                    break;
            }
        });
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
