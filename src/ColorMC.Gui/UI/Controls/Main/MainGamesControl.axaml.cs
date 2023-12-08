using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.Main;

namespace ColorMC.Gui.UI.Controls.Main;

public partial class MainGamesControl : UserControl
{
    public MainGamesControl()
    {
        InitializeComponent();

        ScrollViewer1.PointerPressed += ScrollViewer1_PointerPressed;
    }

    private void ScrollViewer1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(ScrollViewer1).Properties.IsLeftButtonPressed)
        {
            (DataContext as MainModel)!.Select(null);
        }
    }
}
