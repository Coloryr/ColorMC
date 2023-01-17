using Avalonia.Controls;
using ColorMC.Gui.UI.Animations;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Controls.Setting;
using System;
using System.Threading;

namespace ColorMC.Gui.UI.Windows;

public partial class SettingWindow : Window, IBaseWindow
{
    private readonly Controls.Hello.Tab2Control tab1 = new();
    private readonly Tab2Control tab2 = new();
    private readonly Tab3Control tab3 = new();
    private readonly Tab4Control tab4 = new();
    private readonly Tab5Control tab5 = new();

    private bool switch1 = false;

    private readonly ContentControl content1 = new();
    private readonly ContentControl content2 = new();

    private readonly PageSlide slide = new(TimeSpan.FromMilliseconds(500));

    private int now;

    Info4Control IBaseWindow.Info => Info;
    Info1Control IBaseWindow.Info1 => Info1;
    Info2Control IBaseWindow.Info2 => Info2;
    Info3Control IBaseWindow.Info3 => Info3;
    public Window Window => this;

    public SettingWindow()
    {
        InitializeComponent();

        this.MakeItNoChrome();
        Rectangle1.MakeResizeDrag(this);

        Tabs.SelectionChanged += Tabs_SelectionChanged;
        Tab1.Children.Add(content1);
        Tab1.Children.Add(content2);

        tab1.SetWindow(this);
        tab2.SetWindow(this);
        tab3.SetWindow(this);
        tab4.SetWindow(this);
        tab5.SetWindow(this);

        content1.Content = tab1;

        Closed += SettingWindow_Closed;

        App.PicUpdate += Update;

        Update();
    }

    private void Tabs_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        switch (Tabs.SelectedIndex)
        {
            case 0:
                Go(tab1);
                break;
            case 1:
                tab2.Load();
                Go(tab2);
                break;
            case 2:
                tab3.Load();
                Go(tab3);
                break;
            case 3:
                tab4.Load();
                Go(tab4);
                break;
            case 4:
                tab5.Load();
                Go(tab5);
                break;
            case 5:
                //tab5.Load();
                //Go(tab6);
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

    private void SettingWindow_Closed(object? sender, EventArgs e)
    {
        App.PicUpdate -= Update;

        App.SettingWindow = null;
    }

    public void Update()
    {
        App.Update(this, Image_Back, Grid1);
    }

    public void Next()
    {

    }

    public void Tab5Load()
    {
        tab5.Load();
    }
}
