using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Net;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.Utils.LaunchSetting;
using System;

namespace ColorMC.Gui.UI.Controls.CurseForge;

public partial class CurseForgeControl : UserControl
{
    private IBase1Window Window;
    private bool haveimg;
    public CurseForgeObj.Data Data { get; private set; }
    public CurseForgeControl()
    {
        InitializeComponent();

        PointerPressed += CurseForgeControl_PointerPressed;
        DoubleTapped += CurseForgeControl_DoubleTapped;
    }

    private void CurseForgeControl_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        Window.Install();
    }

    private void CurseForgeControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Window.SetSelect(this);
    }

    public void SetSelect(bool select)
    {
        Rectangle1.IsVisible = select;
    }

    public async void Load(CurseForgeObj.Data data)
    {
        Data = data;

        Label1.Content = data.name;
        TextBlock1.Text = data.summary;
        string temp = "";
        foreach (var item in data.authors)
        {
            temp += item.name + " ";
        }
        Label2.Content = temp;
        Label3.Content = data.downloadCount;
        Label4.Content = DateTime.Parse(data.dateModified);
        haveimg = false;
        if (data.logo != null)
        {
            try
            {
                using var data1 = await BaseClient.DownloadClient.GetAsync(data.logo.url);
                var bitmap = new Bitmap(data1.Content.ReadAsStream());
                Image_Logo.Source = bitmap;
                haveimg = true;
            }
            catch (Exception e)
            {
                Logs.Error(Localizer.Instance["AddCurseForgeWindow.Error5"], e);
            }
        }
        if (!haveimg)
        {
            Image_Logo.Source = App.GameIcon;
        }
    }

    public void SetWindow(IBase1Window window)
    {
        Window = window;
    }
}
