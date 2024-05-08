using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Flyouts;

public class MainFlyout
{
    public MainFlyout(Control con, GameItemModel obj)
    {
        var run = BaseBinding.IsGameRun(obj.Obj);

        _ = new FlyoutsControl(
        [
            (App.Lang("MainWindow.Flyouts.Text2"), true, ()=>
            {
                App.ShowGameEdit(obj.Obj);
            }),
            (App.Lang("MainWindow.Flyouts.Text1"), true, ()=>
            {
                App.ShowGameLog(obj.Obj);
            }),
            (App.Lang("MainWindow.Flyouts.Text3"), true, ()=>
            {
                App.ShowAdd(obj.Obj, FileType.Mod);
            }),
            (App.Lang("MainWindow.Flyouts.Text4"), true, ()=>
            {
                App.ShowGameEdit(obj.Obj, GameEditWindowType.Mod);
            }),
            (App.Lang("MainWindow.Flyouts.Text6"), true, ()=>
            {
                App.ShowGameEdit(obj.Obj, GameEditWindowType.World);
            }),
            (App.Lang("Button.OpFile"), true, ()=>
            {
                PathBinding.OpPath(obj.Obj, PathType.GamePath);
            }),
            (App.Lang("MainWindow.Flyouts.Text7"), true, obj.EditGroup),
            (App.Lang("MainWindow.Flyouts.Text8"), true, async ()=>
            {
                await GameBinding.SetGameIconFromFile(obj.Model, obj.Obj);
                obj.LoadIcon();
            }),
            (App.Lang("MainWindow.Flyouts.Text15"), SystemInfo.Os == OsType.Windows, ()=>
            {
                BaseBinding.CreateLaunch(obj.Obj);
            }),
            (App.Lang("MainWindow.Flyouts.Text14"), GameCloudUtils.Connect, ()=>
            {
                App.ShowGameCloud(obj.Obj);
            }),
            (App.Lang("MainWindow.Flyouts.Text10"), !run, obj.Rename),
            (App.Lang("MainWindow.Flyouts.Text9"), !run, ()=>
            {
                App.ShowGameExport(obj.Obj);
            }),
            (App.Lang("MainWindow.Flyouts.Text11"), !run, obj.DeleteGame),
            (App.Lang("MainWindow.Flyouts.Text12"), !run,  obj.Copy),
            (App.Lang("MainWindow.Flyouts.Text16"),
                GameJoystick.NowGameJoystick.ContainsKey(obj.Obj.UUID), obj.SetJoystick),
            (App.Lang("MainWindow.Flyouts.Text13"), run, ()=>
            {
                BaseBinding.StopGame(obj.Obj);
            })
        ], con);
    }
}
