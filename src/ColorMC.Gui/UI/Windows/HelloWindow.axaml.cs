using Avalonia.Controls;
using ColorMC.Core;
using ColorMC.Gui.UI.Controls.Hello;
using System;
using System.Threading;

namespace ColorMC.Gui.UI.Windows;

public partial class HelloWindow : SelfBaseWindow
{
    public HelloWindow()
    {
        Main = new HelloControl();
        MainControl.Children.Add(Main.Con);
        SetTitle("MainWindow.Title");
    }
}
