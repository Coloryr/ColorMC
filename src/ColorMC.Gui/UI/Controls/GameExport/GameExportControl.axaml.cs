using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.GameExport;

namespace ColorMC.Gui.UI.Controls.GameExport;

public partial class GameExportControl : MenuControl
{
    private readonly GameSettingObj _obj;

    private Tab1Control _tab1;
    private Tab2Control _tab2;
    private Tab3Control _tab3;
    private Tab4Control _tab4;

    public GameExportControl(GameSettingObj obj)
    {
        _obj = obj;

        Title = string.Format(App.Lang("GameExportWindow.Title"), _obj.Name);
        UseName = (ToString() ?? "GameExportControl") + ":" + obj.UUID;
    }

    public override async void Opened()
    {
        Window.SetTitle(Title);

        var model = (DataContext as GameExportModel)!;
        model.Model.Progress(App.Lang("GameExportWindow.Info7"));

        await model.LoadMod();
        model.LoadFile();

        model.Model.ProgressClose();
        model.NowView = 0;
    }

    public override void Closed()
    {
        WindowManager.GameExportWindows.Remove(_obj.UUID);
    }

    public override void SetBaseModel(BaseModel model)
    {
        DataContext = new GameExportModel(model, _obj);
    }

    protected override Control ViewChange(bool iswhell, int old, int index)
    {
        var model = (DataContext as GameExportModel)!;
        if (old == 1 || old == 2)
        {
            model.RemoveChoise();
        }

        if (index == 1)
        {
            model.SetTab2Choise();
        }
        else if (index == 2)
        {
            model.SetTab3Choise();
        }

        return index switch
        {
            0 => _tab1 ??= new(),
            1 => _tab2 ??= new(),
            2 => _tab3 ??= new(),
            3 => _tab4 ??= new(),
            _ => throw new InvalidEnumArgumentException(),
        };
    }

    public override Bitmap GetIcon()
    {
        var icon = ImageManager.GetGameIcon(_obj);
        return icon ?? ImageManager.GameIcon;
    }
}
