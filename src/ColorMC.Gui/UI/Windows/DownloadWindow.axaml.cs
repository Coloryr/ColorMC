using ColorMC.Gui.UI.Controls.Download;

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
