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

namespace ColorMC.Gui.UI;

public partial class DownloadWindow : Window
{
    private ObservableCollection<DownlaodItemControl> List = new();
    private Dictionary<DownloadItem, DownlaodItemControl> List1 = new();

    public DownloadWindow()
    {
        InitializeComponent();

        this.MakeItNoChrome();
        FontFamily = Program.FontFamily;

        CoreMain.DownloadItemStateUpdate = DownloadItemStateUpdate;

        ListView.Items = List;
    }

    public void DownloadItemStateUpdate(int index, DownloadItem item)
    {
        if (item.State == DownloadItemState.Init)
        {
            List.Add(new()
            {
                PName = item.Name,
                PState = item.State.GetName()
            });
            return;
        }
        List1[item].PState = item.State.GetName();

        if (item.State == DownloadItemState.Done
            && List1.Remove(item, out var item1))
        {
            List.Remove(item1);
        }
        else if (item.State == DownloadItemState.Download)
        {
            List1[item].PAll = item.AllSize;
            List1[item].PNow = item.NowSize;
        }
        else if (item.State == DownloadItemState.Error)
        {
            List1[item].PError = item.ErrorTime;
        }
    }
}
