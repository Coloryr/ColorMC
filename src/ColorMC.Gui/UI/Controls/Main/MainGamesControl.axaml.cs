using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Gui.UI.Model.Main;

namespace ColorMC.Gui.UI.Controls.Main;

public partial class MainGamesControl : UserControl
{
    public MainGamesControl()
    {
        InitializeComponent();

        ScrollViewer1.PointerPressed += ScrollViewer1_PointerPressed;
        Search.LostFocus += Search_LostFocus;
    }

    private void Search_LostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainModel model)
        {
            model.SearchClose();
        }
    }

    private void ScrollViewer1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(ScrollViewer1).Properties.IsLeftButtonPressed)
        {
            if (DataContext is MainModel model)
            {
                model.SearchClose();
                model.Select(null);
            }

        }
    }
}
