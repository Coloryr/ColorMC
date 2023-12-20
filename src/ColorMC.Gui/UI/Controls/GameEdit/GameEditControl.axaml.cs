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

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class GameEditControl : MenuControl
{
    private readonly GameSettingObj _obj;

    private readonly Tab1Control _tab1 = new();
    private readonly Tab2Control _tab2 = new();
    private readonly Tab4Control _tab4 = new();
    private readonly Tab5Control _tab5 = new();
    private readonly Tab8Control _tab8 = new();
    private readonly Tab9Control _tab9 = new();
    private readonly Tab10Control _tab10 = new();
    private readonly Tab11Control _tab11 = new();
    private readonly Tab12Control _tab12 = new();

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

    public override async void OnKeyDown(object? sender, KeyEventArgs e)
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
        }
    }
    public override void Opened()
    {
        Window.SetTitle(Title);

        Content1.Child = _tab1;

        _tab4.Opened();
        _tab10.Opened();
        _tab11.Opened();
        _tab12.Opened();

        var icon = _obj.GetIconFile();
        if (File.Exists(icon))
        {
            _icon = new(icon);
            Window.SetIcon(_icon);
        }

        (DataContext as GameEditModel)?.OpenLoad();
    }

    public void SetType(GameEditWindowType type)
    {
        var model = (DataContext as GameEditModel)!;
        switch (type)
        {
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
                return _tab4;
            case 3:
                _ = model.LoadWorld();
                return _tab5;
            case 4:
                _ = model.LoadResource();
                return _tab8;
            case 5:
                model.LoadScreenshot();
                return _tab9;
            case 6:
                model.SetChoiseTab10();
                model.LoadServer();
                return _tab10;
            case 7:
                _ = model.LoadShaderpack();
                return _tab11;
            case 8:
                _ = model.LoadSchematic();
                return _tab12;
            default:
                throw new InvalidEnumArgumentException();
        }
    }
}
