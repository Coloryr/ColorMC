using ColorMC.Gui.UI.Controls.Error;
using System;

namespace ColorMC.Gui.UI.Windows;

public partial class ErrorWindow : SelfBaseWindow
{
    public ErrorWindow()
    {
        Main = new ErrorControl();
        MainControl.Children.Add(Main.Con);
        SetTitle("ErrorWindow.Title");
    }
}
