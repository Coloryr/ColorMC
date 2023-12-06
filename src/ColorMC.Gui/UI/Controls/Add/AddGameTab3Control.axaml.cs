using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class AddGameTab3Control : UserControl
{
    public AddGameTab3Control()
    {
        InitializeComponent();

        if (SystemInfo.Os == OsType.Android)
        {
            IsEnabled = false;
        }
    }
}