using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Flyouts;

public class MainFlyout
{
    private readonly GameItemModel _obj;
    public MainFlyout(Control con, GameItemModel obj)
    {
        _obj = obj;

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
            (App.GetLanguage("MainWindow.Flyouts.Text14"), GameCloudUtils.Connect, Button14_Click),
            (App.GetLanguage("MainWindow.Flyouts.Text10"), !run, Button12_Click),
            (App.GetLanguage("MainWindow.Flyouts.Text9"), !run, Button8_Click),
            (App.GetLanguage("MainWindow.Flyouts.Text11"), !run, Button6_Click),
            (App.GetLanguage("MainWindow.Flyouts.Text12"), !run, Button13_Click),
            (App.GetLanguage("MainWindow.Flyouts.Text13"), run, Button10_Click)
            
        }, con);
    }

    private void Button14_Click()
    {
        App.ShowGameCloud(_obj.Obj);
    }
    private void Button3_Click()
    {
        App.ShowGameEdit(_obj.Obj);
    }

    private void Button13_Click()
    {
        _obj.Copy();
    }

    private void Button12_Click()
    {
        _obj.Rename();
    }

    private void Button11_Click()
    {
        App.ShowAdd(_obj.Obj, FileType.Mod);
    }

    private void Button10_Click()
    {
        BaseBinding.StopGame(_obj.Obj);
    }

    private async void Button9_Click()
    {
        await GameBinding.SetGameIconFromFile(_obj.Window, _obj.Obj);
        _obj.Reload();
    }

    private void Button8_Click()
    {
        App.ShowGameExport(_obj.Obj);
    }

    private void Button7_Click()
    {
        GameBinding.OpPath(_obj.Obj);
    }

    private void Button6_Click()
    {
        _obj.DeleteGame();
    }

    private void Button5_Click()
    {
        _obj.EditGroup();
    }

    private void Button4_Click()
    {
        App.ShowGameEdit(_obj.Obj, GameEditWindowType.World);
    }

    private void Button2_Click()
    {
        App.ShowGameEdit(_obj.Obj, GameEditWindowType.Mod);
    }

    private void Button1_Click()
    {
        App.ShowGameLog(_obj.Obj);
    }
}
