using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Flyouts;

namespace ColorMC.Gui.UI.Controls.GameEdit.Items;

public partial class ScreenshotControl : UserControl
{
    private Tab9Control Tab;

    public ScreenshotDisplayObj Obj { get; private set; }
    public ScreenshotControl()
    {
        InitializeComponent();

        PointerPressed += ScreenshotControl_PointerPressed;
    }

    private void ScreenshotControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Tab.SetSelect(this);

        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            _ = new GameEditFlyout4(Tab, Obj);
        }
    }

    public void Load(ScreenshotDisplayObj obj)
    {
        Obj = obj;

        Label1.Content = obj.Name;
        Image1.Source = obj.Image;
    }

    public void SetWindow(Tab9Control tab)
    {
        Tab = tab;
    }

    public void SetSelect(bool select)
    {
        Rectangle1.IsVisible = select;
    }
}
