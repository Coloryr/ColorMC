using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UIBinding;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Timers;

namespace ColorMC.Gui.UI.Windows;

public partial class DownloadWindow : Window
{
    private readonly ObservableCollection<DownloadDisplayObj> List = new();
    private readonly Dictionary<string, DownloadDisplayObj> List1 = new();

    private bool pause = false;
    private long Count;
    private Timer Timer;

    public DownloadWindow()
    {
        InitializeComponent();

        this.Init();
        Icon = App.Icon;
        Border1.MakeResizeDrag(this);

        CoreMain.DownloadItemStateUpdate = DownloadItemStateUpdate;

        DataGrid_Download.Items = List;

        Expander_P.ContentTransition = App.CrossFade100;
        Expander_S.ContentTransition = App.CrossFade100;

        Button_P1.PointerExited += Button_P1_PointerLeave;
        Button_P.PointerEntered += Button_P_PointerEnter;

        Button_S1.PointerExited += Button_S1_PointerLeave;
        Button_S.PointerEntered += Button_S_PointerEnter;

        Button_P1.Click += Button_P_Click;
        Button_S1.Click += Button_S_Click;

        Closing += DownloadWindow_Closing;
        Closed += DownloadWindow_Closed;
        Opened += DownloadWindow_Opened;

        ProgressBar1.Value = 0;
        Timer = new(TimeSpan.FromSeconds(1))
        {
            AutoReset = true
        };
        Timer.Elapsed += Timer_Elapsed;
        App.PicUpdate += Update;

        Update();
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
        var res = await Info.ShowWait(App.GetLanguage("DownloadWindow.Info1"));
        if (res)
        {
            List.Clear();
            List1.Clear();
            DownloadManager.DownloadStop();
        }
    }

    private void Button_P_Click(object? sender, RoutedEventArgs e)
    {
        if (!pause)
        {
            DownloadManager.DownloadPause();
            pause = true;
            Button_P.Content = "R";
            Button_P1.Content = App.GetLanguage("DownloadWindow.Info5");
            Info2.Show(App.GetLanguage("DownloadWindow.Info2"));
        }
        else
        {
            DownloadManager.DownloadResume();
            Button_P.Content = "P";
            Button_P1.Content = App.GetLanguage("DownloadWindow.Text1");
            pause = false;
            Info2.Show(App.GetLanguage("DownloadWindow.Info3"));
        }
    }

    private void Button_S1_PointerLeave(object? sender, PointerEventArgs e)
    {
        Expander_S.IsExpanded = false;
    }

    private void Button_S_PointerEnter(object? sender, PointerEventArgs e)
    {
        Expander_S.IsExpanded = true;
    }

    private void Button_P1_PointerLeave(object? sender, PointerEventArgs e)
    {
        Expander_P.IsExpanded = false;
    }

    private void Button_P_PointerEnter(object? sender, PointerEventArgs e)
    {
        Expander_P.IsExpanded = true;
    }

    private void DownloadWindow_Opened(object? sender, EventArgs e)
    {
        DataGrid_Download.MakeTran();
        Expander_P.MakePadingNull();
        Expander_S.MakePadingNull();
    }

    private void DownloadWindow_Closed(object? sender, EventArgs e)
    {
        Timer.Dispose();

        App.PicUpdate -= Update;

        CoreMain.DownloadItemStateUpdate = null;
        App.DownloadWindow = null;
    }

    private async void DownloadWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (BaseBinding.GetDownloadState() != CoreRunState.End)
        {
            var res = await Info.ShowWait(App.GetLanguage("DownloadWindow.Info4"));
            if (res)
            {
                DownloadManager.DownloadStop();
            }
            e.Cancel = true;
            return;
        }
    }

    public void DownloadItemStateUpdate(int index, DownloadItem item)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (item.State == DownloadItemState.Init)
            {
                var item11 = new DownloadDisplayObj()
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
        Label2.Content = data.Item1;
        Label1.Content = data.Item2;
    }

    public void Update()
    {
        App.Update(this, Image_Back, Border1, Border2);
    }
}
