using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.GameEdit;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class GameEditControl : MenuControl
{
    private readonly GameSettingObj _obj;

    private Tab1Control _tab1;
    private Tab2Control _tab2;
    private Tab4Control _tab4;
    private Tab5Control _tab5;
    private Tab8Control _tab8;
    private Tab9Control _tab9;
    private Tab10Control _tab10;
    private Tab11Control _tab11;
    private Tab12Control _tab12;

    public GameEditControl(GameSettingObj obj)
    {
        _obj = obj;

        UseName = (ToString() ?? "GameEditControl") + ":" + obj.UUID;
        Title = string.Format(App.Lang("GameEditWindow.Title"), _obj.Name);
    }

    public override async Task<bool> OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            var model = (DataContext as GameEditModel)!;
            switch (model.NowView)
            {
                case 2:
                    await model.LoadMod();
                    break;
                case 3:
                    await model.LoadWorld();
                    break;
                case 4:
                    await model.LoadResource();
                    break;
                case 5:
                    model.LoadScreenshot();
                    break;
                case 6:
                    model.LoadServer();
                    break;
                case 7:
                    model.LoadShaderpack();
                    break;
                case 8:
                    model.LoadSchematic();
                    break;
            }

            return true;
        }

        return false;
    }
    public override void Opened()
    {
        Window.SetTitle(Title);
    }

    public override void Closed()
    {
        WindowManager.GameEditWindows.Remove(_obj.UUID);
    }

    public override TopModel GenModel(BaseModel model)
    {
        return new GameEditModel(model, _obj);
    }

    protected override Control ViewChange(int old, int index)
    {
        var model = (DataContext as GameEditModel)!;
        switch (index)
        {
            case 0:
                model.GameLoad();
                _tab1 ??= new();
                return _tab1;
            case 1:
                model.ConfigLoad();
                _tab2 ??= new();
                return _tab2;
            case 2:
                _ = model.LoadMod();
                return _tab4 ??= new();
            case 3:
                _ = model.LoadWorld();
                return _tab5 ??= new();
            case 4:
                _ = model.LoadResource();
                return _tab8 ??= new();
            case 5:
                model.LoadScreenshot();
                return _tab9 ??= new();
            case 6:
                model.LoadServer();
                return _tab10 ??= new();
            case 7:
                model.LoadShaderpack();
                return _tab11 ??= new();
            case 8:
                model.LoadSchematic();
                return _tab12 ??= new();
            default:
                throw new InvalidEnumArgumentException();
        }
    }

    public override Bitmap GetIcon()
    {
        var icon = ImageManager.GetGameIcon(_obj);
        return icon ?? ImageManager.GameIcon;
    }

    public void SetType(GameEditWindowType type)
    {
        var model = (DataContext as GameEditModel)!;
        switch (type)
        {
            case GameEditWindowType.Normal:
                model.NowView = 0;
                break;
            case GameEditWindowType.Mod:
                model.NowView = 2;
                break;
            case GameEditWindowType.World:
                model.NowView = 3;
                break;
        }
    }

    public void Started()
    {
        (DataContext as GameEditModel)!.GameStateChange();
    }
}
