using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using System;
using System.IO;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class FileItemControl : UserControl
{
    private CancellationTokenSource cancel = new();
    public FileItemDisplayObj Data { get; private set; }
    public FileItemControl()
    {
        InitializeComponent();

        PointerPressed += CurseForgeControl_PointerPressed;
        DoubleTapped += CurseForgeControl_DoubleTapped;
        PointerPressed += FileItemControl_PointerPressed;

        Image_Logo.Source = App.GameIcon;
    }

    private void FileItemControl_PointerPressed(object? sender,
        PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            if (Data.SourceType == SourceType.CurseForge)
            {
                new UrlFlyout((Data.Data as CurseForgeObj.Data)!.links.websiteUrl).ShowAt(this, true);
            }
            else if (Data.SourceType == SourceType.Modrinth)
            {
                var obj = (Data.Data as ModrinthSearchObj.Hit)!;
                new UrlFlyout(Data.FileType switch
                {
                    FileType.ModPack => "https://modrinth.com/modpack/",
                    FileType.Shaderpack => "https://modrinth.com/shaders/",
                    FileType.Resourcepack => "https://modrinth.com/resourcepacks/",
                    FileType.DataPacks => "https://modrinth.com/datapacks/",
                    _ => "https://modrinth.com/mod/"
                } + obj.project_id).ShowAt(this, true);
            }
        }
    }

    private void CurseForgeControl_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        var window = (VisualRoot as IAddWindow)!;
        window.Install();
    }

    private void CurseForgeControl_PointerPressed(object? sender,
        PointerPressedEventArgs e)
    {
        var window = (VisualRoot as IAddWindow)!;
        window.SetSelect(this);
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

    public void Load(FileItemDisplayObj data)
    {
        cancel.Dispose();
        cancel = new();

        Data = data;

        Label5.IsVisible = data.IsDownload;

        Label1.Content = data.Name;
        TextBlock1.Text = data.Summary;
        Label2.Content = data.Author;
        Label3.Content = data.DownloadCount;
        Label4.Content = DateTime.Parse(data.ModifiedDate);

        if (Image_Logo.Source != App.GameIcon)
        {
            (Image_Logo.Source as Bitmap)?.Dispose();

            Image_Logo.Source = App.GameIcon;
        }

        if (data.Logo != null)
        {
            try
            {
                BaseClient.Poll(data.Logo, (data1) =>
                {
                    if (cancel.IsCancellationRequested)
                        return;

                    var bitmap = new Bitmap(data1);
                    Dispatcher.UIThread.Post(() =>
                    {
                        Image_Logo.Source = bitmap;
                    });
                }, cancel.Token);
            }
            catch (Exception e)
            {
                if (cancel.IsCancellationRequested)
                    return;

                Logs.Error(App.GetLanguage("AddModPackWindow.Error5"), e);
            }
        }
    }

    public void Close()
    {
        cancel.Cancel();

        if (Image_Logo.Source != App.GameIcon)
        {
            (Image_Logo.Source as Bitmap)?.Dispose();

            Image_Logo.Source = App.GameIcon;
        }

        cancel.Dispose();
    }
}
