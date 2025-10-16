using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Controls.Items;

/// <summary>
/// 映射子项目
/// </summary>
public partial class NetFrpLocalControl : UserControl
{
    public NetFrpLocalControl()
    {
        InitializeComponent();

        PointerPressed += NetFrpLocalControl_PointerPressed;
    }

    private void NetFrpLocalControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is NetFrpLocalModel model)
        {
            model.Select();
        }
    }
}
