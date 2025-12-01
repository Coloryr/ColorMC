using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.Add;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class AddFileInfoControl : UserControl
{
    public AddFileInfoControl()
    {
        InitializeComponent();

        ModPackFiles.PointerPressed += ModPackFiles_PointerPressed;
    }

    private void ModPackFiles_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var ev = e.GetCurrentPoint(this);
        if (ev.Properties.IsXButton1Pressed)
        {
            (DataContext as AddBaseModel)?.Download();
            e.Handled = true;
        }
    }
}