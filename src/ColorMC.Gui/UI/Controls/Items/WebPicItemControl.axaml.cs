using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Controls.Items;

public partial class WebPicItemControl : UserControl
{
    public WebPicItemControl()
    {
        InitializeComponent();

        DoubleTapped += WebPicItemControl_DoubleTapped;
    }

    private void WebPicItemControl_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is WebPicModel model)
        {
            model.Open();
        }
    }
}