using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Helpers;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Timers;
using Timer = System.Timers.Timer;

namespace ColorMC.Gui.UI.Model.Download;

public partial class DownloadModel : ObservableObject
{
    private IUserControl Con;
    public ObservableCollection<DownloadDisplayModel> ItemList { get; init; } = new();

    public Dictionary<string, DownloadDisplayModel> List1 = new();

    [ObservableProperty]
    private string speed;
    [ObservableProperty]
    private string now;
    [ObservableProperty]
    private string button = "P";
    [ObservableProperty]
    private string button1 = "P";
    [ObservableProperty]
    private double value = 0;
    [ObservableProperty]
    private bool displayP;
    [ObservableProperty]
    private bool displayS;

    public bool pause = false;
    private long Count;
    private Timer Timer;

    public DownloadModel(IUserControl con)
    {
        Con = con;

        ColorMCCore.DownloadItemStateUpdate = DownloadItemStateUpdate;

        Timer = new(TimeSpan.FromSeconds(1))
        {
            AutoReset = true
        };
        Timer.Elapsed += Timer_Elapsed;
    }

    [RelayCommand]
    public void Pause()
    {
        var windows = Con.Window;
        if (!pause)
        {
            BaseBinding.DownloadPause();
            pause = true;
            Button = "R";
            Button1 = App.GetLanguage("DownloadWindow.Info5");
            windows.NotifyInfo.Show(App.GetLanguage("DownloadWindow.Info2"));
        }
        else
        {
            DownloadManager.DownloadResume();
            Button = "P";
            Button1 = App.GetLanguage("DownloadWindow.Text1");
            pause = false;
            windows.NotifyInfo.Show(App.GetLanguage("DownloadWindow.Info3"));
        }
    }

    [RelayCommand]
    public async void Stop()
    {
        var windows = Con.Window;
        var res = await windows.OkInfo.ShowWait(App.GetLanguage("DownloadWindow.Info1"));
        if (res)
        {
            ItemList.Clear();
            List1.Clear();
            BaseBinding.DownloadStop();
        }
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        long now = Count;
        Count = 0;
        Speed = UIUtils.MakeSpeedSize(now);
    }

    public void Close()
    {
        Timer.Dispose();
    }

    public void DownloadItemStateUpdate(int index, DownloadItemObj item)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (item.State == DownloadItemState.Init)
            {
                var item11 = new DownloadDisplayModel()
                {
                    Name = item.Name,
                    State = item.State.GetName(),
                };
                ItemList.Add(item11);
                List1.Add(item.Name, item11);
                Timer.Start();

                return;
            }

            if (!List1.ContainsKey(item.Name))
                return;

            List1[item.Name].State = item.State.GetName();

            if (item.State == DownloadItemState.Done
                && List1.TryGetValue(item.Name, out var item1))
            {
                var data = BaseBinding.GetDownloadSize();
                Load();
                ItemList.Remove(item1);
            }
            else if (item.State == DownloadItemState.GetInfo)
            {
                List1[item.Name].AllSize = $"{(double)item.AllSize / 1000 / 1000:0.##}";
            }
            else if (item.State == DownloadItemState.Download)
            {
                long temp = List1[item.Name].Last;
                List1[item.Name].NowSize = $"{(double)item.NowSize / item.AllSize * 100:0.##} %";
                List1[item.Name].Last = item.NowSize;
                Count += item.NowSize - temp;
            }
            else if (item.State == DownloadItemState.Error)
            {
                List1[item.Name].ErrorTime = item.ErrorTime;
            }
        });
    }

    public void Load()
    {
        var data = BaseBinding.GetDownloadSize();
        Value = (double)data.Item2 / data.Item1 * 100;
        Now = $"{data.Item2}/{data.Item1}";
    }
}
