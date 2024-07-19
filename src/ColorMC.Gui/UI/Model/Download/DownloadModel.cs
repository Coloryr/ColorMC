using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Threading;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using Timer = System.Timers.Timer;

namespace ColorMC.Gui.UI.Model.Download;

public partial class DownloadModel : TopModel
{
    public ObservableCollection<DownloadItemModel> DisplayList { get; init; } = [];

    private readonly Dictionary<int, DownloadItemModel> _downloadList = [];

    private long _count;
    private readonly Timer _timer;

    [ObservableProperty]
    private string _speed;
    [ObservableProperty]
    private string _now;
    [ObservableProperty]
    private double _value;
    [ObservableProperty]
    private int _size;
    [ObservableProperty]
    private bool _isPause;

    private readonly string _useName;

    public DownloadModel(BaseModel model) : base(model)
    {
        _useName = ToString() ?? "DownloadModel";

        _timer = new(TimeSpan.FromSeconds(1))
        {
            AutoReset = true
        };
        _timer.Elapsed += Timer_Elapsed;

        Model.SetChoiseContent(_useName, App.Lang("DownloadWindow.Text2"),
            App.Lang("DownloadWindow.Text1"));
        Model.SetChoiseCall(_useName, () => _ = Stop(), Pause);
        Model.HeadBackEnable = false;
    }

    partial void OnIsPauseChanged(bool value)
    {
        if (!value)
        {
            BaseBinding.DownloadResume();
            Model.SetChoiseContent(_useName,
                App.Lang("DownloadWindow.Text2"), App.Lang("DownloadWindow.Text1"));
            Model.Notify(App.Lang("DownloadWindow.Info3"));
        }
        else
        {
            BaseBinding.DownloadPause();
            Model.SetChoiseContent(_useName,
                App.Lang("DownloadWindow.Text2"), App.Lang("DownloadWindow.Text4"));
            Model.Notify(App.Lang("DownloadWindow.Info2"));
        }
    }

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

        var res = await Model.ShowWait(App.Lang("DownloadWindow.Info1"));
        if (res)
        {
            DisplayList.Clear();
            _downloadList.Clear();
            BaseBinding.DownloadStop();

            return true;
        }
        else
        {
            Model.HeadChoise1Display = true;
            Model.HeadChoiseDisplay = true;

            return false;
        }
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        long now = _count;
        _count = 0;
        Speed = UIUtils.MakeSpeedSize(now);
    }

    public void DownloadItemUpdate(int thread, DownloadItemObj item)
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
            _count += item.NowSize - temp;
        }
        else if (item.State == DownloadItemState.Error)
        {
            value.ErrorTime = item.ErrorTime;
        }
    }

    public override void Close()
    {
        _timer.Dispose();
        Model.HeadBackEnable = true;
        Model.RemoveChoiseData(_useName);
        DisplayList.Clear();
        _downloadList.Clear();
    }

    public void DownloadUpdate(int thread, DownloadState state, int count)
    {
        if (state == DownloadState.Start)
        {
            if (_downloadList.Count == 0 || _downloadList.Count != thread)
            {
                for (int a = 0; a < thread; a++)
                {
                    var item11 = new DownloadItemModel(a + 1);
                    Dispatcher.UIThread.Post(() => DisplayList.Add(item11));
                    _downloadList.Add(a, item11);
                }
            }
            if (!_timer.Enabled)
            {
                _timer.Start();
            }
            Size = count;
        }
        else if (state == DownloadState.End)
        {
            Dispatcher.UIThread.Post(WindowClose);
        }
    }

    private void DownloadTaskUpdate(int all, int now)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Value = (double)now / all * 100;
            Now = $"{now}/{all}";
        });
    }

    public DownloadArg Start()
    {
        return new()
        {
            Update = DownloadUpdate,
            UpdateTask = DownloadTaskUpdate,
            UpdateItem = DownloadItemUpdate
        };
    }
}
