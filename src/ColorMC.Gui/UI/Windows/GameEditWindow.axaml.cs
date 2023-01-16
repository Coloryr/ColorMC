using Avalonia.Animation;
using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Controls.GameEdit;
using ColorMC.Gui.Utils.LaunchSetting;
using System;
using System.Threading;

namespace ColorMC.Gui.UI.Windows;

public partial class GameEditWindow : Window
{
    private bool switch1 = false;

    private readonly Tab1Control tab1 = new();
    private readonly Tab2Control tab2 = new();
    private readonly Tab3Control tab3 = new();
    private readonly Tab4Control tab4 = new();
    private readonly Tab5Control tab5 = new();

    private readonly ContentControl content1 = new();
    private readonly ContentControl content2 = new();

    private readonly PageSlide slide = new(TimeSpan.FromMilliseconds(500));

    private int now;

    private GameSettingObj Obj;

    public GameEditWindow()
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
        Opened += GameEditWindow_Opened;

        Update();
    }

    private void GameEditWindow_Opened(object? sender, EventArgs e)
    {
        tab1.Update();
    }

    public void SetType(int type)
    {
        switch (type)
        {
            //查看Mod
            case 1:
                Tabs.SelectedIndex = 3;
                break;
            //查看配置文件
            case 2:
                Tabs.SelectedIndex = 2;
                break;
            //查看地图
            case 3:
                Tabs.SelectedIndex = 4;
                break;
            //导出
            case 5:

                break;
            //测试
            case 6:

                break;
        }
    }

    public void Log(string? data)
    {
        if (data == null)
            return;


    }

    public void SetGame(GameSettingObj obj)
    {
        Obj = obj;
        Head.Title = string.Format(Localizer.Instance["GameEditWindow.Title"], obj.Name);

        tab1.SetGame(obj);
        tab2.SetGame(obj);
        tab3.SetGame(obj);
        tab4.SetGame(obj);
        tab5.SetGame(obj);
    }

    private void Tabs_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        switch (Tabs.SelectedIndex)
        {
            case 0:
                Go(tab1);
                break;
            case 1:
                tab2.Update();
                Go(tab2);
                break;
            case 2:
                tab3.Update();
                Go(tab3);
                break;
            case 3:
                tab4.Update();
                Go(tab4);
                break;
            case 4:
                tab5.Update();
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
        App.GameEditWindows.Remove(Obj);
    }

    public void Update()
    {
        App.Update(this, Image_Back, Grid1);
    }
}
