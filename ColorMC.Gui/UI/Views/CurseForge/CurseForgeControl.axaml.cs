using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core.Net;
using ColorMC.Core.Objs.CurseForge;
using System;

namespace ColorMC.Gui.UI.Views.CurseForge;

public partial class CurseForgeControl : UserControl
{
    private AddCurseForgeWindow Window;
    public CurseForgeObj.Data Data { get; private set; }
    public CurseForgeControl()
    {
        InitializeComponent();

        PointerPressed += CurseForgeControl_PointerPressed;
        DoubleTapped += CurseForgeControl_DoubleTapped;
    }

    private void CurseForgeControl_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        Window.Install(Data);
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
        if (data.logo != null)
        {
            var data1 = await BaseClient.Client.GetAsync(data.logo.url);
            Dispatcher.UIThread.Post(() =>
            {
                var bitmap = new Bitmap(data1.Content.ReadAsStream());
                Image_Logo.Source = bitmap;
            });
        }
    }

    public void SetWindow(AddCurseForgeWindow window)
    {
        Window = window;
    }
}
