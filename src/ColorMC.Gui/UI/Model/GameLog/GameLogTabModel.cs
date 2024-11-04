using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.GameLog;

public partial class GameLogModel : GameModel
{
    public const string NameEnd = "End";
    public const string NameInsert = "Insert";
    public const string NameTop = "Top";
    public const string NameSearch = "Search";

    public ObservableCollection<string> FileList { get; init; } = [];

    public ObservableCollection<string> Threads { get; init; } = [""];
    public ObservableCollection<string> Categorys { get; init; } = [""];

    [ObservableProperty]
    private TextDocument _text;

    [ObservableProperty]
    private bool _isGameRun;
    [ObservableProperty]
    private bool _isWordWrap;
    [ObservableProperty]
    private bool _isAuto;
    [ObservableProperty]
    private string? _file;

    [ObservableProperty]
    private string? _selectThread;
    [ObservableProperty]
    private string? _selectCategory;

    [ObservableProperty]
    private bool _enableNone;
    [ObservableProperty]
    private bool _enableInfo;
    [ObservableProperty]
    private bool _enableWarn;
    [ObservableProperty]
    private bool _enableError;
    [ObservableProperty]
    private bool _enableDebug;
    [ObservableProperty]
    private bool _isFile;

    public string Temp { get; private set; } = "";

    private ConcurrentQueue<string> _queue = new();

    private GameLogSettingObj _setting;

    private List<GameLogItemObj> _logs;
    private readonly Thread _timer;
    private bool _run;
    private bool _load;

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

        _setting = GameLogSetting.ReadConfig(obj);
        _enableDebug = _setting.EnableDebug;
        _enableError = _setting.EnableError;
        _enableInfo = _setting.EnableInfo;
        _enableNone = _setting.EnableNone;
        _enableWarn = _setting.EnableWarn;
        _isAuto = _setting.Auto;
        _isWordWrap = _setting.WordWrap;
    }

    async partial void OnFileChanged(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            IsFile = false;
            LoadLast();
            OnPropertyChanged(NameTop);
            return;
        }

        IsFile = true;
        Model.Progress(App.Lang("GameLogWindow.Info1"));
        var data = await GameBinding.ReadLog(Obj, value);
        Model.ProgressClose();
        if (data == null)
        {
            Model.Show(App.Lang("GameLogWindow.Info2"));
            return;
        }

        Text = new(data);
        OnPropertyChanged(NameTop);
    }

    partial void OnSelectThreadChanged(string? value)
    {
        LoadLast1();
    }

    partial void OnSelectCategoryChanged(string? value)
    {
        LoadLast1();
    }

    partial void OnIsAutoChanged(bool value)
    {
        _setting.Auto = value;
        GameLogSetting.WriteConfig(Obj, _setting);
    }

    partial void OnIsWordWrapChanged(bool value)
    {
        _setting.WordWrap = value;
        GameLogSetting.WriteConfig(Obj, _setting);
    }

    partial void OnEnableNoneChanged(bool value)
    {
        LoadLast();

        _setting.EnableNone = value;
        GameLogSetting.WriteConfig(Obj, _setting);
    }

    partial void OnEnableInfoChanged(bool value)
    {
        LoadLast();

        _setting.EnableInfo = value;
        GameLogSetting.WriteConfig(Obj, _setting);
    }

    partial void OnEnableWarnChanged(bool value)
    {
        LoadLast();

        _setting.EnableWarn = value;
        GameLogSetting.WriteConfig(Obj, _setting);
    }

    partial void OnEnableErrorChanged(bool value)
    {
        LoadLast();

        _setting.EnableError = value;
        GameLogSetting.WriteConfig(Obj, _setting);
    }

    partial void OnEnableDebugChanged(bool value)
    {
        LoadLast();

        _setting.EnableDebug = value;
        GameLogSetting.WriteConfig(Obj, _setting);
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
        var url = await WebBinding.PushMclo(Text.Text);
        Model.ProgressClose();
        if (url == null)
        {
            Model.Show(App.Lang("GameLogWindow.Error1"));
            return;
        }
        else
        {
            var top = Model.GetTopLevel();
            if (top == null)
            {
                return;
            }
            Model.ShowReadInfoOne(string.Format(App.Lang("GameLogWindow.Info5"), url), null);
            await BaseBinding.CopyTextClipboard(top, url);
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
        GameManager.StopGame(Obj);
        GameBinding.CancelLaunch();
        IsGameRun = false;
    }

    [RelayCommand]
    public async Task Launch()
    {
        if (IsGameRun)
            return;

        IsGameRun = true;

        var res = await GameBinding.Launch(Model, Obj, hide: GuiConfigUtils.Config.CloseBeforeLaunch);
        if (!res.Res)
        {
            Model.Show(res.Message!);
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
        OnPropertyChanged(NameSearch);
    }

    public void Load()
    {
        IsGameRun = GameManager.IsGameRun(Obj);

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

    public void Log(GameLogItemObj data)
    {
        if (!EnableNone && data.Level == LogLevel.None)
        {
            return;
        }
        if (!EnableInfo && data.Level == LogLevel.Info)
        {
            return;
        }
        if (!EnableWarn && data.Level == LogLevel.Warn)
        {
            return;
        }
        if (!EnableError && data.Level == LogLevel.Error)
        {
            return;
        }
        if (!EnableDebug && data.Level == LogLevel.Debug)
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(File))
        {
            _queue.Enqueue(data.Log + Environment.NewLine);

            if (!string.IsNullOrWhiteSpace(data.Category) && !Categorys.Contains(data.Category))
            {
                Categorys.Add(data.Category);
            }
            if (!string.IsNullOrWhiteSpace(data.Thread) && !Threads.Contains(data.Thread))
            {
                Threads.Add(data.Thread);
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
                OnPropertyChanged(NameInsert);
            });
            Temp = "";
        }

        if (IsAuto)
        {
            OnPropertyChanged(NameEnd);
        }
    }

    public override void Close()
    {
        FileList.Clear();
        _run = false;
    }

    private void LoadLast()
    {
        if (IsFile)
        {
            return;
        }
        _load = true;

        Threads.Clear();
        Threads.Add("");
        Categorys.Clear();
        Categorys.Add("");

        if (GameManager.GetGameLog(Obj.UUID, BuildLevel()) is { } text)
        {
            _logs = text;
            var builder = new StringBuilder();
            foreach (var item in text)
            {
                if (!string.IsNullOrWhiteSpace(item.Category) && !Categorys.Contains(item.Category))
                {
                    Categorys.Add(item.Category);
                }
                if (!string.IsNullOrWhiteSpace(item.Thread) && !Threads.Contains(item.Thread))
                {
                    Threads.Add(item.Thread);
                }

                builder.AppendLine(item.Log);
            }
            Text = new(builder.ToString());
        }
        else
        {
            Text = new();
        }

        _load = false;
    }

    private void LoadLast1()
    {
        if (_load || IsFile)
        {
            return;
        }
        var builder = new StringBuilder();
        bool cap = string.IsNullOrWhiteSpace(SelectCategory);
        bool thr = string.IsNullOrWhiteSpace(SelectThread);
        foreach (var item in _logs)
        {
            if (!cap && item.Category != SelectCategory)
            {
                continue;
            }
            if (!thr && item.Thread != SelectThread)
            {
                continue;
            }

            builder.AppendLine(item.Log);
        }
        Text = new(builder.ToString());
    }

    private LogLevel BuildLevel()
    {
        LogLevel level = LogLevel.Base;
        if (EnableNone)
        {
            level |= LogLevel.None;
        }
        if (EnableInfo)
        {
            level |= LogLevel.Info;
        }
        if (EnableWarn)
        {
            level |= LogLevel.Warn;
        }
        if (EnableError)
        {
            level |= LogLevel.Error;
        }
        if (EnableDebug)
        {
            level |= LogLevel.Debug;
        }
        return level;
    }
}
