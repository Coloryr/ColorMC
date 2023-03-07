using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Controls.Download;
using ColorMC.Gui.UI.Controls.Hello;
using ColorMC.Gui.UIBinding;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Timers;

namespace ColorMC.Gui.UI.Windows;

public partial class DownloadWindow : SelfBaseWindow
{
    public DownloadWindow()
    {
        Main = new DownloadControl();
        MainControl.Children.Add(Main.Con);
        OnClosed = Closed;
        SetTitle("DownloadWindow.Title");
    }

    private new void Closed()
    {

        App.DownloadWindow = null;
    }

    public void Load()
    {
        (Main as DownloadControl)?.Load();
    }
}
