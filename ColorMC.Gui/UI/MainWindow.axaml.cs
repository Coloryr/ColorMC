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

namespace ColorMC.Gui.UI;

public partial class MainWindow : Window
{
    public static MainWindow Window;
    public MainWindow()
    {
        Window = this;

        InitializeComponent();

        this.MakeItNoChrome();
        FontFamily = Program.FontFamily;
        
    }
}
