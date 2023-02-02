using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Controls.Setting;
using System;
using System.Threading;

namespace ColorMC.Gui.UI.Windows;

public partial class SettingWindow : Window
{
    private Tab1Control tab1 = new();
    private Tab2Control tab2 = new();
    private Tab3Control tab3 = new();
    private Tab4Control tab4 = new();
    private Tab5Control tab5 = new();
    private Tab6Control tab6 = new();

    private bool switch1 = false;

    private ContentControl content1 = new();
    private ContentControl content2 = new();

    private int now;

    public SettingWindow()
    {
        InitializeComponent();

        Head.SetWindow(this);

        FontFamily = Program.Font;

        Icon = App.Icon;

        Rectangle1.MakeResizeDrag(this);

        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;

        Tabs.SelectionChanged += Tabs_SelectionChanged;
        Tab1.Children.Add(content1);
        Tab1.Children.Add(content2);

        tab1.SetWindow(this);
        tab2.SetWindow(this);
        tab3.SetWindow(this);
        tab4.SetWindow(this);
        tab5.SetWindow(this);
        tab6.SetWindow(this);

        content1.Content = tab2;

        Closed += SettingWindow_Closed;

        App.PicUpdate += Update;

        Update();
    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (e.Delta.Y > 0)
        {
            ScrollViewer1.LineLeft();
        }
        else if (e.Delta.Y < 0)
        {
            ScrollViewer1.LineRight();
        }
    }

    private void Tabs_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        switch (Tabs.SelectedIndex)
        {
            case 0:
                tab2.Load();
                Go(tab2);
                break;
            case 1:
                tab3.Load();
                Go(tab3);
                break;
            case 2:
                tab4.Load();
                Go(tab4);
                break;
            case 3:
                tab5.Load();
                Go(tab5);
                break;
            case 4:
                tab6.Load();
                Go(tab6);
                break;
            case 5:
                Go(tab1);
                break;
        }

        now = Tabs.SelectedIndex;
    }

    private async void Go(UserControl to)
    {
        Tabs.IsEnabled = false;

        if (!switch1)
        {
            content2.Content = to;
            await App.PageSlide500.Start(content1, content2, now < Tabs.SelectedIndex, CancellationToken.None);
        }
        else
        {
            content1.Content = to;
            await App.PageSlide500.Start(content2, content1, now < Tabs.SelectedIndex, CancellationToken.None);
        }

        switch1 = !switch1;
        Tabs.IsEnabled = true;
    }

    private void SettingWindow_Closed(object? sender, EventArgs e)
    {
        App.PicUpdate -= Update;

        App.SettingWindow = null;
    }

    public void Update()
    {
        App.Update(this, Image_Back, Grid1);
    }

    public void Tab5Load()
    {
        tab5.Load();
    }
}
