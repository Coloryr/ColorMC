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

    public void Show(string data, Exception e, bool close)
        => (Main as ErrorControl)?.Show(data, e, close);

    public void Show(string data, string e, bool close)
        => (Main as ErrorControl)?.Show(data, e, close);
}
