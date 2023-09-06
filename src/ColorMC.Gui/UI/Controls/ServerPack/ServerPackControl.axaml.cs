using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.ServerPack;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.ServerPack;

public partial class ServerPackControl : UserControl, IUserControl
{
    private GameSettingObj _obj;

    private readonly Tab1Control _tab1 = new();
    private readonly Tab2Control _tab2 = new();
    private readonly Tab3Control _tab3 = new();
    private readonly Tab4Control _tab4 = new();

    private bool _switch1 = false;

    private CancellationTokenSource _cancel = new();

    private int _now;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public string Title => string.Format(App.GetLanguage("ServerPackWindow.Title"),
           _obj.Name);

    public ServerPackControl()
    {
        InitializeComponent();
    }

    public ServerPackControl(GameSettingObj obj) : this()
    {
        _obj = obj;
        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;

        Tabs.SelectionChanged += Tabs_SelectionChanged;

        Content1.Content = _tab1;
    }

    public void Opened()
    {
        Window.SetTitle(Title);

        _tab2.Opened();
        _tab3.Opened();
        _tab4.Opened();
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
        var model = (DataContext as ServerPackModel)!;
        switch (Tabs.SelectedIndex)
        {
            case 0:
                Go(_tab1);
                model.LoadConfig();
                break;
            case 1:
                Go(_tab2);
                model.LoadMod();
                break;
            case 2:
                Go(_tab3);
                model.LoadConfigList();
                break;
            case 3:
                Go(_tab4);
                model.LoadFile();
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
        App.ServerPackWindows.Remove(_obj.UUID);
    }

    public void SetBaseModel(BaseModel model)
    {
        var pack = GameBinding.GetServerPack(_obj);
        if (pack == null)
        {
            pack = new()
            {
                Game = _obj,
                Mod = new(),
                Resourcepack = new(),
                Config = new()
            };

            GameBinding.SaveServerPack(pack);
        }

        var amodel = new ServerPackModel(model, pack);
        DataContext = amodel;
    }
}
