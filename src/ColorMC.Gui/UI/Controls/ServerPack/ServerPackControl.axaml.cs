using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.ServerPack;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.ServerPack;

public partial class ServerPackControl : UserControl, IUserControl
{
    private readonly Tab1Control _tab1 = new();
    private readonly Tab2Control _tab2 = new();
    private readonly Tab3Control _tab3 = new();
    private readonly Tab4Control _tab4 = new();

    private readonly ServerPackTab1Model _model1;
    private readonly ServerPackTab2Model _model2;
    private readonly ServerPackTab3Model _model3;
    private readonly ServerPackTab4Model _model4;

    private readonly ServerPackModel _model;

    private bool _switch1 = false;

    private CancellationTokenSource _cancel = new();

    private int _now;

    public string GameName => _model1.Obj.Game.Name;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public UserControl Con => this;

    public string Title => string.Format(App.GetLanguage("ServerPackWindow.Title"),
            _model1.Obj.Game.Name);

    public ServerPackControl() : this(new() { Empty = true })
    {

    }

    public ServerPackControl(GameSettingObj obj)
    {
        InitializeComponent();

        if (!obj.Empty)
        {
            var pack = GameBinding.GetServerPack(obj);
            if (pack == null)
            {
                pack = new()
                {
                    Game = obj,
                    Mod = new(),
                    Resourcepack = new(),
                    Config = new()
                };

                GameBinding.SaveServerPack(pack);
            }

            _model = new(this, pack);
            DataContext = _model;

            _model1 = new(this, pack);
            _tab1.DataContext = _model1;

            _model2 = new(this, pack);
            _tab2.DataContext = _model2;

            _model3 = new(this, pack);
            _tab3.DataContext = _model3;

            _model4 = new(this, pack);
            _tab4.DataContext = _model4;
        }

        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;

        Tabs.SelectionChanged += Tabs_SelectionChanged;

        Content1.Content = _tab1;
    }

    public void Opened()
    {
        Window.SetTitle(Title);
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
                Go(_tab1);
                _model1.Load();
                break;
            case 1:
                Go(_tab2);
                _model2.Load();
                break;
            case 2:
                Go(_tab3);
                _model3.Load();
                break;
            case 3:
                Go(_tab4);
                _model4.Load();
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
        App.ServerPackWindows.Remove(_model1.Obj.Game.UUID);
    }
}
