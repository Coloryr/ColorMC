using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace ColorMC.Gui.UI.Model.Download;

public partial class DownloadModel : BaseModel
{
    public ObservableCollection<DownloadDisplayModel> ItemList { get; init; } = new();

    public Dictionary<string, DownloadDisplayModel> List1 = new();

    private long _count;
    private readonly Timer _timer;

    [ObservableProperty]
    private string _speed;
    [ObservableProperty]
    private string _now;
    [ObservableProperty]
    private string _button1 = App.GetLanguage("DownloadWindow.Text1");
    [ObservableProperty]
    private double _value = 0;
    [ObservableProperty]
    private bool _isPause;

    public DownloadModel(IUserControl con) : base(con)
    {
        ColorMCCore.DownloadItemStateUpdate = DownloadItemStateUpdate;

        _timer = new(TimeSpan.FromSeconds(1))
        {
            AutoReset = true
        };
        _timer.Elapsed += Timer_Elapsed;
    }

    partial void OnIsPauseChanged(bool value)
    {
        if (!value)
        {
            BaseBinding.DownloadResume();
            Button1 = App.GetLanguage("DownloadWindow.Text1");
            Notify(App.GetLanguage("DownloadWindow.Info3"));
        }
        else
        {
            BaseBinding.DownloadPause();
            Button1 = App.GetLanguage("DownloadWindow.Info5");
            Notify(App.GetLanguage("DownloadWindow.Info2"));
        }
    }

    [RelayCommand]
    public void Pause()
    {
        IsPause = !IsPause;
    }

    [RelayCommand]
    public async Task Stop()
    {
        var res = await ShowWait(App.GetLanguage("DownloadWindow.Info1"));
        if (res)
        {
            ItemList.Clear();
            List1.Clear();
            BaseBinding.DownloadStop();
        }
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        long now = _count;
        _count = 0;
        Speed = UIUtils.MakeSpeedSize(now);
    }

    public void DownloadItemStateUpdate(int index, DownloadItemObj item)
    {
        if (item.State == DownloadItemState.Init)
        {
            var item11 = new DownloadDisplayModel()
            {
                Name = item.Name,
                State = item.State.GetName(),
            };
            Dispatcher.UIThread.Post(() => ItemList.Add(item11));
            List1.Add(item.Name, item11);
            _timer.Start();

            return;
        }

        if (!List1.TryGetValue(item.Name, out DownloadDisplayModel? value))
        {
            return;
        }
        value.State = item.State.GetName();

        if (item.State == DownloadItemState.Done
            && List1.TryGetValue(item.Name, out var item1))
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
            value.NowSize = $"{(double)item.NowSize / item.AllSize * 100:0.##} %";
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

    public override void Close()
    {
        _timer.Dispose();
    }
}
