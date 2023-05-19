using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Add;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System;

namespace ColorMC.Gui.UI.Controls.Add.Items;

public partial class FileItemControl : UserControl, IDisposable
{
    public static readonly StyledProperty<FileItemModel> FileItemModelProperty =
        AvaloniaProperty.Register<FileItemControl, FileItemModel>(nameof(FileItemModel));

    public FileItemModel FileItemModel
    {
        get => GetValue(FileItemModelProperty);
        set => SetValue(FileItemModelProperty, value);
    }

    public FileItemControl()
    {
        InitializeComponent();

        PointerPressed += CurseForgeControl_PointerPressed;
        DoubleTapped += CurseForgeControl_DoubleTapped;
        PointerEntered += FileItemControl_PointerEntered;
        PointerExited += FileItemControl_PointerExited;

        PropertyChanged += FileItemControl_PropertyChanged;
    }

    private void FileItemControl_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == FileItemModelProperty)
        {
            if (FileItemModel == null)
                return;

            DataContext = FileItemModel;
        }
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
        (window.Con as IAddWindow)?.Install(FileItemModel);
    }

    private void CurseForgeControl_PointerPressed(object? sender,
        PointerPressedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        (window.Con as IAddWindow)?.SetSelect(FileItemModel);

        var ev = e.GetCurrentPoint(this);
        if (ev.Properties.IsRightButtonPressed)
        {
            var url = FileItemModel.Data?.GetUrl();
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
