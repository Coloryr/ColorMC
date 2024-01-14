using Avalonia.Controls;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.NetFrp;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.NetFrp;

public partial class NetFrpControl : MenuControl
{
    private readonly NetFrpTab1Control _tab1 = new();
    private readonly NetFrpTab2Control _tab2 = new();
    private readonly NetFrpTab3Control _tab3 = new();
    private readonly NetFrpTab4Control _tab4 = new();
    private readonly NetFrpTab5Control _tab5 = new();

    public override string Title => App.Lang("NetFrpWindow.Title");

    public override string UseName { get; }

    public NetFrpControl()
    {
        UseName = ToString() ?? "NetFrpControl";
    }

    public override void Closed()
    {
        App.NetFrpWindow = null;
    }

    public override void Opened()
    {
        Window.SetTitle(Title);

        Content1.Child = _tab4;

        var model = (DataContext as NetFrpModel)!;
        model.Open();
    }

    protected override MenuModel SetModel(BaseModel model)
    {
        return new NetFrpModel(model);
    }

    protected override Control ViewChange(bool iswhell, int old, int index)
    {
        var model = (DataContext as NetFrpModel)!;
        switch (old)
        {
            case 0:
            case 3:
            case 4:
                model.RemoveClick();
                break;
        }
        switch (index)
        {
            case 0:
                model.LoadCloud();
                model.SetTab4Click();
                return _tab4;
            case 1:
                model.LoadSakura();
                return _tab1;
            case 2:
                model.LoadOpenFrp();
                return _tab5;
            case 3:
                model.LoadLocal();
                model.SetTab2Click();
                return _tab2;
            case 4:
                model.SetTab3Click();
                return _tab3;
            default:
                throw new InvalidEnumArgumentException();
        }
    }

    public override Task<bool> Closing()
    {
        return (DataContext as NetFrpModel)!.Closing();
    }
}
