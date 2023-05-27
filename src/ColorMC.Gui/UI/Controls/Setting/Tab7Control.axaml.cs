using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core;
using ColorMC.Gui.UI.Model.Setting;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class Tab7Control : UserControl
{
    private readonly SettingTab7Model model;
    public Tab7Control()
    {
        InitializeComponent();

        model = new();
        DataContext = model;
    }
}
