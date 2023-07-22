using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Setting;
using ColorMC.Gui.UI.Windows;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class SettingControl : UserControl, IUserControl
{
    private readonly Tab1Control _tab1 = new();
    private readonly Tab2Control _tab2 = new();
    private readonly Tab3Control _tab3 = new();
    private readonly Tab4Control _tab4 = new();
    private readonly Tab5Control _tab5 = new();
    private readonly Tab6Control _tab6 = new();
    private readonly Tab7Control _tab7 = new();

    private bool _switch1 = false;

    private readonly SettingTab1Model _model1;
    private readonly SettingTab2Model _model2;
    private readonly SettingTab3Model _model3;
    private readonly SettingTab4Model _model4;
    private readonly SettingTab5Model _model5;
    private readonly SettingTab6Model _model6;
    private readonly SettingTab7Model _model7;

    private CancellationTokenSource _cancel = new();

    private int _now;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public UserControl Con => this;

    public string Title => App.GetLanguage("SettingWindow.Title");

    public SettingControl()
    {
        InitializeComponent();

        _model1 = new(this);
        _tab1.DataContext = _model1;

        _model2 = new(this);
        _tab2.DataContext = _model2;

        _model3 = new(this);
        _tab3.DataContext = _model3;

        _model4 = new(this);
        _tab4.DataContext = _model4;

        _model5 = new(this);
        _tab5.DataContext = _model5;

        _model6 = new(this);
        _tab6.DataContext = _model6;

        _model7 = new();
        _tab7.DataContext = _model7;

        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;

        Tabs.SelectionChanged += Tabs_SelectionChanged;

        Content1.Content = _tab2;
    }

    public void Closed()
    {
        App.SettingWindow = null;
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
                Go(_tab2);
                _model2.Load();
                break;
            case 1:
                Go(_tab3);
                _model3.Load();
                break;
            case 2:
                Go(_tab4);
                _model4.Load();
                break;
            case 3:
                Go(_tab5);
                _model5.Load();
                break;
            case 4:
                Go(_tab6);
                _model6.Load();
                break;
            case 5:
                Go(_tab1);
                break;
            case 6:
                Go(_tab7);
                break;
        }

        _now = Tabs.SelectedIndex;
    }

    private void Go(UserControl to)
    {
        _cancel.Cancel();
        _cancel.Dispose();

        _cancel = new();

        Tabs.IsEnabled = false;

        if (!_switch1)
        {
            Content2.Content = to;
            _ = App.PageSlide500.Start(Content1, Content2, _now < Tabs.SelectedIndex, _cancel.Token);
        }
        else
        {
            Content1.Content = to;
            _ = App.PageSlide500.Start(Content2, Content1, _now < Tabs.SelectedIndex, _cancel.Token);
        }

        _switch1 = !_switch1;
        Tabs.IsEnabled = true;
    }

    public void GoTo(SettingType type)
    {
        switch (type)
        {
            case SettingType.SetJava:
                Tabs.SelectedIndex = 3;
                break;
        }
    }

    public void Opened()
    {
        Window.SetTitle(Title);

        _model2.Load();
    }
}
