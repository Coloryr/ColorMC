using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.ServerPack;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Controls.ServerPack;

/// <summary>
/// 服务器实例生成窗口
/// </summary>
public partial class ServerPackControl : MenuControl
{
    /// <summary>
    /// 游戏实例
    /// </summary>
    private readonly GameSettingObj _obj;

    private Tab1Control _tab1;
    private Tab2Control _tab2;
    private Tab3Control _tab3;
    private Tab4Control _tab4;

    public ServerPackControl() : base(WindowManager.GetUseName<ServerPackControl>())
    {

    }

    public ServerPackControl(GameSettingObj obj) : base(WindowManager.GetUseName<ServerPackControl>(obj))
    {
        _obj = obj;
        Title = string.Format(App.Lang("ServerPackWindow.Title"), _obj.Name);

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

        Title = string.Format(App.Lang("ServerPackWindow.Title"), _obj.Name);
    }

    private void EventManager_GameIconChange(object? sender, string uuid)
    {
        if (uuid != _obj.UUID)
        {
            return;
        }

        ReloadIcon();
    }


    public override void Opened()
    {
        if (DataContext is ServerPackModel model)
        {
            model.NowView = 0;
        }
    }

    public override void Closed()
    {
        EventManager.GameIconChange -= EventManager_GameIconChange;
        EventManager.GameNameChange -= EventManager_GameNameChange;
        EventManager.GameDelete -= EventManager_GameDelete;

        WindowManager.ServerPackWindows.Remove(_obj.UUID);
    }

    protected override TopModel GenModel(BaseModel model)
    {
        var pack = GameBinding.GetServerPack(_obj);
        if (pack == null)
        {
            pack = new()
            {
                Game = _obj,
                Mod = [],
                Resourcepack = [],
                Config = []
            };

            GameBinding.SaveServerPack(pack);
        }

        return new ServerPackModel(model, pack);
    }

    protected override Control ViewChange(int old, int index)
    {
        var model = (DataContext as ServerPackModel)!;
        switch (model.NowView)
        {
            case 0:
                model.LoadConfig();
                return _tab1 ??= new();
            case 1:
                model.LoadMod();
                return _tab2 ??= new();
            case 2:
                model.LoadResourceList();
                return _tab3 ??= new();
            case 3:
                model.LoadFile();
                return _tab4 ??= new();
            default:
                throw new InvalidEnumArgumentException();
        }
    }
    public override Bitmap GetIcon()
    {
        return ImageManager.GetGameIcon(_obj) ?? ImageManager.GameIcon;
    }
}
