using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class FileItemControl : UserControl, IDisposable
{
    private Task<Bitmap?> Image => GetImage();
    public FileItemDisplayObj? Data { get; init; }

    public FileItemControl(FileItemDisplayObj? data)
    {
        InitializeComponent();

        Data = data;
        Image1.Source = App.GameIcon;

        DataContext = this;

        PointerPressed += CurseForgeControl_PointerPressed;
        DoubleTapped += CurseForgeControl_DoubleTapped;

        Load();
    }

    public FileItemControl() : this(null)
    {

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
            var url = Data?.GetUrl();
            if (url == null)
                return;

            _ = new UrlFlyout(this, url);
        }
    }

    public void SetSelect(bool select)
    {
        Rectangle1.IsVisible = select;
    }

    public void SetNoDownloadNow()
    {
        Grid1.IsVisible = false;
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

    private async Task<Bitmap?> GetImage()
    {
        if (Data == null || Data.Logo == null)
            return null;
        try
        {
            return await ImageTemp.Load(Data.Logo);
        }
        catch (Exception e)
        {
            Logs.Error(App.GetLanguage("AddModPackWindow.Error5"), e);
        }

        return null;
    }

    private void Load()
    {
        if (Data == null)
            return;

        Label5.IsVisible = Data.IsDownload;
        Label4.Content = DateTime.Parse(Data.ModifiedDate);
    }

    public void Dispose()
    {
        if (Image1.Source != null && Image1.Source != App.GameIcon)
        {
            (Image1.Source as Bitmap)?.Dispose();
        }
    }
}
