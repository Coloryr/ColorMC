using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Hook;
using ColorMC.Gui.Joystick;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Net.Apis;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Flyouts;

/// <summary>
/// 主界面
/// 游戏实例右键菜单
/// </summary>
public static class MainFlyout
{
    public static void Show(Control con, GameItemModel obj)
    {
        var run = GameManager.IsGameRun(obj.Obj);

        new FlyoutsControl(
        [
            new FlyoutMenuModel(App.Lang("MainWindow.Flyouts.Text3"), true, ()=>
            {
                WindowManager.ShowAdd(obj.Obj, FileType.Mod);
            }),
            new FlyoutMenuModel(App.Lang("MainWindow.Flyouts.Text2"), true, null)
            {
                SubItem =
                [
                    new FlyoutMenuModel(App.Lang("MainWindow.Flyouts.Text19"), true, ()=>
                    {
                        WindowManager.ShowGameEdit(obj.Obj);
                    }),
                    new FlyoutMenuModel(App.Lang("MainWindow.Flyouts.Text24"), true, ()=>
                    {
                        WindowManager.ShowGameEdit(obj.Obj, GameEditWindowType.Arg);
                    }),
                    new FlyoutMenuModel(App.Lang("MainWindow.Flyouts.Text4"), true, ()=>
                    {
                        WindowManager.ShowGameEdit(obj.Obj, GameEditWindowType.Mod);
                    }),
                    new FlyoutMenuModel(App.Lang("MainWindow.Flyouts.Text6"), true, ()=>
                    {
                        WindowManager.ShowGameEdit(obj.Obj, GameEditWindowType.World);
                    })
                ]
            },
            new FlyoutMenuModel(App.Lang("MainWindow.Flyouts.Text1"), true, ()=>
            {
                WindowManager.ShowGameLog(obj.Obj);
            }),
            new FlyoutMenuModel(App.Lang("Button.OpFile"), true, ()=>
            {
                PathBinding.OpenPath(obj.Obj, PathType.GamePath);
            }),
            new FlyoutMenuModel(App.Lang("MainWindow.Flyouts.Text18"), true, null)
            {
                SubItem =
                [
                    new FlyoutMenuModel(App.Lang("MainWindow.Flyouts.Text15"), SystemInfo.Os == OsType.Windows, ()=>
                    {
                        HookUtils.CreateLaunch(obj.Obj);
                    }),
                    new FlyoutMenuModel(App.Lang("MainWindow.Flyouts.Text14"), ColorMCCloudAPI.Connect, ()=>
                    {
                        WindowManager.ShowGameCloud(obj.Obj);
                    }),
                    new FlyoutMenuModel(App.Lang("MainWindow.Flyouts.Text9"), !run, ()=>
                    {
                        WindowManager.ShowGameExport(obj.Obj);
                    }),
                    new FlyoutMenuModel(App.Lang("MainWindow.Flyouts.Text23"), true, obj.ExportCmd),
                ]
            },
            new FlyoutMenuModel(App.Lang("MainWindow.Flyouts.Text17"), true, null)
            {
                SubItem =
                [
                    new FlyoutMenuModel(App.Lang("MainWindow.Flyouts.Text10"), !run, obj.Rename),
                    new FlyoutMenuModel(App.Lang("MainWindow.Flyouts.Text11"), !run, obj.DeleteGame),
                    new FlyoutMenuModel(App.Lang("MainWindow.Flyouts.Text12"), !run, obj.Copy),
                    new FlyoutMenuModel(App.Lang("MainWindow.Flyouts.Text7"), true, obj.EditGroup),
                    new FlyoutMenuModel(App.Lang("MainWindow.Flyouts.Text8"), true, async ()=>
                    {
                        var top = TopLevel.GetTopLevel(con);
                        if (top == null)
                        {
                            return;
                        }
                        await GameBinding.SetGameIconFromFileAsync(top, obj.Model, obj.Obj);
                        obj.ReloadIcon();
                    }),
                    new FlyoutMenuModel(App.Lang("MainWindow.Flyouts.Text13"), run, ()=>
                    {
                        GameManager.KillGame(obj.Obj);
                    })
                ]
            },
            new FlyoutMenuModel(App.Lang("MainWindow.Flyouts.Text16"),
                GameJoystick.NowGameJoystick.ContainsKey(obj.Obj.UUID), obj.SetJoystick),
        ]).Show(con);
    }
}
