using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.GameExport;

public partial class Tab2Control : UserControl
{
    public Tab2Control()
    {
        InitializeComponent();
    }

    public void Opened()
    {
        DataGrid1.SetFontColor();
    }
}