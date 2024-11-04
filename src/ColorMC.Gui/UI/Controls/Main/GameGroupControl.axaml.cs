using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Main;

namespace ColorMC.Gui.UI.Controls.Main;

public partial class GameGroupControl : UserControl
{
    public GameGroupControl()
    {
        InitializeComponent();

        Expander_Head.ContentTransition = ThemeManager.CrossFade300;

        PointerEntered += GameGroupControl_PointerEntered;
        PointerExited += GameGroupControl_PointerExited;

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);
    }

    private void GameGroupControl_PointerExited(object? sender, PointerEventArgs e)
    {
        Button1.IsVisible = false;
    }

    private void GameGroupControl_PointerEntered(object? sender, PointerEventArgs e)
    {
        Button1.IsVisible = true;
    }

    private void DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Source is Control && DataContext is GameGroupModel model)
        {
            Grid1.IsVisible = model.DropIn(e.Data);
        }
    }

    private void DragLeave(object? sender, DragEventArgs e)
    {
        Grid1.IsVisible = false;
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        Grid1.IsVisible = false;
        if (e.Source is Control && DataContext is GameGroupModel model)
        {
            model.Drop(e.Data);
        }
    }
}
