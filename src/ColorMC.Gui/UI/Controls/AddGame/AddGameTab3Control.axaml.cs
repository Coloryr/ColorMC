using Avalonia.Controls;

namespace ColorMC.Gui.UI.Controls.AddGame;

/// <summary>
/// 添加游戏实例窗口
/// </summary>
public partial class AddGameTab3Control : UserControl
{
    public AddGameTab3Control()
    {
        InitializeComponent();
#if Phone
        IsEnabled = false;
#endif
    }
}