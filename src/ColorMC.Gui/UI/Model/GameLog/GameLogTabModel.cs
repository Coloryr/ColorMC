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
using ColorMC.Core.Game;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs.Config;
using ColorMC.Gui.UI.Model.Dialog;
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
    /// <summary>
    /// 是否启用错误日志
    /// </summary>
    [ObservableProperty]
    private bool _enableError;
    /// <summary>
    /// 是否启启用调试日志
    /// </summary>
    [ObservableProperty]
    private bool _enableDebug;

    /// <summary>
    /// 日志
    /// </summary>
    public string Temp { get; private set; } = "";

    /// <summary>
    /// 日志队列
    /// </summary>
    private readonly ConcurrentQueue<string> _queue = new();

    /// <summary>
    /// 实例设置
    /// </summary>
    private readonly GameGuiSettingObj _setting;

    /// <summary>
    /// 日志列表
    /// </summary>
    private List<GameLogItemObj>? _logs;
    /// <summary>
    /// 更新定时器
    /// </summary>
    private readonly Thread _timer;
    /// <summary>
    /// 是否在运行中
    /// </summary>
    private bool _run;
    /// <summary>
    /// 是否强制结束
    /// </summary>
    private bool _isKill;
    /// <summary>
    /// 是否在加载中
    /// </summary>
    private bool _load;

    private GameRuntimeLog? _nowLog;

    public GameLogModel(WindowModel model, GameSettingObj obj) : base(model, obj)
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

        //读取设置
        _setting = GameManager.ReadConfig(obj);
        _enableDebug = _setting.Log.EnableDebug;
        _enableError = _setting.Log.EnableError;
        _enableInfo = _setting.Log.EnableInfo;
        _enableNone = _setting.Log.EnableNone;
        _enableWarn = _setting.Log.EnableWarn;
        _isAuto = _setting.Log.Auto;
        _isWordWrap = _setting.Log.WordWrap;
    }

    /// <summary>
    /// 切换文件
    /// </summary>
    /// <param name="value"></param>
    partial void OnFileChanged(string? value)
    {
        LoadLast();
    }

    partial void OnSelectThreadChanged(string? value)
    {
        LoadLogWithSelect();
    }

    partial void OnSelectCategoryChanged(string? value)
    {
        LoadLogWithSelect();
    }

    partial void OnIsAutoChanged(bool value)
    {
        _setting.Log.Auto = value;
        GameManager.WriteConfig(Obj, _setting);
    }

    partial void OnIsWordWrapChanged(bool value)
    {
        _setting.Log.WordWrap = value;
        GameManager.WriteConfig(Obj, _setting);
    }

    partial void OnEnableNoneChanged(bool value)
    {
        LoadLast();

        _setting.Log.EnableNone = value;
        GameManager.WriteConfig(Obj, _setting);
    }

    partial void OnEnableInfoChanged(bool value)
    {
        LoadLast();

        _setting.Log.EnableInfo = value;
        GameManager.WriteConfig(Obj, _setting);
    }

    partial void OnEnableWarnChanged(bool value)
    {
        LoadLast();

        _setting.Log.EnableWarn = value;
        GameManager.WriteConfig(Obj, _setting);
    }

    partial void OnEnableErrorChanged(bool value)
    {
        LoadLast();

        _setting.Log.EnableError = value;
        GameManager.WriteConfig(Obj, _setting);
    }

    partial void OnEnableDebugChanged(bool value)
    {
        LoadLast();

        _setting.Log.EnableDebug = value;
        GameManager.WriteConfig(Obj, _setting);
    }

    /// <summary>
    /// 上传日志
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task Push()
    {
        if (string.IsNullOrWhiteSpace(Text.Text))
        {
            Window.Show(LangUtils.Get("GameLogWindow.Text26"));
            return;
        }
        var res = await Window.ShowChoice(LangUtils.Get("GameLogWindow.Text19"));
        if (!res)
        {
            return;
        }

        var dialog = Window.ShowProgress(LangUtils.Get("GameLogWindow.Text21"));
        var url = await McloAPI.PushAsync(Text.Text);
        Window.CloseDialog(dialog);
        if (url == null)
        {
            Window.Show(LangUtils.Get("GameLogWindow.Text25"));
            return;
        }
        else
        {
            var top = Window.GetTopLevel();
            if (top == null)
            {
                return;
            }
            var dialog1 = new InputModel(Window.WindowId)
            {
                Text1 = string.Format(LangUtils.Get("GameLogWindow.Text20"), url),
                ChoiseText = LangUtils.Get("GameLogWindow.Text23"),
                TextReadonly = true,
                ChoiseCall = () =>
                {
                    BaseBinding.CopyTextClipboard(top, url);
                    Window.Notify(LangUtils.Get("GameLogWindow.Text22"));
                }
            };
            Window.ShowDialog(dialog1);
            BaseBinding.CopyTextClipboard(top, url);
            Window.Notify(LangUtils.Get("GameLogWindow.Text22"));
        }
    }

    /// <summary>
    /// 加载日志文件列表
    /// </summary>
    [RelayCommand]
    public void LoadFileList()
    {
        FileList.Clear();
        FileList.Add("");
        FileList.AddRange(GameBinding.GetLogList(Obj));
    }

    /// <summary>
    /// 强制停止游戏
    /// </summary>
    [RelayCommand]
    public void Stop()
    {
        _isKill = true;
        GameManager.KillGame(Obj);
        GameManager.CancelLaunch(Obj);
        IsGameRun = false;
    }

    /// <summary>
    /// 启动游戏实例
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task Launch()
    {
        if (IsGameRun)
        {
            return;
        }

        _isKill = false;
        IsGameRun = true;

        var res = await GameBinding.LaunchAsync(Obj, Window, null, hide: GuiConfigUtils.Config.CloseBeforeLaunch);
        if (!res.Res && !string.IsNullOrWhiteSpace(res.Message))
        {
            IsGameRun = false;
            Window.Show(res.Message!);
            return;
        }
        Load();
        File = null;
        IsAuto = true;
    }

    /// <summary>
    /// 打开搜索
    /// </summary>
    [RelayCommand]
    public void Search()
    {
        OnPropertyChanged(NameSearch);
    }

    /// <summary>
    /// 加载游戏实例信息
    /// </summary>
    public void Load()
    {
        IsGameRun = GameManager.IsGameRun(Obj);

        if (IsGameRun)
        {
            Window.SubTitle = LangUtils.Get("GameLogWindow.Text18");
        }
        else
        {
            Window.SubTitle = "";
        }

        LoadLast();
    }

    /// <summary>
    /// 设置不自动下拉
    /// </summary>
    public void SetNotAuto()
    {
        IsAuto = false;
    }

    /// <summary>
    /// 添加日志
    /// </summary>
    /// <param name="data"></param>
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

    /// <summary>
    /// 清理日志
    /// </summary>
    public void Clear()
    {
        if (string.IsNullOrWhiteSpace(File))
        {
            Text = new();
        }
    }

    /// <summary>
    /// 日志处理
    /// </summary>
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

    /// <summary>
    /// 加载当前实例日志
    /// </summary>
    private async void LoadLast()
    {
        _load = true;

        Threads.Clear();
        Threads.Add("");
        Categorys.Clear();
        Categorys.Add("");

        _logs = null;

        //切换日志
        if (string.IsNullOrWhiteSpace(File))
        {
            if (GameManager.GetGameLog(Obj, BuildLevel()) is { } text)
            {
                _logs = text;
            }
        }
        else
        {
            if (_nowLog == null || _nowLog.File != File)
            {
                _nowLog = null;
                var dialog = Window.ShowProgress(LangUtils.Get("Text.Loading"));
                _nowLog = await GameBinding.ReadLogAsync(Obj, File);
                Window.CloseDialog(dialog);
                if (_nowLog == null)
                {
                    Window.Show(LangUtils.Get("GameLogWindow.Text17"));
                    return;
                }
            }

            if (_nowLog.GetLog(BuildLevel()) is { } text)
            {
                _logs = text;
            }
        }

        //显示日志
        if (_logs != null)
        {
            var builder = new StringBuilder();
            foreach (var item in _logs)
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

        OnPropertyChanged(NameTop);

        _load = false;
    }

    /// <summary>
    /// 筛选日志
    /// </summary>
    private void LoadLogWithSelect()
    {
        if (_load || _logs == null)
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

    /// <summary>
    /// 获取等级
    /// </summary>
    /// <returns></returns>
    private LogLevel BuildLevel()
    {
        var level = LogLevel.Base;
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

    /// <summary>
    /// 游戏退出
    /// </summary>
    /// <param name="code">退出码</param>
    public void GameExit(int code)
    {
        if (code == 0 || _isKill)
        {
            return;
        }
        DispatcherTimer.RunOnce(() =>
        {
            //弹出日志上传选项
            var dialog = new ChoiceModel(Window.WindowId)
            {
                Text = string.Format(LangUtils.Get("GameLogWindow.Text24"), code),
                ChoiceText = LangUtils.Get("GameLogWindow.Text8")
            };

            dialog.ChoiceCall = async () =>
            {
                Window.CloseDialog(dialog);
                var dialog1 = Window.ShowProgress(LangUtils.Get("GameLogWindow.Text21"));
                var url = await McloAPI.PushAsync(Text.Text);
                Window.CloseDialog(dialog1);
                if (url == null)
                {
                    Window.Show(LangUtils.Get("GameLogWindow.Text25"));
                    return;
                }
                else
                {
                    var top = Window.GetTopLevel();
                    if (top == null)
                    {
                        return;
                    }
                    var dialog2 = new InputModel(Window.WindowId)
                    {
                        Text1 = string.Format(LangUtils.Get("GameLogWindow.Text20"), url),
                        ChoiseText = LangUtils.Get("GameLogWindow.Text23"),
                        TextReadonly = true,
                        ChoiseCall = () =>
                        {
                            BaseBinding.CopyTextClipboard(top, url);
                            Window.Notify(LangUtils.Get("GameLogWindow.Text22"));
                        }
                    };
                    Window.ShowDialog(dialog2);
                    BaseBinding.CopyTextClipboard(top, url);
                    Window.Notify(LangUtils.Get("GameLogWindow.Text22"));
                }
            };

            Window.ShowDialog(dialog);
        }, TimeSpan.FromMilliseconds(200));
        LoadFileList();
        var item = Obj.GetLastCrash();
        File = item;
        if (File != null)
        {
            IsAuto = false;
            OnPropertyChanged(NameTop);
        }
    }
}
