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
    private readonly GameItemModel _obj;
    public MainFlyout(Control con, GameItemModel obj)
    {
        _obj = obj;

        var run = BaseBinding.IsGameRun(obj.Obj);

        _ = new FlyoutsControl(
        [
            (App.Lang("MainWindow.Flyouts.Text2"), true, ()=>
            {
                App.ShowGameEdit(_obj.Obj);
            }),
            (App.Lang("MainWindow.Flyouts.Text1"), true, ()=>
            {
                App.ShowGameLog(_obj.Obj);
            }),
            (App.Lang("MainWindow.Flyouts.Text3"), true, ()=>
            {
                App.ShowAdd(_obj.Obj, FileType.Mod);
            }),
            (App.Lang("MainWindow.Flyouts.Text4"), true, ()=>
            {
                App.ShowGameEdit(_obj.Obj, GameEditWindowType.Mod);
            }),
            (App.Lang("MainWindow.Flyouts.Text6"), true, ()=>
            {
                App.ShowGameEdit(_obj.Obj, GameEditWindowType.World);
            }),
            (App.Lang("Button.OpFile"), true, ()=>
            {
                GameBinding.OpPath(_obj.Obj);
            }),
            (App.Lang("MainWindow.Flyouts.Text7"), true, _obj.EditGroup),
            (App.Lang("MainWindow.Flyouts.Text8"), true, async ()=>
            {
                await GameBinding.SetGameIconFromFile(_obj.Model, _obj.Obj);
                _obj.LoadIcon();
            }),
            (App.Lang("MainWindow.Flyouts.Text15"), SystemInfo.Os == OsType.Windows, ()=>
            {
                BaseBinding.CreateLaunch(_obj.Obj);
            }),
            (App.Lang("MainWindow.Flyouts.Text14"), GameCloudUtils.Connect, ()=>
            {
                App.ShowGameCloud(_obj.Obj);
            }),
            (App.Lang("MainWindow.Flyouts.Text10"), !run, _obj.Rename),
            (App.Lang("MainWindow.Flyouts.Text9"), !run, ()=>
            {
                App.ShowGameExport(_obj.Obj);
            }),
            (App.Lang("MainWindow.Flyouts.Text11"), !run, _obj.DeleteGame),
            (App.Lang("MainWindow.Flyouts.Text12"), !run,  _obj.Copy),
            (App.Lang("MainWindow.Flyouts.Text13"), run, ()=>
            {
                BaseBinding.StopGame(_obj.Obj);
            })
        ], con);
    }
}
