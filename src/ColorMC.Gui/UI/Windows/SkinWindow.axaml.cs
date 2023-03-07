using ColorMC.Gui.UI.Controls.Skin;

namespace ColorMC.Gui.UI.Windows;

public partial class SkinWindow : SelfBaseWindow
{
    public SkinWindow()
    {
        Main = new SkinControl();
        MainControl.Children.Add(Main.Con);
        OnClosed = Closed;
        SetTitle("SkinWindow.Title");
        Main.Update();
    }

    private new void Closed()
    {
        App.SkinWindow = null;
    }
}


