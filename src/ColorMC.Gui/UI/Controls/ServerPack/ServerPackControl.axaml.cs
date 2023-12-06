using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.ServerPack;
using ColorMC.Gui.UIBinding;
using System.ComponentModel;
using System.IO;

namespace ColorMC.Gui.UI.Controls.ServerPack;

public partial class ServerPackControl : MenuControl
{
    private readonly GameSettingObj _obj;

    private readonly Tab1Control _tab1 = new();
    private readonly Tab2Control _tab2 = new();
    private readonly Tab3Control _tab3 = new();
    private readonly Tab4Control _tab4 = new();

    private Bitmap _icon;
    public override Bitmap GetIcon() => _icon;

    public override string Title => string.Format(App.Lang("ServerPackWindow.Title"),
           _obj.Name);

    public override string UseName { get; }

    public ServerPackControl()
    {
        UseName = ToString() ?? "ServerPackControl";
    }

    public ServerPackControl(GameSettingObj obj) : this()
    {
        _obj = obj;
    }

    public override void Opened()
    {
        Window.SetTitle(Title);

        Content1.Child = _tab1;

        _tab2.Opened();
        _tab3.Opened();
        _tab4.Opened();

        var icon = _obj.GetIconFile();
        if (File.Exists(icon))
        {
            _icon = new(icon);
            Window.SetIcon(_icon);
        }
    }

    public override void Closed()
    {
        _icon?.Dispose();

        App.ServerPackWindows.Remove(_obj.UUID);
    }

    protected override MenuModel SetModel(BaseModel model)
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
                return _tab1;
            case 1:
                model.LoadMod();
                return _tab2;
            case 2:
                model.LoadConfigList();
                return _tab3;
            case 3:
                model.LoadFile();
                return _tab4;
            default:
                throw new InvalidEnumArgumentException();
        }
    }
}
