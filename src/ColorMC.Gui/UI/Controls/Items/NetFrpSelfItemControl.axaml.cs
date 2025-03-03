using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Controls.Items;

public partial class NetFrpSelfItemControl : UserControl
{
    public NetFrpSelfItemControl()
    {
        InitializeComponent();

        PointerPressed += FrpSelfItemControl_PointerPressed;
    }

    private void FrpSelfItemControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is NetFrpSelfItemModel model)
        {
            model.Select();
        }
    }
}