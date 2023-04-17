using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class GameEditControl : UserControl, IUserControl
{
    private bool switch1 = false;

    private readonly Tab1Control tab1 = new();
    private readonly Tab2Control tab2 = new();
    private readonly Tab3Control tab3 = new();
    private readonly Tab4Control tab4 = new();
    private readonly Tab5Control tab5 = new();
    private readonly Tab6Control tab6 = new();
    private readonly Tab7Control tab7 = new();
    private readonly Tab8Control tab8 = new();
    private readonly Tab9Control tab9 = new();
    private readonly Tab10Control tab10 = new();
    private readonly Tab11Control tab11 = new();
    private readonly Tab12Control tab12 = new();

    private readonly ContentControl content1 = new();
    private readonly ContentControl content2 = new();
    private CancellationTokenSource cancel = new();

    private int now;

    public GameSettingObj? Obj { get; private set; }

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public GameEditControl(GameSettingObj? obj)
    {
        Obj = obj;

        tab1.SetGame(obj);
        tab2.SetGame(obj);
        tab3.SetGame(obj);
        tab4.SetGame(obj);
        tab5.SetGame(obj);
        tab6.SetGame(obj);
        tab7.SetGame(obj);
        tab8.SetGame(obj);
        tab9.SetGame(obj);
        tab10.SetGame(obj);
        tab11.SetGame(obj);
        tab12.SetGame(obj);

        InitializeComponent();

        Tabs.SelectionChanged += Tabs_SelectionChanged;

        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;

        Tab1.Children.Add(content1);
        Tab1.Children.Add(content2);

        content1.Content = tab1;
    }

    public GameEditControl() : this(null)
    {

    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (e.Delta.Y > 0)
        {
            ScrollViewer1.LineLeft();
            ScrollViewer1.LineLeft();
            ScrollViewer1.LineLeft();
            ScrollViewer1.LineLeft();
            ScrollViewer1.LineLeft();
        }
        else if (e.Delta.Y < 0)
        {
            ScrollViewer1.LineRight();
            ScrollViewer1.LineRight();
            ScrollViewer1.LineRight();
            ScrollViewer1.LineRight();
            ScrollViewer1.LineRight();
        }
    }

    public void Opened()
    {
        Window.SetTitle(string.Format(App.GetLanguage("GameEditWindow.Title"), Obj?.Name));

        tab1.Update();
    }

    public void SetType(GameEditWindowType type)
    {
        switch (type)
        {
            case GameEditWindowType.Config:
                Tabs.SelectedIndex = 2;
                break;
            case GameEditWindowType.Mod:
                Tabs.SelectedIndex = 3;
                break;
            case GameEditWindowType.World:
                Tabs.SelectedIndex = 4;
                break;
            case GameEditWindowType.Export:
                Tabs.SelectedIndex = 10;
                break;
            case GameEditWindowType.Log:
                Tabs.SelectedIndex = 11;
                break;
        }
    }

    public void ClearLog()
    {
        tab7.Clear();
    }

    public void Log(string? data)
    {
        if (data == null)
            return;

        tab7.Log(data);
    }

    private void Tabs_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        switch (Tabs.SelectedIndex)
        {
            case 0:
                Go(tab1);
                tab1.Update();
                break;
            case 1:
                Go(tab2);
                tab2.Update();
                break;
            case 2:
                Go(tab3);
                tab3.Update();
                break;
            case 3:
                Go(tab4);
                tab4.Update();
                break;
            case 4:
                Go(tab5);
                tab5.Update();
                break;
            case 5:
                Go(tab8);
                tab8.Update();
                break;
            case 6:
                Go(tab9);
                tab9.Update();
                break;
            case 7:
                Go(tab10);
                tab10.Update();
                break;
            case 8:
                Go(tab11);
                tab11.Update();
                break;
            case 9:
                Go(tab12);
                tab12.Update();
                break;
            case 10:
                Go(tab6);
                tab6.Update();
                break;
            case 11:
                Go(tab7);
                tab7.Update();
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

    public void Closed()
    {
        App.GameEditWindows.Remove(Obj.UUID);
    }
}
