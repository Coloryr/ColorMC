using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.GameEdit.Items;

public partial class WorldControl : UserControl
{
    public WorldDisplayObj World { get; private set; }
    public WorldControl()
    {
        InitializeComponent();

        PointerPressed += WorldControl_PointerPressed;
    }

    private void WorldControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var tab = this.FindTop<Tab5Control>()!;
        tab.SetSelect(this);

        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            _ = new GameEditFlyout2(tab, World);
        }
    }

    public void Load(WorldDisplayObj world)
    {
        World = world;

        Label1.Text = world.Name;
        Label2.Text = world.Mode;
        Label3.Text = world.Time;
        Label4.Text = world.Local;
        Label5.Text = world.Difficulty;
        Label6.Text = world.Hardcore.ToString();

        if (world.Pic != null)
        {
            Image1.Source = world.Pic;
        }
    }

    public void SetSelect(bool select)
    {
        Rectangle1.IsVisible = select;
    }
}
