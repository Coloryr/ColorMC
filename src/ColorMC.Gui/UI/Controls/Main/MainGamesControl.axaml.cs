using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Flyouts;
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
        var temp = e.GetCurrentPoint(ScrollViewer1).Properties;
        if (temp.IsLeftButtonPressed)
        {
            if (DataContext is MainModel model)
            {
                model.EndMut();
                model.SearchClose();
                model.Select(obj: null);
            }
        }
        else if (temp.IsRightButtonPressed)
        {
            if (DataContext is MainModel model)
            {
                if (model.IsMut)
                {
                    Flyout1(this, model);
                }
                else
                {
                    Flyout(this, model);
                }
            }
        }
    }

    private void Flyout(Control con, MainModel model)
    {
        _ = new MainFlyout1(con, model);
    }

    private void Flyout1(Control con, MainModel model)
    {
        _ = new MainFlyout2(con, model);
    }
}
