using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Model.GameEdit;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab5Control : UserControl
{
    public Tab5Control()
    {
        InitializeComponent();

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);
    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is GameEditModel model && model.NowView == 3)
        {
            if (e.Delta.Y < 0)
            {
                model.NowView++;
            }
            else if (e.Delta.Y > 0)
            {
                model.NowView--;
            }
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ScrollViewer1.PointerWheelChanged -= ScrollViewer1_PointerWheelChanged;
    }

    private void DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Files))
        {
            Grid2.IsVisible = true;
        }
    }

    private void DragLeave(object? sender, DragEventArgs e)
    {
        Grid2.IsVisible = false;
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        Grid2.IsVisible = false;
        (DataContext as GameEditModel)?.DropWorld(e.Data);
    }
}
