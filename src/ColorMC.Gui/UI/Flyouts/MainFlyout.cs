using Avalonia.Controls;
using Avalonia.VisualTree;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.Main;
using ColorMC.Gui.UIBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Flyouts;

public class MainFlyout
{
    private GameControl Obj;
    private MainControl Win;
    public MainFlyout(MainControl win, GameControl obj)
    {
        Win = win;
        Obj = obj;

        var run = BaseBinding.IsGameRun(obj.Obj);

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("MainWindow.Control.Text4"), !run, Button1_Click),
            (App.GetLanguage("MainWindow.Flyouts.Text8"), true, Button14_Click),
            (App.GetLanguage("MainWindow.Flyouts.Text7"), true, Button11_Click),
            (App.GetLanguage("MainWindow.Flyouts.Text1"), true, Button2_Click),
            (App.GetLanguage("MainWindow.Flyouts.Text2"), true, Button3_Click),
            (App.GetLanguage("MainWindow.Flyouts.Text3"), true, Button4_Click),
            (App.GetLanguage("MainWindow.Flyouts.Text4"), true, Button5_Click),
            (App.GetLanguage("Button.OpFile"), true, Button7_Click),
            (App.GetLanguage("MainWindow.Control.Text8"), true, Button9_Click),
            (App.GetLanguage("MainWindow.Control.Text6"), !run, Button8_Click),
            (App.GetLanguage("MainWindow.Control.Text12"), !run, Button12_Click),
            (App.GetLanguage("MainWindow.Control.Text5"), !run, Button6_Click),
            (App.GetLanguage("MainWindow.Control.Text13"), !run, Button13_Click),
            (App.GetLanguage("MainWindow.Control.Text6"), run, Button10_Click)
        }, win);
    }

    private void Button14_Click()
    {
        App.ShowGameEdit(Obj.Obj, GameEditWindowType.Normal);
    }

    private void Button13_Click()
    {
        Win.Copy(Obj.Obj);
    }

    private void Button12_Click()
    {
        Win.Rename(Obj.Obj);
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
        await GameBinding.SetGameIconFromFile(App.FindRoot(Win.GetVisualRoot()), Obj.Obj);
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
        Win.DeleteGame(Obj.Obj);
    }

    private void Button5_Click()
    {
        Win.EditGroup(Obj.Obj);
    }

    private void Button4_Click()
    {
        App.ShowGameEdit(Obj.Obj, GameEditWindowType.World);
    }

    private void Button3_Click()
    {
        App.ShowGameEdit(Obj.Obj, GameEditWindowType.Config);
    }

    private void Button2_Click()
    {
        App.ShowGameEdit(Obj.Obj, GameEditWindowType.Mod);
    }

    private void Button1_Click()
    {
        Win.Launch(true);
    }
}
