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

namespace ColorMC.Gui.UI.Controls.Add;

public partial class FileItemControl : UserControl
{
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

    public async void Load(FileItemDisplayObj data)
    {
        Data = data;

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
                using var data1 = await BaseClient.DownloadClient.GetAsync(data.Logo);
                var bitmap = new Bitmap(data1.Content.ReadAsStream());
                Image_Logo.Source = bitmap;
            }
            catch (Exception e)
            {
                Logs.Error(App.GetLanguage("AddModPackWindow.Error5"), e);
            }
        }
    }
}
