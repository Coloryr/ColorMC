using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Gui.UI.Controls.Add;

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