using Avalonia.Controls;
using ColorMC.Core;
using ColorMC.Core.Http.Downloader;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Views;
using ColorMC.Gui.UIBinding;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.ComponentModel;
using Avalonia.Threading;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Animation;

namespace ColorMC.Gui.UI;

public class DownloadObj : INotifyPropertyChanged
{
    private string name;
    private string allsize;
    private string nowsize;
    private string state;
    private int errortime;

    public string Name { get { return name; } set { name = value; NotifyPropertyChanged(); } }
    public string AllSize { get { return allsize; } set { allsize = value; NotifyPropertyChanged(); } }
    public string NowSize { get { return nowsize; } set { nowsize = value; NotifyPropertyChanged(); } }
    public string State { get { return state; } set { state = value; NotifyPropertyChanged(); } }
    public int ErrorTime { get { return errortime; } set { errortime = value; NotifyPropertyChanged(); } }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public partial class DownloadWindow : Window
{
    private ObservableCollection<DownloadObj> List = new();
    private Dictionary<string, DownloadObj> List1 = new();

    private bool pause;

    public DownloadWindow()
    {
        InitializeComponent();

        this.MakeItNoChrome();
        FontFamily = Program.FontFamily;

        CoreMain.DownloadItemStateUpdate = DownloadItemStateUpdate;

        DataGrid_Download.Items = List;

        if (App.BackBitmap != null)
        {
            Image_Back.Source = App.BackBitmap;
        }

        Expander_P.ContentTransition = new CrossFade(TimeSpan.FromMilliseconds(100));
        Expander_S.ContentTransition = new CrossFade(TimeSpan.FromMilliseconds(100));

        Button_P1.PointerLeave += Button_P1_PointerLeave;
        Button_P.PointerEnter += Button_P_PointerEnter;

        Button_S1.PointerLeave += Button_S1_PointerLeave;
        Button_S.PointerEnter += Button_S_PointerEnter;

        Button_P1.Click += Button_P_Click;
        Button_S1.Click += Button_S_Click;

        Closing += DownloadWindow_Closing;
        Closed += DownloadWindow_Closed;
        Opened += DownloadWindow_Opened;
    }

    private async void Button_S_Click(object? sender, RoutedEventArgs e)
    {
        var res = await Info.ShowWait("是否要停止下载");
        if (res)
        {
            DownloadManager.Stop();
        }
    }

    private void Button_P_Click(object? sender, RoutedEventArgs e)
    {
        if (pause)
        {
            DownloadManager.Pause();
            pause = true;
            Button_P.Content = "R";
            Button_P1.Content = "继续下载";
            Info2.Show("下载已暂停");
        }
        else
        {
            DownloadManager.Resume();
            Button_P.Content = "R";
            Button_P1.Content = "继续下载";
            pause = false;
            Info2.Show("下载已暂停");
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
        CoreMain.DownloadItemStateUpdate = null;
        App.DownloadWindow = null;
    }

    private void DownloadWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (List.Count != 0)
        {
            Info.Show("下载还未完成");
            e.Cancel = true;
        }
    }

    public void DownloadItemStateUpdate(int index, DownloadItem item)
    {
        if (item.State == DownloadItemState.Init)
        {
            var item1 = new DownloadObj()
            {
                Name = item.Name,
                State = item.State.GetName(),
            };
            List.Add(item1);
            List1.Add(item.Name, item1);

            var data = OtherBinding.GetDownloadState();

            ProgressBar1.Maximum = data.Item1;

            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            List1[item.Name].State = item.State.GetName();

            if (item.State == DownloadItemState.Done
                && List1.TryGetValue(item.Name, out var item1))
            {
                var data = OtherBinding.GetDownloadState();
                ProgressBar1.Value = data.Item2;
                Label1.Content = $"{(double)data.Item2 / data.Item1 * 100:0.##}";
                List.Remove(item1);
            }
            else if (item.State == DownloadItemState.GetInfo)
            {
                List1[item.Name].AllSize = $"{(double)item.AllSize / 1000 / 1000:0.##}";
            }
            else if (item.State == DownloadItemState.Download)
            {
                List1[item.Name].NowSize = $"{(double)item.NowSize / item.AllSize * 100:0.##} %";
            }
            else if (item.State == DownloadItemState.Error)
            {
                List1[item.Name].ErrorTime = item.ErrorTime;
            }
        });
    }
}
