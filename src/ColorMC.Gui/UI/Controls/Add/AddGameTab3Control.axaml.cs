using Avalonia.Controls;

namespace ColorMC.Gui.UI.Controls.Add;

/// <summary>
/// �����Ϸʵ������
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