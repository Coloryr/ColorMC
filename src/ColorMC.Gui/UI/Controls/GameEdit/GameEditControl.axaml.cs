using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.GameEdit;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

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

    private Bitmap _icon;
    public override Bitmap GetIcon() => _icon;

    public override string Title =>
        string.Format(App.Lang("GameEditWindow.Title"), _obj.Name);

    public override string UseName { get; }

    public GameEditControl(GameSettingObj obj)
    {
        UseName = (ToString() ?? "GameEditControl") + ":" + obj.UUID;

        _obj = obj;
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
                    await model.LoadShaderpack();
                    break;
                case 8:
                    await model.LoadSchematic();
                    break;
            }

            return true;
        }

        return false;
    }
    public override void Opened()
    {
        Window.SetTitle(Title);

        var icon = _obj.GetIconFile();
        if (File.Exists(icon))
        {
            _icon = new(icon);
            Window.SetIcon(_icon);
        }
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

    public override void Closed()
    {
        _icon?.Dispose();

        App.GameEditWindows.Remove(_obj.UUID);
    }

    public void Started()
    {
        (DataContext as GameEditModel)!.GameStateChange();
    }

    protected override MenuModel SetModel(BaseModel model)
    {
        return new GameEditModel(model, _obj);
    }
    protected override Control ViewChange(bool iswhell, int old, int index)
    {
        var model = (DataContext as GameEditModel)!;
        switch (old)
        {
            case 2:
                model.RemoveChoise();
                break;
            case 6:
                model.RemoveChoiseTab10();
                break;
        }
        switch (index)
        {
            case 0:
                model.GameLoad();
                _tab1 ??= new();
                if (iswhell && old == 1)
                {
                    _tab1.End();
                }
                else
                {
                    _tab1.Reset();
                }
                return _tab1;
            case 1:
                model.ConfigLoad();
                _tab2 ??= new();
                if (iswhell && old == 2)
                {
                    _tab2.End();
                }
                else
                {
                    _tab2.Reset();
                }
                return _tab2;
            case 2:
                model.SetChoise();
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
                model.SetChoiseTab10();
                model.LoadServer();
                return _tab10 ??= new();
            case 7:
                _ = model.LoadShaderpack();
                return _tab11 ??= new();
            case 8:
                _ = model.LoadSchematic();
                return _tab12 ??= new();
            default:
                throw new InvalidEnumArgumentException();
        }
    }
}
