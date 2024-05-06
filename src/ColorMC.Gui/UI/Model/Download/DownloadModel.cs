using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Threading;
using ColorMC.Core.Downloader;
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
    public ObservableCollection<DownloadItemModel> ItemList { get; init; } = [];

    private readonly Dictionary<string, DownloadItemModel> _downloadList = [];

    private long _count;
    private readonly Timer _timer;
    private readonly ICollection<DownloadItemObj> _list;

    [ObservableProperty]
    private string _speed;
    [ObservableProperty]
    private string _now;
    [ObservableProperty]
    private double _value = 0;
    [ObservableProperty]
    private bool _isPause;

    private readonly string _useName;

    public DownloadModel(BaseModel model, ICollection<DownloadItemObj> list) : base(model)
    {
        _useName = ToString() ?? "DownloadModel";

        _timer = new(TimeSpan.FromSeconds(1))
        {
            AutoReset = true
        };
        _timer.Elapsed += Timer_Elapsed;
        _list = list;

        Model.SetChoiseContent(_useName,
            App.Lang("DownloadWindow.Text1"), App.Lang("DownloadWindow.Text2"));
        Model.SetChoiseCall(_useName, Pause, () => _ = Stop());
        Model.HeadBackEnable = false;
    }

    partial void OnIsPauseChanged(bool value)
    {
        if (!value)
        {
            BaseBinding.DownloadResume();
            Model.SetChoiseContent(_useName,
                App.Lang("DownloadWindow.Text1"), App.Lang("DownloadWindow.Text2"));
            Model.Notify(App.Lang("DownloadWindow.Info3"));
        }
        else
        {
            BaseBinding.DownloadPause();
            Model.SetChoiseContent(_useName,
                App.Lang("DownloadWindow.Text4"), App.Lang("DownloadWindow.Text2"));
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
            ItemList.Clear();
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

    public void DownloadItemUpdate(DownloadItemObj item)
    {
        if (item.State == DownloadItemState.Init)
        {
            var item11 = new DownloadItemModel()
            {
                Name = item.Name,
                State = item.State.GetName(),
            };
            Dispatcher.UIThread.Post(() => ItemList.Add(item11));
            _downloadList.Add(item.Name, item11);
            _timer.Start();

            return;
        }

        if (!_downloadList.TryGetValue(item.Name, out DownloadItemModel? value))
        {
            return;
        }
        value.State = item.State.GetName();

        if (item.State == DownloadItemState.Done
            && _downloadList.TryGetValue(item.Name, out var item1))
        {
            var data = BaseBinding.GetDownloadSize();
            Load();
            Dispatcher.UIThread.Post(() => ItemList.Remove(item1));
        }
        else if (item.State == DownloadItemState.GetInfo)
        {
            value.AllSize = $"{(double)item.AllSize / 1000 / 1000:0.##}";
        }
        else if (item.State == DownloadItemState.Download)
        {
            long temp = value.Last;
            if (item.AllSize != 0)
            {
                value.NowSize = $"{(double)item.NowSize / item.AllSize * 100:0.##} %";
            }
            else
            {
                value.NowSize = $"{item.NowSize / 1000 / 1000}:0.## MB";
            }
            value.Last = item.NowSize;
            _count += item.NowSize - temp;
        }
        else if (item.State == DownloadItemState.Error)
        {
            value.ErrorTime = item.ErrorTime;
        }
    }

    public void Load()
    {
        var data = BaseBinding.GetDownloadSize();
        Value = (double)data.Item2 / data.Item1 * 100;
        Now = $"{data.Item2}/{data.Item1}";
    }

    protected override void Close()
    {
        _timer.Dispose();
        Model.HeadBackEnable = true;
        Model.RemoveChoiseData(_useName);
        ItemList.Clear();
        _downloadList.Clear();
    }

    public void DownloaderUpdate(DownloadState state)
    {
        if (state == DownloadState.Start)
        {
            Load();
        }
        else if (state == DownloadState.End)
        {
            OnPropertyChanged("WindowClose");
        }
    }

    public Task<bool> Start()
    {
        return DownloadManager.StartAsync(_list, DownloaderUpdate, DownloadItemUpdate);
    }
}
