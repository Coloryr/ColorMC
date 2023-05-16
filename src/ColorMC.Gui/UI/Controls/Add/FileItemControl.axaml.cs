using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Add;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using System;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class FileItemControl : UserControl, IDisposable
{
    private FileItemModel model;

    public FileItemControl() : this(null)
    {

    }

    public FileItemControl(FileItemDisplayObj? data)
    {
        InitializeComponent();

        model = new(data);
        DataContext = model;

        PointerPressed += CurseForgeControl_PointerPressed;
        DoubleTapped += CurseForgeControl_DoubleTapped;
        PointerEntered += FileItemControl_PointerEntered;
        PointerExited += FileItemControl_PointerExited;
    }

    private void FileItemControl_PointerExited(object? sender, PointerEventArgs e)
    {
        Rectangle2.IsVisible = false;
    }

    private void FileItemControl_PointerEntered(object? sender, PointerEventArgs e)
    {
        Rectangle2.IsVisible = true;
    }


    private void CurseForgeControl_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        (window.Con as IAddWindow)?.Install(model);
    }

    private void CurseForgeControl_PointerPressed(object? sender,
        PointerPressedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        (window.Con as IAddWindow)?.SetSelect(model);

        var ev = e.GetCurrentPoint(this);
        if (ev.Properties.IsRightButtonPressed)
        {
            var url = model.Data?.GetUrl();
            if (url == null)
                return;

            _ = new UrlFlyout(this, url);
            e.Handled = true;
        }
        else if (ev.Properties.IsXButton1Pressed)
        {
            (window.Con as IAddWindow)?.Back();
            e.Handled = true;
        }
        else if (ev.Properties.IsXButton2Pressed)
        {
            (window.Con as IAddWindow)?.Next();
            e.Handled = true;
        }
    }

    public void Dispose()
    {
        if (Image1.Source != null && Image1.Source != App.GameIcon)
        {
            (Image1.Source as Bitmap)?.Dispose();
        }
    }
}
