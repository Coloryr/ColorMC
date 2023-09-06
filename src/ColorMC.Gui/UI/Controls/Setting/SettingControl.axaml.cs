using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
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

    private CancellationTokenSource _cancel = new();

    private int _now;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public string Title => App.GetLanguage("SettingWindow.Title");

    public SettingControl()
    {
        InitializeComponent();

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
        var model = (DataContext as SettingModel)!;
        switch (Tabs.SelectedIndex)
        {
            case 0:
                Go(_tab2);
                model.LoadUISetting();
                break;
            case 1:
                Go(_tab3);
                model.LoadHttpSetting();
                break;
            case 2:
                Go(_tab4);
                model.LoadArg();
                break;
            case 3:
                Go(_tab5);
                model.LoadJava();
                break;
            case 4:
                Go(_tab6);
                model.LoadServer();
                break;
            case 5:
                Go(_tab1);
                break;
            case 6:
                Go(_tab7);
                _tab7.Start();
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

        (DataContext as SettingModel)!.LoadUISetting();
    }

    public void SetBaseModel(BaseModel model)
    {
        var amodel = new SettingModel(model);
        DataContext = amodel;

        _tab7.DataContext = new SettingTab7Model();
    }
}
