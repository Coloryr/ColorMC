using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.Objs;

namespace ColorMC.Gui.UI.Controls.GameEdit.Items;

public partial class ResourcePackControl : UserControl
{
    private Tab8Control Tab;
    public ResourcepackDisplayObj Pack { get; private set; }
    public ResourcePackControl()
    {
        InitializeComponent();

        PointerPressed += WorldControl_PointerPressed;
    }

    private void WorldControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Tab.SetSelect(this);

        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            new GameEditFlyout3(Tab, Pack).ShowAt(this, true);
        }
    }

    public void Load(ResourcepackDisplayObj pack)
    {
        Pack = pack;

        Label1.Text = pack.Local;
        Label2.Text = pack.PackFormat.ToString();
        Label3.Text = pack.Description;
        Label4.Text = pack.Pack.Broken ?
            App.GetLanguage("GameEditWindow.Tab8.Info6") : "";

        if (pack.Icon != null)
        {
            Image1.Source = pack.Icon;
        }
    }

    public void SetWindow(Tab8Control tab)
    {
        Tab = tab;
    }

    public void SetSelect(bool select)
    {
        Rectangle1.IsVisible = select;
    }
}
