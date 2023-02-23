using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.UI.Controls.Setting;
using System;
using System.Threading;

namespace ColorMC.Gui.UI.Windows;

public partial class SettingWindow : Window
{
    private readonly Tab1Control tab1 = new();
    private readonly Tab2Control tab2 = new();
    private readonly Tab3Control tab3 = new();
    private readonly Tab4Control tab4 = new();
    private readonly Tab5Control tab5 = new();
    private readonly Tab6Control tab6 = new();
    private readonly Tab7Control tab7 = new();

    private bool switch1 = false;

    private readonly ContentControl content1 = new();
    private readonly ContentControl content2 = new();

    private CancellationTokenSource cancel = new();

    private int now;

    public SettingWindow()
    {
        InitializeComponent();

        this.Init();
        Icon = App.Icon;
        Border1.MakeResizeDrag(this);

        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;

        Tabs.SelectionChanged += Tabs_SelectionChanged;
        Tab1.Children.Add(content1);
        Tab1.Children.Add(content2);

        content1.Content = tab2;

        tab2.Load();

        Closed += SettingWindow_Closed;
        Activated += SettingWindow_Activated;

        App.PicUpdate += Update;

        Update();
    }

    private void SettingWindow_Activated(object? sender, EventArgs e)
    {
        App.LastWindow = this;
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
            case 6:
                Go(tab7);
                break;
        }

        now = Tabs.SelectedIndex;
    }

    private void Go(UserControl to)
    {
        cancel.Cancel();
        cancel.Dispose();

        cancel = new();

        Tabs.IsEnabled = false;

        if (!switch1)
        {
            content2.Content = to;
            App.PageSlide500.Start(content1, content2, now < Tabs.SelectedIndex,
                cancel.Token);
        }
        else
        {
            content1.Content = to;
            App.PageSlide500.Start(content2, content1, now < Tabs.SelectedIndex,
                cancel.Token);
        }

        switch1 = !switch1;
        Tabs.IsEnabled = true;
    }

    private void SettingWindow_Closed(object? sender, EventArgs e)
    {
        content1.Content = null;
        content2.Content = null;

        App.PicUpdate -= Update;

        App.SettingWindow = null;
    }

    public void Update()
    {
        App.Update(this, Image_Back, Border1, Border2);
    }

    public void Tab5Load()
    {
        tab5.Load();
    }
}
