using Avalonia;
using Avalonia.Controls;
using Avalonia.X11;
using ColorMC.Core;
using ColorMC.Core.Utils;
using ColorMC.Gui;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace ColorMC.UI;

public partial class MainWindow : Window
{
    public static MainWindow Window;
    public MainWindow()
    {
        Window = this;

        InitializeComponent();
        FontFamily = Program.FontFamily;
        Opened += MainWindow_Opened;
        this.MakeItNoChrome();
    }

    private void MainWindow_Opened(object? sender, System.EventArgs e)
    {
        //CoreMain.Init(AppContext.BaseDirectory);
        //App.ShowNew();

        
    }
}
