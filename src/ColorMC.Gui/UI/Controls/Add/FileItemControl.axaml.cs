using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class FileItemControl : UserControl
{
    public static readonly StyledProperty<Task<Bitmap?>> ImageProperty =
           AvaloniaProperty.Register<Image, Task<Bitmap?>>(nameof(Image), defaultBindingMode: BindingMode.TwoWay);

    private CancellationTokenSource cancel = new();
    private Task<Bitmap?> Image
    {
        get { return GetValue(ImageProperty); }
        set { SetValue(ImageProperty, value); }
    }
    public FileItemDisplayObj Data { get; private set; }
    public FileItemControl()
    {
        InitializeComponent();

        DataContext = this;

        PointerPressed += CurseForgeControl_PointerPressed;
        DoubleTapped += CurseForgeControl_DoubleTapped;

        Image1.Source = App.GameIcon;
    }

    private void CurseForgeControl_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        (window.Con as IAddWindow)?.Install();
    }

    private void CurseForgeControl_PointerPressed(object? sender,
        PointerPressedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        (window.Con as IAddWindow)?.SetSelect(this);

        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            var url = Data.GetUrl();
            if (url == null)
                return;

            new UrlFlyout(url).ShowAt(this, true);
        }
    }

    public void SetSelect(bool select)
    {
        Rectangle1.IsVisible = select;
    }

    public void SetNowDownload()
    {
        Grid1.IsVisible = true;
        Label5.IsVisible = false;
    }

    public void SetDownloaded()
    {
        Grid1.IsVisible = false;
        Dispatcher.UIThread.Post(() =>
        {
            Label5.IsVisible = true;
        });
    }

    public void Cancel()
    {
        cancel.Cancel();
    }

    private Task<Bitmap?> GetImage()
    {
        return Task.Run(() =>
        {
            if (Data == null || Data.Logo == null)
                return null;
            try
            {
                var image = ImageTemp.Load(Data.Logo, cancel.Token);
                if (cancel.IsCancellationRequested)
                {
                    image?.Dispose();
                    return null;
                }
                return image;
            }
            catch (Exception e)
            {
                if (cancel.IsCancellationRequested)
                    return null;

                Logs.Error(App.GetLanguage("AddModPackWindow.Error5"), e);
            }

            return null;
        });
    }

    public void Load(FileItemDisplayObj data)
    {
        Cancel();

        cancel.Dispose();
        cancel = new();

        Data = data;

        Label5.IsVisible = data.IsDownload;

        Label1.Content = data.Name;
        TextBlock1.Text = data.Summary;
        Label2.Content = data.Author;
        Label3.Content = data.DownloadCount;
        Label4.Content = DateTime.Parse(data.ModifiedDate);

        Image1.Source = App.GameIcon;
        Image = GetImage();
    }
}
