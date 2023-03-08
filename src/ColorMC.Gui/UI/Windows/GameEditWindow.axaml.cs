using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.GameEdit;

namespace ColorMC.Gui.UI.Windows;

public partial class GameEditWindow : SelfBaseWindow
{
    public GameEditWindow()
    {
        var con = new GameEditControl();
        Main = con;
        MainControl.Children.Add(con);
    }
}
