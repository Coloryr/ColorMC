using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

public class MainFlyout
{
    private readonly GameItemModel Obj;
    public MainFlyout(Control con, GameItemModel obj)
    {
        Obj = obj;

        var run = BaseBinding.IsGameRun(obj.Obj);

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("MainWindow.Flyouts.Text2"), true, Button3_Click),
            (App.GetLanguage("MainWindow.Flyouts.Text1"), true, Button1_Click),
            (App.GetLanguage("MainWindow.Flyouts.Text3"), true, Button11_Click),
            (App.GetLanguage("MainWindow.Flyouts.Text4"), true, Button2_Click),
            (App.GetLanguage("MainWindow.Flyouts.Text6"), true, Button4_Click),
            (App.GetLanguage("Button.OpFile"), true, Button7_Click),
            (App.GetLanguage("MainWindow.Flyouts.Text7"), true, Button5_Click),
            (App.GetLanguage("MainWindow.Flyouts.Text8"), true, Button9_Click),
            (App.GetLanguage("MainWindow.Flyouts.Text10"), !run, Button12_Click),
            (App.GetLanguage("MainWindow.Flyouts.Text9"), !run, Button8_Click),
            (App.GetLanguage("MainWindow.Flyouts.Text11"), !run, Button6_Click),
            (App.GetLanguage("MainWindow.Flyouts.Text12"), !run, Button13_Click),
            (App.GetLanguage("MainWindow.Flyouts.Text13"), run, Button10_Click)
        }, con);
    }
    private void Button3_Click()
    {
        App.ShowGameEdit(Obj.Obj);
    }

    private void Button13_Click()
    {
        Obj.Copy();
    }

    private void Button12_Click()
    {
        Obj.Rename();
    }

    private void Button11_Click()
    {
        App.ShowAdd(Obj.Obj, FileType.Mod);
    }

    private void Button10_Click()
    {
        BaseBinding.StopGame(Obj.Obj);
    }

    private async void Button9_Click()
    {
        await GameBinding.SetGameIconFromFile(Obj.Con.Window, Obj.Obj);
        Obj.Reload();
    }

    private void Button8_Click()
    {
        App.ShowGameEdit(Obj.Obj, GameEditWindowType.Export);
    }

    private void Button7_Click()
    {
        GameBinding.OpPath(Obj.Obj);
    }

    private void Button6_Click()
    {
        Obj.DeleteGame();
    }

    private void Button5_Click()
    {
        Obj.EditGroup();
    }

    private void Button4_Click()
    {
        App.ShowGameEdit(Obj.Obj, GameEditWindowType.World);
    }

    private void Button2_Click()
    {
        App.ShowGameEdit(Obj.Obj, GameEditWindowType.Mod);
    }

    private void Button1_Click()
    {
        App.ShowGameLog(Obj.Obj);
    }
}
