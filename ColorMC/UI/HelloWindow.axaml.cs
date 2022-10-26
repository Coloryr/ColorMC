using Avalonia.Controls;
using ColorMC.UI.Animations;
using ColorMC.UI.Views.Hello;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.UI;

public partial class HelloWindow : Window
{
    private Tab1Control tab1 = new();
    private Tab2Control tab2 = new();
    private Tab3Control tab3 = new();
    private Tab4Control tab4 = new();
    private Tab5Control tab5 = new();

    private bool switch1 = false;

    private ContentControl content1 = new();
    private ContentControl content2 = new();

    private PageSlide slide = new(TimeSpan.FromMilliseconds(500));

    private int now;
    public HelloWindow()
    {
        InitializeComponent();

        FontFamily = Program.FontFamily;

        //Opened += HelloWindow_Opened;
        Tabs.SelectionChanged += Tabs_SelectionChanged;
        Tab1.Children.Add(content1);
        Tab1.Children.Add(content2);

        content1.Content = tab1;
        slide.Start(content2, content1, false, CancellationToken.None);
    }

    private async void Go(UserControl to) 
    {
        Tabs.IsEnabled = false;
        
        if (!switch1)
        {
            content2.Content = to;
            await slide.Start(content1, content2, now < Tabs.SelectedIndex, CancellationToken.None);
        }
        else
        {
            content1.Content = to;
            await slide.Start(content2, content1, now < Tabs.SelectedIndex, CancellationToken.None);
        }

        switch1 = !switch1;
        Tabs.IsEnabled = true;

    }

    private void Tabs_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        switch (Tabs.SelectedIndex)
        {
            case 0:
                Go(tab1);
                break;
            case 1:
                Go(tab2);
                break;
            case 2:
                Go(tab3);
                break;
            case 3:
                Go(tab4);
                break;
            case 4:
                Go(tab5);
                break;
        }

        now = Tabs.SelectedIndex;
    }

    private void HelloWindow_Opened(object? sender, System.EventArgs e)
    {
        Info.Show($"检测你是第一次启动{Environment.NewLine}是否需要进行初始化设置", (res) => 
        {
            if (!res)
                Close();
        });
    }
    public void Set() 
    {
        ShowDialog(MainWindow.Window);
    }
}
