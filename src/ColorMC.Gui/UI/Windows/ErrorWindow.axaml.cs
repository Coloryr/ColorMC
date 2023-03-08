using ColorMC.Gui.UI.Controls.Error;
using System;

namespace ColorMC.Gui.UI.Windows;

public partial class ErrorWindow : SelfBaseWindow
{
    public ErrorWindow()
    {
        var con = new ErrorControl();
        Main = con;
        MainControl.Children.Add(con);
    }
}
