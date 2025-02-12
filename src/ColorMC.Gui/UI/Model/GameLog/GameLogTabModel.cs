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

/// <summary>
/// 游戏实例日志窗口
/// </summary>
public partial class GameLogModel : GameModel
{
    public const string NameEnd = "End";
    public const string NameInsert = "Insert";
    public const string NameTop = "Top";
    public const string NameSearch = "Search";
    
    /// <summary>
    /// 文件列表
    /// </summary>
    public ObservableCollection<string> FileList { get; init; } = [];

    /// <summary>
    /// 线程列表
    /// </summary>
    public ObservableCollection<string> Threads { get; init; } = [""];
    /// <summary>
    /// 分类列表
    /// </summary>
    public ObservableCollection<string> Categorys { get; init; } = [""];

    /// <summary>
    /// 文本
    /// </summary>
    [ObservableProperty]
    private TextDocument _text;

    /// <summary>
    /// 游戏是否在运行
    /// </summary>
    [ObservableProperty]
    private bool _isGameRun;
    /// <summary>
    /// 是否自动换行
    /// </summary>
    [ObservableProperty]
    private bool _isWordWrap;
    /// <summary>
    /// 是否自动下拉
    /// </summary>
    [ObservableProperty]
    private bool _isAuto;
    /// <summary>
    /// 选择的文件名
    /// </summary>
    [ObservableProperty]
    private string? _file;
    /// <summary>
    /// 选择的线程
    /// </summary>
    [ObservableProperty]
    private string? _selectThread;
    /// <summary>
    /// 选择的分类
    /// </summary>
    [ObservableProperty]
    private string? _selectCategory;
    /// <summary>
    /// 是否启用其他日志
    /// </summary>
    [ObservableProperty]
    private bool _enableNone;
    /// <summary>
    /// 是否启用信息日志
    /// </summary>
    [ObservableProperty]
    private bool _enableInfo;
    /// <summary>
    /// 是否启用警告日志
    /// </summary>
    [ObservableProperty]
    private bool _enableWarn;
    [ObservableProperty]
    private bool _enableError;
    [ObservableProperty]
    private bool _enableDebug;
    [ObservableProperty]
    private bool _isFile;

    public string Temp { get; private set; } = "";

    private readonly ConcurrentQueue<string> _queue = new();

    private readonly GameGuiSettingObj _setting;

    private List<GameLogItemObj> _logs;
    private readonly Thread _timer;
    private bool _run;
    private bool _isKill;
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

        _setting = GameGuiSetting.ReadConfig(obj);
        _enableDebug = _setting.Log.EnableDebug;
        _enableError = _setting.Log.EnableError;
        _enableInfo = _setting.Log.EnableInfo;
        _enableNone = _setting.Log.EnableNone;
        _enableWarn = _setting.Log.EnableWarn;
        _isAuto = _setting.Log.Auto;
        _isWordWrap = _setting.Log.WordWrap;
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
        _setting.Log.Auto = value;
        GameGuiSetting.WriteConfig(Obj, _setting);
    }

    partial void OnIsWordWrapChanged(bool value)
    {
        _setting.Log.WordWrap = value;
        GameGuiSetting.WriteConfig(Obj, _setting);
    }

    partial void OnEnableNoneChanged(bool value)
    {
        LoadLast();

        _setting.Log.EnableNone = value;
        GameGuiSetting.WriteConfig(Obj, _setting);
    }

    partial void OnEnableInfoChanged(bool value)
    {
        LoadLast();

        _setting.Log.EnableInfo = value;
        GameGuiSetting.WriteConfig(Obj, _setting);
    }

    partial void OnEnableWarnChanged(bool value)
    {
        LoadLast();

        _setting.Log.EnableWarn = value;
        GameGuiSetting.WriteConfig(Obj, _setting);
    }

    partial void OnEnableErrorChanged(bool value)
    {
        LoadLast();

        _setting.Log.EnableError = value;
        GameGuiSetting.WriteConfig(Obj, _setting);
    }

    partial void OnEnableDebugChanged(bool value)
    {
        LoadLast();

        _setting.Log.EnableDebug = value;
        GameGuiSetting.WriteConfig(Obj, _setting);
    }

    [RelayCommand]
    public async Task Push()
    {
        if (string.IsNullOrWhiteSpace(Text.Text))
        {
            Model.Show(App.Lang("GameLogWindow.Error2"));
            return;
        }
        var res = await Model.ShowAsync(App.Lang("GameLogWindow.Info4"));
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
            Model.InputWithChoise(string.Format(App.Lang("GameLogWindow.Info5"), url), App.Lang("GameLogWindow.Info8"), async () =>
            {
                await BaseBinding.CopyTextClipboard(top, url);
                Model.Notify(App.Lang("GameLogWindow.Info7"));
            });
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
        _isKill = true;
        GameManager.KillGame(Obj);
        GameBinding.CancelLaunch();
        IsGameRun = false;
    }

    [RelayCommand]
    public async Task Launch()
    {
        if (IsGameRun)
        {
            return;
        }

        _isKill = false;
        IsGameRun = true;

        var res = await GameBinding.Launch(Model, Obj, hide: GuiConfigUtils.Config.CloseBeforeLaunch);
        if (!res.Res && !string.IsNullOrWhiteSpace(res.Message))
        {
            Model.Show(res.Message!);
            return;
        }
        Load();
        File = null;
        IsAuto = true;
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
            Model.SubTitle = App.Lang("GameLogWindow.Info3");
        }
        else
        {
            Model.SubTitle = "";
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
            Dispatcher.UIThread.Invoke(() =>
            {
                OnPropertyChanged(NameEnd);
            });
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

        if (GameManager.GetGameLog(Obj, BuildLevel()) is { } text)
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

    public void GameExit(int code)
    {
        if (code == 0 || _isKill)
        {
            return;
        }
        Dispatcher.UIThread.Post(() =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                Model.ShowWithChoise(string.Format(App.Lang("GameLogWindow.Info9"), code), App.Lang("GameLogWindow.Text8"), async () =>
                {
                    Model.ShowClose();
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
                        Model.InputWithChoise(string.Format(App.Lang("GameLogWindow.Info5"), url), App.Lang("GameLogWindow.Info8"), async () =>
                        {
                            await BaseBinding.CopyTextClipboard(top, url);
                            Model.Notify(App.Lang("GameLogWindow.Info7"));
                        });
                        await BaseBinding.CopyTextClipboard(top, url);
                        Model.Notify(App.Lang("GameLogWindow.Info7"));
                    }
                });
            });
        });
        Load1();
        var item = GameBinding.GetLastCrash(Obj);
        File = item;
        if (File != null)
        {
            IsAuto = false;
            OnPropertyChanged(NameTop);
        }
    }
}
