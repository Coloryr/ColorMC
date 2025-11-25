using Avalonia.Controls;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Windows;

public partial class MainView : Panel
{
    public MainView()
    {
        InitializeComponent();

        UIUtils.InitDialog(Dialog.DataTemplates);
    }
}