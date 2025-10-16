using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Controls.Items;

/// <summary>
/// 映射子项目
/// </summary>
public partial class NetFrpRemoteControl : UserControl
{
    public NetFrpRemoteControl()
    {
        InitializeComponent();

        PointerPressed += NetFrpRemoteControl_PointerPressed;
    }

    private void NetFrpRemoteControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is NetFrpRemoteModel model)
        {
            model.Select();
        }
    }
}
