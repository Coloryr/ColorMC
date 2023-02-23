using Avalonia.Controls;
using ColorMC.Core;
using ColorMC.Gui.UI.Controls.Hello;
using System;
using System.Threading;

namespace ColorMC.Gui.UI.Windows;

public partial class HelloWindow : Window
{
    private readonly Tab1Control tab1 = new();
    private readonly Tab2Control tab2 = new();
    private readonly Tab3Control tab3 = new();
    private readonly Tab4Control tab4 = new();
    private readonly Tab5Control tab5 = new();
    private readonly Tab6Control tab6 = new();

    private bool switch1 = false;

    private readonly ContentControl content1 = new();
    private readonly ContentControl content2 = new();

    private int now;
    private CancellationTokenSource cancel = new();

    public HelloWindow()
    {
        InitializeComponent();

        this.Init();
        Icon = App.Icon;
        Border1.MakeResizeDrag(this);

        Tabs.SelectionChanged += Tabs_SelectionChanged;
        Tab1.Children.Add(content1);
        Tab1.Children.Add(content2);

        content1.Content = tab1;

        Closed += HelloWindow_Closed;

        App.PicUpdate += Update;

        Update();
    }

    private void HelloWindow_Closed(object? sender, EventArgs e)
    {
        CoreMain.PackState = null;
        CoreMain.PackUpdate = null;
        CoreMain.GameOverwirte = null;

        App.PicUpdate -= Update;
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
            App.PageSlide500.Start(content1, content2, now < Tabs.SelectedIndex, cancel.Token);
        }
        else
        {
            content1.Content = to;
            App.PageSlide500.Start(content2, content1, now < Tabs.SelectedIndex, cancel.Token);
        }

        switch1 = !switch1;
        Tabs.IsEnabled = true;
    }

    public void SwitchTab(int index)
    {
        Tabs.SelectedIndex = index;
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
                tab6.Load();
                Go(tab6);
                break;
        }

        now = Tabs.SelectedIndex;
    }

    public void Load()
    {
        tab3.Load();
        tab4.Load();
    }

    public void Next()
    {
        Tabs.SelectedIndex++;
    }

    public void Done()
    {
        Close();
    }

    public void Update()
    {
        App.Update(this, Image_Back, Border1, Border2);
    }
}
