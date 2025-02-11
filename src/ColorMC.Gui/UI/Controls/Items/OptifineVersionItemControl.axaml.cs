using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Gui.UI.Animations;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.Items;

/// <summary>
/// Optifine光影文件版本子项目
/// </summary>
public partial class OptifineVersionItemControl : UserControl
{
    public OptifineVersionItemControl()
    {
        InitializeComponent();

        PointerPressed += FileItemControl_PointerPressed;
        PointerReleased += FileItemControl_PointerReleased;
        DoubleTapped += FileItemControl_DoubleTapped;
        PointerEntered += FileItemControl_PointerEntered;
        PointerExited += FileItemControl_PointerExited;
        PointerMoved += FileItemControl_PointerMoved;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (GuiConfigUtils.Config.Style.EnableAm)
        {
            Dispatcher.UIThread.Post(() =>
            {
                ItemAnimation.Make().RunAsync(this);
            });
        }
    }

    private void FileItemControl_PointerMoved(object? sender, PointerEventArgs e)
    {
        LongPressed.Cancel();
    }

    private void FileItemControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        LongPressed.Released();
    }

    private void FileItemControl_PointerExited(object? sender, PointerEventArgs e)
    {
        if (DataContext is OptifineVersionItemModel model)
        {
            model.Top = false;
        }
    }

    private void FileItemControl_PointerEntered(object? sender, PointerEventArgs e)
    {
        if (DataContext is OptifineVersionItemModel model)
        {
            model.Top = true;
        }
    }

    private void FileItemControl_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        (DataContext as OptifineVersionItemModel)?.Install();
    }

    private void FileItemControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not OptifineVersionItemModel model)
        {
            return;
        }
        model.SetSelect();

        //var ev = e.GetCurrentPoint(this);
        //if (ev.Properties.IsXButton1Pressed)
        //{
        //    model.Back();
        //    e.Handled = true;
        //}
        //else if (ev.Properties.IsXButton2Pressed)
        //{
        //    model.Next();
        //    e.Handled = true;
        //}
    }
}