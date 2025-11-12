using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Threading;
using ColorMC.Core.Downloader;
using ColorMC.Core.GuiHandel;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using Timer = System.Timers.Timer;

namespace ColorMC.Gui.UI.Model.Download;

/// <summary>
/// 下载窗口
/// </summary>
public partial class DownloadModel : TopModel, IDownloadGuiHandel
{
    /// <summary>
    /// 显示项目
    /// </summary>
    public ObservableCollection<DownloadItemModel> DisplayList { get; init; } = [];

    /// <summary>
    /// 下载列表
    /// </summary>
    private readonly Dictionary<int, DownloadItemModel> _downloadList = [];

    /// <summary>
    /// 下载数量统计
    /// </summary>
    private long _count;
    /// <summary>
    /// 更新速度定时器
    /// </summary>
    private readonly Timer _timer;

    /// <summary>
    /// 当前下载速度
    /// </summary>
    [ObservableProperty]
    private string _speed;
    /// <summary>
    /// 下载完成数量
    /// </summary>
    [ObservableProperty]
    private string _now;
    /// <summary>
    /// 下载进度
    /// </summary>
    [ObservableProperty]
    private double _value;
    /// <summary>
    /// 总计下载任务数量
    /// </summary>
    [ObservableProperty]
    private int _size;
    /// <summary>
    /// 是否暂停下载
    /// </summary>
    [ObservableProperty]
    private bool _isPause;

    /// <summary>
    /// 已下载数量
    /// </summary>
    private int _doneNum;
    /// <summary>
    /// 任务数量
    /// </summary>
    private int _taskNum;

    private readonly string _useName;

    /// <summary>
    /// 是否开始运行
    /// </summary>
    private bool _needRun;

    public DownloadModel(int thread, BaseModel model) : base(model)
    {
        _useName = ToString() ?? "DownloadModel";

        _timer = new(TimeSpan.FromSeconds(1))
        {
            AutoReset = true
        };
        _timer.Elapsed += Timer_Elapsed;

        for (int a = 0; a < thread; a++)
        {
            var item11 = new DownloadItemModel(a + 1);
            Dispatcher.UIThread.Post(() => DisplayList.Add(item11));
            _downloadList.Add(a, item11);
        }

        Model.SetChoiseContent(_useName, LanguageUtils.Get("DownloadWindow.Text2"),
            LanguageUtils.Get("DownloadWindow.Text1"));
        Model.SetChoiseCall(_useName, () => _ = Stop(), Pause);
        Model.HeadBackEnable = false;
    }

    /// <summary>
    /// 暂停
    /// </summary>
    /// <param name="value"></param>
    partial void OnIsPauseChanged(bool value)
    {
        if (!value)
        {
            DownloadManager.Resume();
            Model.SetChoiseContent(_useName,
                LanguageUtils.Get("DownloadWindow.Text2"), LanguageUtils.Get("DownloadWindow.Text1"));
            Model.Notify(LanguageUtils.Get("DownloadWindow.Text11"));
        }
        else
        {
            DownloadManager.Pause();
            Model.SetChoiseContent(_useName,
                LanguageUtils.Get("DownloadWindow.Text2"), LanguageUtils.Get("DownloadWindow.Text4"));
            Model.Notify(LanguageUtils.Get("DownloadWindow.Text10"));
        }
    }

    /// <summary>
    /// 暂停下载
    /// </summary>
    private void Pause()
    {
        IsPause = !IsPause;
    }

    /// <summary>
    /// 请求停止下载
    /// </summary>
    /// <returns>是否已经停止</returns>
    public async Task<bool> Stop()
    {
        if (!BaseBinding.IsDownload)
        {
            return true;
        }

        Model.HeadChoise1Display = false;
        Model.HeadChoiseDisplay = false;

        var res = await Model.ShowAsync(LanguageUtils.Get("DownloadWindow.Text9"));
        if (res)
        {
            DisplayList.Clear();
            _downloadList.Clear();
            DownloadManager.Stop();
            Model.Notify(LanguageUtils.Get("DownloadWindow.Text13"));
            return true;
        }
        else
        {
            Model.HeadChoise1Display = true;
            Model.HeadChoiseDisplay = true;

            return false;
        }
    }

    /// <summary>
    /// 更新速度计数器
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        long now = _count;
        _count = 0;
        Speed = UIUtils.MakeSpeedSize(now);
    }

    /// <summary>
    /// 下载项目更新
    /// </summary>
    /// <param name="thread">下载线程</param>
    /// <param name="item">项目</param>
    public void UpdateItem(int thread, FileItemObj item)
    {
        if (!_downloadList.TryGetValue(thread, out DownloadItemModel? value))
        {
            return;
        }

        value.State = item.State.GetName();
        value.Name = item.Name;

        if (item.State == DownloadItemState.Done)
        {
            value.Clear();
        }
        else if (item.State == DownloadItemState.GetInfo)
        {
            value.AllSize = item.AllSize;
        }
        else if (item.State == DownloadItemState.Download)
        {
            long temp = value.NowSize;
            value.NowSize = item.NowSize;
            if (temp > item.NowSize)
            {
                _count = 0;
            }
            else
            {
                _count += item.NowSize - temp;
            }
        }
        else if (item.State == DownloadItemState.Error)
        {
            value.ErrorTime = item.ErrorTime;
        }
    }

    public override void Close()
    {
        _needRun = false;
        _timer.Dispose();
        Model.HeadBackEnable = true;
        Model.RemoveChoiseData(_useName);
        DisplayList.Clear();
        _downloadList.Clear();
    }

    /// <summary>
    /// 下载更新
    /// </summary>
    /// <param name="thread">线程</param>
    /// <param name="state">状态</param>
    /// <param name="count">累计下载量</param>
    public void Update(int thread, bool state, int count)
    {
        if (state == true)
        {
            if (!_timer.Enabled)
            {
                _timer.Start();
            }
            Size = count;
        }
        else if (state == false)
        {
            Dispatcher.UIThread.Invoke(WindowClose);
        }
    }

    /// <summary>
    /// 下载任务更新
    /// </summary>
    /// <param name="all">总计任务</param>
    /// <param name="now">当前任务</param>
    public void UpdateTask(UpdateType type, int num)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (type == UpdateType.AddItems)
            {
                _taskNum += num;
            }
            else
            {
                _doneNum++;
            }
        });
    }

    /// <summary>
    /// 开始下载
    /// </summary>
    /// <returns>Gui</returns>
    public IDownloadGuiHandel Start()
    {
        _needRun = true;
        DispatcherTimer.Run(Run, TimeSpan.FromMilliseconds(100));

        return this;
    }

    /// <summary>
    /// 开始更新界面
    /// </summary>
    /// <returns>是否需要继续更新</returns>
    private bool Run()
    {
        Value = (double)_doneNum / _taskNum * 100;
        Now = $"{_doneNum}/{_taskNum}";

        foreach (var item in DisplayList)
        {
            item.Update();
        }

        return _needRun;
    }
}
