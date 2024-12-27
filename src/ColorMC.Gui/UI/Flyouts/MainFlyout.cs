using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Joystick;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Flyouts;

public class MainFlyout
{
    public MainFlyout(Control con, GameItemModel obj)
    {
        var run = GameManager.IsGameRun(obj.Obj);

        _ = new FlyoutsControl(
        [
            new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text3"), true, ()=>
            {
                WindowManager.ShowAdd(obj.Obj, FileType.Mod);
            }),
            new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text2"), true, null)
            {
                SubItem =
                [
                    new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text19"), true, ()=>
                    {
                        WindowManager.ShowGameEdit(obj.Obj);
                    }),
                    new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text4"), true, ()=>
                    {
                        WindowManager.ShowGameEdit(obj.Obj, GameEditWindowType.Mod);
                    }),
                    new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text6"), true, ()=>
                    {
                        WindowManager.ShowGameEdit(obj.Obj, GameEditWindowType.World);
                    }),
                ]
            },
            new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text1"), true, ()=>
            {
                WindowManager.ShowGameLog(obj.Obj);
            }),
            new FlyoutMenuObj(App.Lang("Button.OpFile"), true, ()=>
            {
                PathBinding.OpenPath(obj.Obj, PathType.GamePath);
            }),
            new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text18"), true, null)
            {
                SubItem =
                [
                    new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text15"), SystemInfo.Os == OsType.Windows, ()=>
                    {
                        BaseBinding.CreateLaunch(obj.Obj);
                    }),
                    new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text14"), GameCloudUtils.Connect, ()=>
                    {
                        WindowManager.ShowGameCloud(obj.Obj);
                    }),
                    new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text9"), !run, ()=>
                    {
                        WindowManager.ShowGameExport(obj.Obj);
                    }),
                ]
            },
            new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text17"), true, null)
            {
                SubItem =
                [
                    new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text10"), !run, obj.Rename),
                    new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text11"), !run, obj.DeleteGame),
                    new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text12"), !run, obj.Copy),
                    new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text7"), true, obj.EditGroup),
                    new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text8"), true, async ()=>
                    {
                        var top = TopLevel.GetTopLevel(con);
                        if (top == null)
                        {
                            return;
                        }
                        await GameBinding.SetGameIconFromFile(top, obj.Model, obj.Obj);
                        obj.ReloadIcon();
                    }),
                    new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text13"), run, ()=>
                    {
                        GameManager.KillGame(obj.Obj);
                    })
                ]
            },
            new FlyoutMenuObj(App.Lang("MainWindow.Flyouts.Text16"),
                GameJoystick.NowGameJoystick.ContainsKey(obj.Obj.UUID), obj.SetJoystick),
        ], con);
    }
}
