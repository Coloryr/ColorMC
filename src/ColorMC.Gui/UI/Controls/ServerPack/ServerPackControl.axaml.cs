using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.ServerPack;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Controls.ServerPack;

public partial class ServerPackControl : MenuControl
{
    private readonly GameSettingObj _obj;

    private Tab1Control _tab1;
    private Tab2Control _tab2;
    private Tab3Control _tab3;
    private Tab4Control _tab4;

    public ServerPackControl()
    {
        UseName = ToString() ?? "ServerPackControl";
    }

    public ServerPackControl(GameSettingObj obj) : this()
    {
        _obj = obj;
        Title = string.Format(App.Lang("ServerPackWindow.Title"),
           _obj.Name);
        UseName += ":" + obj.UUID;
    }

    public override void Opened()
    {
        Window.SetTitle(Title);

        if (DataContext is ServerPackModel model)
        {
            model.NowView = 0;
        }
    }

    public override void Closed()
    {
        WindowManager.ServerPackWindows.Remove(_obj.UUID);
    }

    public override void SetModel(BaseModel model)
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

        DataContext = new ServerPackModel(model, pack);
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
        var icon = ImageManager.GetGameIcon(_obj);
        return icon ?? ImageManager.GameIcon;
    }
}
