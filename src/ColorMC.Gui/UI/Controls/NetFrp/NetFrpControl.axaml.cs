using Avalonia.Controls;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.NetFrp;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.NetFrp;

public partial class NetFrpControl : MenuControl
{
    private readonly NetFrpTab1Control _tab1 = new();
    private readonly NetFrpTab2Control _tab2 = new();
    private readonly NetFrpTab3Control _tab3 = new();
    private readonly NetFrpTab4Control _tab4 = new();

    public override string Title => App.Lang("NetFrpWindow.Ttile");

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

    public void SetProcess(Process process, NetFrpLocalModel model1, string ip)
    {
        var model = (DataContext as NetFrpModel)!;
        model.SetProcess(process, model1, ip);
        model.NowView = 3;
    }

    protected override Control ViewChange(bool iswhell, int old, int index)
    {
        var model = (DataContext as NetFrpModel)!;
        switch (index)
        {
            case 0:
                model.LoadCloud();
                return _tab4;
            case 1:
                model.Load();
                return _tab1;
            case 2:
                model.LoadLocal();
                return _tab2;
            case 3:
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
