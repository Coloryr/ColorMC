using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.GameLog;

public partial class GameLogModel : GameModel
{
    public ObservableCollection<string> FileList { get; init; } = [];

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

    public string Temp { get; private set; } = "";
    public ConcurrentQueue<string> _queue = new();

    private readonly Thread _timer;
    private bool _run;

    public GameLogModel(BaseModel model, GameSettingObj obj) : base(model, obj)
    {
        _text = new();

        _timer = new(() =>
        {
            while (_run)
            {
                Run();
                Thread.Sleep(100);
            }
        });
        _run = true;
        _timer.Start();
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

        Model.Progress(App.Lang("GameLogWindow.Info1"));
        var data = await GameBinding.ReadLog(Obj, value);
        Model.ProgressClose();
        if (data == null)
        {
            Model.Show(App.Lang("GameLogWindow.Info2"));
            return;
        }

        Text = new(data);
        OnPropertyChanged("Top");
    }

    [RelayCommand]
    public async Task Push()
    {
        if (string.IsNullOrWhiteSpace(Text.Text))
        {
            Model.Show(App.Lang("GameLogWindow.Error2"));
            return;
        }
        var res = await Model.ShowWait(App.Lang("GameLogWindow.Info4"));
        if (!res)
        {
            return;
        }

        Model.Progress(App.Lang("GameLogWindow.Info6"));
        var url = await WebBinding.Push(Text.Text);
        Model.ProgressClose();
        if (url == null)
        {
            Model.Show(App.Lang("GameLogWindow.Error1"));
            return;
        }
        else
        {
            Model.ShowReadInfoOne(string.Format(App.Lang("GameLogWindow.Info5"), url), null);

            await BaseBinding.CopyTextClipboard(url);
            Model.Notify(App.Lang("GameLogWindow.Info7"));
        }
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

        var res = await GameBinding.Launch(Model, Obj, hide: GuiConfigUtils.Config.CloseBeforeLaunch);
        if (!res.Item1)
        {
            Model.Show(res.Item2!);
        }
        Load();
        if (File == null)
        {
            IsAuto = true;
        }
    }

    [RelayCommand]
    public void Search()
    {
        OnPropertyChanged("Search");
    }

    public void Load()
    {
        IsGameRun = BaseBinding.IsGameRun(Obj);

        if (IsGameRun)
        {
            Model.Title1 = App.Lang("GameLogWindow.Info3");
        }
        else
        {
            Model.Title1 = "";
        }

        LoadLast();
    }

    public void SetNotAuto()
    {
        IsAuto = false;
    }

    public void Log(string data)
    {
        if (string.IsNullOrWhiteSpace(File))
        {
            _queue.Enqueue(data + Environment.NewLine);
        }
    }

    public void Clear()
    {
        if (string.IsNullOrWhiteSpace(File))
        {
            Text = new();
        }
    }

    private void Run()
    {
        var temp = new StringBuilder();
        while (!_queue.IsEmpty)
        {
            if (_queue.TryDequeue(out var temp1) && !string.IsNullOrWhiteSpace(temp1))
            {
                temp.Append(temp1);
            }
        }
        if (temp != null)
        {
            Temp = temp.ToString();
            Dispatcher.UIThread.Invoke(() =>
            {
                OnPropertyChanged("Insert");
            });
            Temp = "";
        }

        if (IsAuto)
        {
            OnPropertyChanged("End");
        }
    }

    protected override void Close()
    {
        FileList.Clear();
        _run = false;
    }

    public void LoadLast()
    {
        if (BaseBinding.GameLogs.TryGetValue(Obj.UUID, out var temp))
        {
            Text = new(temp.ToString());
        }
    }
}
