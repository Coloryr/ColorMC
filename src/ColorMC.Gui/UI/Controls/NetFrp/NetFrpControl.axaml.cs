using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.NetFrp;

namespace ColorMC.Gui.UI.Controls.NetFrp;

/// <summary>
/// Ó³ÉäÁª»ú´°¿Ú
/// </summary>
public partial class NetFrpControl : MenuControl
{
    private NetFrpTab1Control _tab1;
    private NetFrpTab2Control _tab2;
    private NetFrpTab3Control _tab3;
    private NetFrpTab4Control _tab4;
    private NetFrpTab5Control _tab5;

    public NetFrpControl() : base(WindowManager.GetUseName<NetFrpControl>())
    {
        Title = App.Lang("NetFrpWindow.Title");
    }

    public override void Closed()
    {
        WindowManager.NetFrpWindow = null;

        var model = (DataContext as NetFrpModel)!;
        model.RemoveClick();
    }

    protected override async void Opened()
    {
        var model = (DataContext as NetFrpModel)!;
        if (await model.Open())
        {
            model.NowView = 0;
        }
    }

    protected override TopModel GenModel(BaseModel model)
    {
        return new NetFrpModel(model);
    }

    protected override Control ViewChange(int old, int index)
    {
        var model = (DataContext as NetFrpModel)!;
        switch (old)
        {
            case 4:
                model.RemoveClick();
                break;
        }
        switch (index)
        {
            case 0:
                model.LoadCloud();
                return _tab4 ??= new();
            case 1:
                model.LoadSakura();
                return _tab1 ??= new();
            case 2:
                model.LoadOpenFrp();
                return _tab5 ??= new();
            case 3:
                model.LoadLocal();
                return _tab2 ??= new();
            case 4:
                model.SetTab3Click();
                return _tab3 ??= new();
            default:
                throw new InvalidEnumArgumentException();
        }
    }

    public override Task<bool> Closing()
    {
        return (DataContext as NetFrpModel)!.Closing();
    }

    public override Bitmap GetIcon()
    {
        return ImageManager.GameIcon;
    }
}
