using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.GameEdit;

namespace ColorMC.Gui.UI.Windows;

public partial class GameEditWindow : SelfBaseWindow
{
    public GameEditWindow()
    {
        Main = new GameEditControl();
        MainControl.Children.Add(Main.Con);
        SetTitle("GameEditWindow.Title");
    }

    public void Log(string data)
    {
        (Main as GameEditControl)?.Log(data);
    }

    public void SetGame(GameSettingObj obj)
    {
        (Main as GameEditControl)?.SetGame(obj);
    }

    public void SetType(GameEditWindowType type)
    {
        (Main as GameEditControl)?.SetType(type);
    }

    public void ClearLog()
    {
        (Main as GameEditControl)?.ClearLog();
    }
}
