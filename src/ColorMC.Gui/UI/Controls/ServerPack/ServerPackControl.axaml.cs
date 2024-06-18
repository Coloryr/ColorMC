using System.ComponentModel;
using System.IO;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
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
        UseName += ":" + obj.UUID;
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

        if (DataContext is ServerPackModel model)
        {
            model.NowView = 0;
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

    protected override Control ViewChange(bool iswhell, int old, int index)
    {
        var model = (DataContext as ServerPackModel)!;
        switch (old)
        {
            case 1:
            case 2:
            case 4:
                model.RemoveChoise();
                break;
        }
        switch (model.NowView)
        {
            case 0:
                model.LoadConfig();
                return _tab1 ??= new();
            case 1:
                model.LoadMod();
                model.SetTab2Click();
                return _tab2 ??= new();
            case 2:
                model.LoadConfigList();
                model.SetTab3Click();
                return _tab3 ??= new();
            case 3:
                model.LoadFile();
                model.SetTab4Click();
                return _tab4 ??= new();
            default:
                throw new InvalidEnumArgumentException();
        }
    }
}
