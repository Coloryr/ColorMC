using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Hook;
using ColorMC.Gui.Joystick;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Net.Apis;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;

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
            new FlyoutMenuModel(LanguageUtils.Get("MainWindow.Text44"), true, ()=>
            {
                WindowManager.ShowAdd(obj.Obj, FileType.Mod);
            }),
            new FlyoutMenuModel(LanguageUtils.Get("MainWindow.Text43"), true, null)
            {
                SubItem =
                [
                    new FlyoutMenuModel(LanguageUtils.Get("MainWindow.Text59"), true, ()=>
                    {
                        WindowManager.ShowGameEdit(obj.Obj);
                    }),
                    new FlyoutMenuModel(LanguageUtils.Get("MainWindow.Text64"), true, ()=>
                    {
                        WindowManager.ShowGameEdit(obj.Obj, GameEditWindowType.Arg);
                    }),
                    new FlyoutMenuModel(LanguageUtils.Get("MainWindow.Text45"), true, ()=>
                    {
                        WindowManager.ShowGameEdit(obj.Obj, GameEditWindowType.Mod);
                    }),
                    new FlyoutMenuModel(LanguageUtils.Get("MainWindow.Text46"), true, ()=>
                    {
                        WindowManager.ShowGameEdit(obj.Obj, GameEditWindowType.World);
                    })
                ]
            },
            new FlyoutMenuModel(LanguageUtils.Get("MainWindow.Text42"), true, ()=>
            {
                WindowManager.ShowGameLog(obj.Obj);
            }),
            new FlyoutMenuModel(LanguageUtils.Get("Button.OpFile"), true, ()=>
            {
                PathBinding.OpenPath(obj.Obj, PathType.GamePath);
            }),
            new FlyoutMenuModel(LanguageUtils.Get("MainWindow.Text58"), true, null)
            {
                SubItem =
                [
                    new FlyoutMenuModel(LanguageUtils.Get("MainWindow.Text55"), SystemInfo.Os == OsType.Windows, ()=>
                    {
                        HookUtils.CreateLaunch(obj.Obj);
                    }),
                    new FlyoutMenuModel(LanguageUtils.Get("MainWindow.Text54"), ColorMCCloudAPI.Connect, ()=>
                    {
                        WindowManager.ShowGameCloud(obj.Obj);
                    }),
                    new FlyoutMenuModel(LanguageUtils.Get("MainWindow.Text49"), !run, ()=>
                    {
                        WindowManager.ShowGameExport(obj.Obj);
                    }),
                    new FlyoutMenuModel(LanguageUtils.Get("MainWindow.Text63"), true, obj.ExportCmd),
                ]
            },
            new FlyoutMenuModel(LanguageUtils.Get("MainWindow.Text57"), true, null)
            {
                SubItem =
                [
                    new FlyoutMenuModel(LanguageUtils.Get("Text.Rename"), !run, obj.Rename),
                    new FlyoutMenuModel(LanguageUtils.Get("MainWindow.Text51"), !run, obj.DeleteGame),
                    new FlyoutMenuModel(LanguageUtils.Get("MainWindow.Text52"), !run, obj.Copy),
                    new FlyoutMenuModel(LanguageUtils.Get("MainWindow.Text47"), true, obj.EditGroup),
                    new FlyoutMenuModel(LanguageUtils.Get("MainWindow.Text48"), true, async ()=>
                    {
                        var top = TopLevel.GetTopLevel(con);
                        if (top == null)
                        {
                            return;
                        }
                        await GameBinding.SetGameIconFromFileAsync(top, obj.Window, obj.Obj);
                    }),
                    new FlyoutMenuModel(LanguageUtils.Get("MainWindow.Text53"), run, ()=>
                    {
                        GameManager.KillGame(obj.Obj);
                    })
                ]
            },
            new FlyoutMenuModel(LanguageUtils.Get("MainWindow.Text56"),
                GameJoystick.NowGameJoystick.ContainsKey(obj.Obj.UUID), obj.SetJoystick),
        ]).Show(con);
    }
}
