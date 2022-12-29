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

    public DownloadWindow()
    {
        InitializeComponent();

        this.MakeItNoChrome();
        FontFamily = Program.FontFamily;

        CoreMain.DownloadItemStateUpdate = DownloadItemStateUpdate;

        ListView.Items = List;

        Closing += DownloadWindow_Closing;
        Closed += DownloadWindow_Closed;
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

            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            List1[item.Name].State = item.State.GetName();

            if (item.State == DownloadItemState.Done
                && List1.TryGetValue(item.Name, out var item1))
            {
                List.Remove(item1);
            }
            else if(item.State == DownloadItemState.GetInfo)
            {
                List1[item.Name].AllSize = $"{(double)item.AllSize / 1000 / 1000:.##}";
            }
            else if (item.State == DownloadItemState.Download)
            {
                List1[item.Name].NowSize = $"{(double)item.NowSize / item.AllSize * 100:.##} %";
            }
            else if (item.State == DownloadItemState.Error)
            {
                List1[item.Name].ErrorTime = item.ErrorTime;
            }
        });
    }
}
