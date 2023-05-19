using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UI.Windows;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class GameEditControl : UserControl, IUserControl
{
    private bool switch1 = false;

    private readonly Tab1Control tab1 = new();
    private readonly Tab2Control tab2 = new();
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

    private readonly GameEditTab1Model model1;
    private readonly GameEditTab2Model model2;
    private readonly GameEditTab4Model model4;
    private readonly GameEditTab5Model model5;
    private readonly GameEditTab10Model model10;
    private readonly GameEditTab11Model model11;
    private readonly GameEditTab12Model model12;

    private int now;

    public GameSettingObj Obj { get; private set; }

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public GameEditControl(GameSettingObj obj)
    {
        Obj = obj;

        tab6.SetGame(obj);
        tab7.SetGame(obj);
        tab8.SetGame(obj);
        tab9.SetGame(obj);

        model1 = new(this, obj);
        tab1.DataContext = model1;

        model2 = new(this, obj);
        tab2.DataContext = model2;

        model4 = new(this, obj);
        tab4.DataContext = model4;

        model5 = new(this, obj);
        tab5.DataContext = model5;

        model10 = new(this, obj);
        tab10.DataContext = model10;

        model11 = new(this, obj);
        tab11.DataContext = model11;

        model12 = new(this, obj);
        tab12.DataContext = model12;

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
    }

    public void SetType(GameEditWindowType type)
    {
        switch (type)
        {
            case GameEditWindowType.Mod:
                Tabs.SelectedIndex = 2;
                break;
            case GameEditWindowType.World:
                Tabs.SelectedIndex = 3;
                break;
            case GameEditWindowType.Export:
                Tabs.SelectedIndex = 9;
                break;
            case GameEditWindowType.Log:
                Tabs.SelectedIndex = 10;
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
                model1.Load();
                break;
            case 1:
                Go(tab2);
                model2.Load();
                break;
            case 2:
                Go(tab4);
                model4.Load();
                break;
            case 3:
                Go(tab5);
                model5.Load();
                break;
            case 4:
                Go(tab8);
                tab8.Update();
                break;
            case 5:
                Go(tab9);
                tab9.Update();
                break;
            case 6:
                Go(tab10);
                model10.Load();
                break;
            case 7:
                Go(tab11);
                model11.Load();
                break;
            case 8:
                Go(tab12);
                model12.Load();
                break;
            case 9:
                Go(tab6);
                tab6.Update();
                break;
            case 10:
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
