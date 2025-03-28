using System;
using Avalonia.Controls;
using ColorMC.Gui.UI.Controls;

namespace ColorMC.Gui.UI.Windows;

/// <summary>
/// ������
/// </summary>
public partial class SingleWindow : ABaseWindow
{
    public override IBaseControl ICon => Win;

    public override int DefaultWidth => 760;
    public override int DefaultHeight => 450;

    public SingleWindow()
    {
        InitializeComponent();

        Closed += UserWindow_Closed;
        Closing += SingleWindow_Closing;

        DataContext = Win.DataContext;

        InitBaseWindow();
        SetWindowState();
    }

    private async void SingleWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        var res = await Win.Closing();
        if (res)
        {
            e.Cancel = true;
        }
    }

    private void UserWindow_Closed(object? sender, EventArgs e)
    {
        ColorMCGui.Exit();
    }
}
