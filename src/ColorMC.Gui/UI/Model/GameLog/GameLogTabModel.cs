using Avalonia.Threading;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Timers;

namespace ColorMC.Gui.UI.Model.GameLog;

public partial class GameLogTabModel : BaseModel
{
    public GameSettingObj Obj { get; init; }

    public ObservableCollection<string> FileList { get; init; } = new();

    [ObservableProperty]
    private TextDocument _text;

    [ObservableProperty]
    private bool _isGameRun;
    [ObservableProperty]
    private bool _isWordWrap = true;
    [ObservableProperty]
    private bool _isAuto = true;
    [ObservableProperty]
    private string? _file;

    public string Temp { get; private set; }
    private readonly Timer t_timer;

    public GameLogTabModel(IUserControl con, GameSettingObj obj) : base(con)
    {
        Obj = obj;

        _text = new();

        t_timer = new(TimeSpan.FromMilliseconds(100));
        t_timer.BeginInit();
        t_timer.Elapsed += Timer_Elapsed;
        t_timer.EndInit();

        if (!obj.Empty)
        {
            _isGameRun = BaseBinding.IsGameRun(Obj);

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

        Progress(App.GetLanguage("GameLogWindow.Info1"));
        var data = await GameBinding.ReadLog(Obj, value);
        ProgressClose();
        if (data == null)
        {
            Show(App.GetLanguage("GameLogWindow.Info2"));
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
    public async Task Launch()
    {
        if (IsGameRun)
            return;

        IsGameRun = true;

        var res = await GameBinding.Launch(Window, Obj);
        if (!res.Item1)
        {
            Show(res.Item2!);
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
        if (!t_timer.Enabled)
        {
            t_timer.Start();
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
        if (string.IsNullOrWhiteSpace(File))
        {
            Text = new();
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
