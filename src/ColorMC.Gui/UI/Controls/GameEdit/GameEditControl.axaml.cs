using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UI.Windows;
using System.IO;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class GameEditControl : UserControl, IUserControl
{
    private GameSettingObj _obj;

    private bool _switch1 = false;

    private readonly Tab1Control _tab1 = new();
    private readonly Tab2Control _tab2 = new();
    private readonly Tab4Control _tab4 = new();
    private readonly Tab5Control _tab5 = new();
    private readonly Tab8Control _tab8 = new();
    private readonly Tab9Control _tab9 = new();
    private readonly Tab10Control _tab10 = new();
    private readonly Tab11Control _tab11 = new();
    private readonly Tab12Control _tab12 = new();

    private CancellationTokenSource _cancel = new();

    private int _now;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public UserControl Con => this;

    public string Title =>
        string.Format(App.GetLanguage("GameEditWindow.Title"), _obj.Name);

    public BaseModel Model { get; }

    public GameEditControl()
    {
        InitializeComponent();
    }

    public GameEditControl(GameSettingObj obj) : this()
    {
        _obj = obj;

        Tabs.SelectionChanged += Tabs_SelectionChanged;

        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;

        Content1.Content = _tab1;
    }

    public async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            switch (Tabs.SelectedIndex)
            {
                case 2:
                    await _model4.Load();
                    break;
                case 3:
                    await _model5.Load();
                    break;
                case 4:
                    await _model8.Load();
                    break;
                case 5:
                    _model9.Load();
                    break;
                case 6:
                    await _model10.Load();
                    break;
                case 7:
                    await _model11.Load();
                    break;
                case 8:
                    await _model12.Load();
                    break;
            }
        }
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
        Window.SetTitle(Title);

        _tab4.Opened();
        _tab10.Opened();
        _tab11.Opened();
        _tab12.Opened();

        var icon = _model1.Obj.GetIconFile();
        if (File.Exists(icon))
        {
            Window.SetIcon(new(icon));
        }
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
        }
    }

    private async void Tabs_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        switch (Tabs.SelectedIndex)
        {
            case 0:
                Go(_tab1);
                _model1.Load();
                break;
            case 1:
                Go(_tab2);
                _model2.Load();
                break;
            case 2:
                Go(_tab4);
                await _model4.Load();
                break;
            case 3:
                Go(_tab5);
                await _model5.Load();
                break;
            case 4:
                Go(_tab8);
                await _model8.Load();
                break;
            case 5:
                Go(_tab9);
                _model9.Load();
                break;
            case 6:
                Go(_tab10);
                await _model10.Load();
                break;
            case 7:
                Go(_tab11);
                await _model11.Load();
                break;
            case 8:
                Go(_tab12);
                await _model12.Load();
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

    public void Closed()
    {
        App.GameEditWindows.Remove(_model1.Obj.UUID);
    }

    public void Started()
    {
        _model1.GameStateChange();
    }

    public void SetBaseModel(BaseModel model)
    {
        var amodel = new GameEditModel(model, _obj);
        DataContext = amodel;
    }
}
