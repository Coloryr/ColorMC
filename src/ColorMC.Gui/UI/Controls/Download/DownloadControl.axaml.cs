using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Helpers;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace ColorMC.Gui.UI.Controls.Download;

public partial class DownloadControl : UserControl, IUserControl
{
    private readonly ObservableCollection<DownloadDisplayModel> List = new();
    private readonly Dictionary<string, DownloadDisplayModel> List1 = new();

    private bool pause = false;
    private long Count;
    private Timer Timer;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public DownloadControl()
    {
        InitializeComponent();

        ColorMCCore.DownloadItemStateUpdate = DownloadItemStateUpdate;

        DataGrid_Download.Items = List;

        Button_P1.PointerExited += Button_P1_PointerLeave;
        Button_P.PointerEntered += Button_P_PointerEnter;

        Button_S1.PointerExited += Button_S1_PointerLeave;
        Button_S.PointerEntered += Button_S_PointerEnter;

        Button_P.Click += Button_P_Click;
        Button_P1.Click += Button_P_Click;
        Button_S.Click += Button_S_Click;
        Button_S1.Click += Button_S_Click;

        ProgressBar1.Value = 0;
        Timer = new(TimeSpan.FromSeconds(1))
        {
            AutoReset = true
        };
        Timer.Elapsed += Timer_Elapsed;
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        long now = Count;
        Count = 0;
        Dispatcher.UIThread.Post(() =>
        {
            Label3.Content = UIUtils.MakeFileSize(now);
        });
    }

    private async void Button_S_Click(object? sender, RoutedEventArgs e)
    {
        var windows = App.FindRoot(VisualRoot);
        var res = await windows.Info.ShowWait(App.GetLanguage("DownloadWindow.Info1"));
        if (res)
        {
            List.Clear();
            List1.Clear();
            BaseBinding.DownloadStop();
        }
    }

    private void Button_P_Click(object? sender, RoutedEventArgs e)
    {
        var windows = App.FindRoot(VisualRoot);
        if (!pause)
        {
            BaseBinding.DownloadPause();
            pause = true;
            Button_P.Content = "R";
            Button_P1.Content = App.GetLanguage("DownloadWindow.Info5");
            windows.Info2.Show(App.GetLanguage("DownloadWindow.Info2"));
        }
        else
        {
            DownloadManager.DownloadResume();
            Button_P.Content = "P";
            Button_P1.Content = App.GetLanguage("DownloadWindow.Text1");
            pause = false;
            windows.Info2.Show(App.GetLanguage("DownloadWindow.Info3"));
        }
    }

    private void Button_S1_PointerLeave(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(Button_S1, null, CancellationToken.None);
    }

    private void Button_S_PointerEnter(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(null, Button_S1, CancellationToken.None);
    }

    private void Button_P1_PointerLeave(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(Button_P1, null, CancellationToken.None);
    }

    private void Button_P_PointerEnter(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(null, Button_P1, CancellationToken.None);
    }

    public void Opened()
    {
        Window.SetTitle(App.GetLanguage("DownloadWindow.Title"));

        DataGrid_Download.MakeTran();
    }

    public void Closed()
    {
        Timer.Dispose();

        ColorMCCore.DownloadItemStateUpdate = null;

        App.DownloadWindow = null;
    }

    public async Task<bool> Closing()
    {
        var windows = App.FindRoot(VisualRoot);
        if (BaseBinding.IsDownload)
        {
            var res = await windows.Info.ShowWait(App.GetLanguage("DownloadWindow.Info4"));
            if (res)
            {
                BaseBinding.DownloadStop();
                return false;
            }
            return true;
        }

        return false;
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
                List.Add(item11);
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
                List.Remove(item1);
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
        ProgressBar1.Maximum = 100;
        ProgressBar1.Value = (double)data.Item2 / data.Item1 * 100;
        Label1.Content = $"{data.Item2}/{data.Item1}";
    }
}
