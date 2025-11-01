using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.GameExport;

namespace ColorMC.Gui.UI.Controls.GameExport;

/// <summary>
/// 游戏实例导出
/// </summary>
public partial class GameExportControl : MenuControl
{
    /// <summary>
    /// 游戏实例
    /// </summary>
    private readonly GameSettingObj _obj;

    private Tab1Control _tab1;
    private Tab2Control _tab2;
    private Tab3Control _tab3;
    private Tab4Control _tab4;

    public GameExportControl(GameSettingObj obj) : base(WindowManager.GetUseName<GameExportControl>(obj))
    {
        _obj = obj;

        Title = string.Format(LanguageUtils.Get("GameExportWindow.Title"), _obj.Name);

        EventManager.GameIconChange += EventManager_GameIconChange;
        EventManager.GameNameChange += EventManager_GameNameChange;
        EventManager.GameDelete += EventManager_GameDelete;
    }

    private void EventManager_GameDelete(object? sender, string uuid)
    {
        if (uuid != _obj.UUID)
        {
            return;
        }

        Window?.Close();
    }

    private void EventManager_GameNameChange(object? sender, string uuid)
    {
        if (uuid != _obj.UUID)
        {
            return;
        }

        Title = string.Format(LanguageUtils.Get("GameExportWindow.Title"), _obj.Name);
    }

    private void EventManager_GameIconChange(object? sender, string uuid)
    {
        if (uuid != _obj.UUID)
        {
            return;
        }

        ReloadIcon();
    }

    public override async void Opened()
    {
        var model = (DataContext as GameExportModel)!;
        model.Model.Progress(LanguageUtils.Get("GameExportWindow.Info7"));

        await model.LoadMod();
        model.LoadFile();

        model.Model.ProgressClose();
        model.NowView = 0;
    }

    public override void Closed()
    {
        EventManager.GameIconChange -= EventManager_GameIconChange;
        EventManager.GameNameChange -= EventManager_GameNameChange;
        EventManager.GameDelete -= EventManager_GameDelete;

        WindowManager.GameExportWindows.Remove(_obj.UUID);
    }

    protected override TopModel GenModel(BaseModel model)
    {
        return new GameExportModel(model, _obj);
    }

    protected override Control ViewChange(int old, int index)
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
        return ImageManager.GetGameIcon(_obj) ?? ImageManager.GameIcon;
    }
}
