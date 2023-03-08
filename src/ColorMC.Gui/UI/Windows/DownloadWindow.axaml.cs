using ColorMC.Gui.UI.Controls.Download;

namespace ColorMC.Gui.UI.Windows;

public class DownloadWindow : SelfBaseWindow
{
    public DownloadWindow()
    {
        var con = new DownloadControl();
        Main = con;
        MainControl.Children.Add(con);
    }
}
