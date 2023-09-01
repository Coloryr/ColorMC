using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Gui.UI.Controls.Add.AddGame;

public partial class Tab3Control : UserControl
{
    public Tab3Control()
    {
        InitializeComponent();

        if (SystemInfo.Os == OsType.Android)
        {
            IsEnabled = false;
        }
    }
}