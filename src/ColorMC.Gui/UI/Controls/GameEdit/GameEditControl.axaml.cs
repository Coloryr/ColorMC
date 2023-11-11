using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UI.Windows;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class GameEditControl : UserControl, IUserControl
{
    private readonly GameSettingObj _obj;

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
    private CancellationTokenSource _cancel1 = new();

    private int _now;

    private Bitmap _icon;
    public Bitmap GetIcon() => _icon;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public UserControl Con => this;

    public string Title =>
        string.Format(App.Lang("GameEditWindow.Title"), _obj.Name);

    public string UseName { get; }

    public GameEditControl()
    {
        InitializeComponent();
    }

    public GameEditControl(GameSettingObj obj) : this()
    {
        UseName = (ToString() ?? "GameEditControl") + ":" + obj.UUID;

        _obj = obj;

        StackPanel1.PointerPressed += StackPanel1_PointerPressed;
        StackPanel2.PointerPressed += StackPanel2_PointerPressed;
    }

    private void StackPanel2_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as GameEditModel)!.OpenSide();
    }

    private void StackPanel1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as GameEditModel)!.CloseSide();
    }

    public async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            var model = (DataContext as GameEditModel)!;
            switch (model.NowView)
            {
                case 2:
                    await model.LoadMod();
                    break;
                case 3:
                    await model.LoadWorld();
                    break;
                case 4:
                    await model.LoadResource();
                    break;
                case 5:
                    model.LoadScreenshot();
                    break;
                case 6:
                    model.LoadServer();
                    break;
                case 7:
                    await model.LoadShaderpack();
                    break;
                case 8:
                    await model.LoadSchematic();
                    break;
            }
        }
    }
    public void Opened()
    {
        Window.SetTitle(Title);

        Content1.Content = _tab1;

        _tab4.Opened();
        _tab10.Opened();
        _tab11.Opened();
        _tab12.Opened();

        var icon = _obj.GetIconFile();
        if (File.Exists(icon))
        {
            _icon = new(icon);
            Window.SetIcon(_icon);
        }

        (DataContext as GameEditModel)?.OpenLoad();
    }

    public void SetType(GameEditWindowType type)
    {
        var model = (DataContext as GameEditModel)!;
        switch (type)
        {
            case GameEditWindowType.Mod:
                model.NowView = 2;
                break;
            case GameEditWindowType.World:
                model.NowView = 3;
                break;
        }
    }

    private void Go(UserControl to)
    {
        _cancel.Cancel();
        _cancel.Dispose();

        _cancel = new();

        var model = (DataContext as GameEditModel)!;

        if (!_switch1)
        {
            Content2.Content = to;
            _ = App.PageSlide500.Start(Content1, Content2, _now < model.NowView, _cancel.Token);
        }
        else
        {
            Content1.Content = to;
            _ = App.PageSlide500.Start(Content2, Content1, _now < model.NowView, _cancel.Token);
        }

        _switch1 = !_switch1;
    }

    public void Closed()
    {
        _icon?.Dispose();

        App.GameEditWindows.Remove(_obj.UUID);
    }

    public void Started()
    {
        (DataContext as GameEditModel)!.GameStateChange();
    }

    public void SetBaseModel(BaseModel model)
    {
        var amodel = new GameEditModel(model, _obj);
        amodel.PropertyChanged += Amodel_PropertyChanged;
        DataContext = amodel;
    }

    private async void Amodel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Switch")
        {
            var model = (DataContext as GameEditModel)!;
            switch (model.NowView)
            {
                case 0:
                    _tab1.Reset();
                    break;
                case 1:
                    _tab2.Reset();
                    break;
            }
        }
        else  if (e.PropertyName == "NowView")
        {
            var model = (DataContext as GameEditModel)!;
            switch (_now)
            {
                case 2:
                    model.RemoveBackHead();
                    break;
                case 7:
                    model.RemoveBackHeadTab10();
                    break;
            }
            _now = model.NowView;
            switch (model.NowView)
            {
                case 0:
                    Go(_tab1);
                    model.GameLoad();
                    break;
                case 1:
                    Go(_tab2);
                    model.ConfigLoad();
                    break;
                case 2:
                    Go(_tab4);
                    model.SetBackHeadTab();
                    await model.LoadMod();
                    break;
                case 3:
                    Go(_tab5);
                    await model.LoadWorld();
                    break;
                case 4:
                    Go(_tab8);
                    await model.LoadResource();
                    break;
                case 5:
                    Go(_tab9);
                    model.LoadScreenshot();
                    break;
                case 6:
                    Go(_tab10);
                    model.SetBackHeadTab10();
                    model.LoadServer();
                    break;
                case 7:
                    Go(_tab11);
                    await model.LoadShaderpack();
                    break;
                case 8:
                    Go(_tab12);
                    await model.LoadSchematic();
                    break;
            }
        }
        else if (e.PropertyName == "SideOpen")
        {
            _cancel1.Cancel();
            _cancel1.Dispose();
            _cancel1 = new();

            StackPanel1.IsVisible = true;
            Dispatcher.UIThread.Post(() =>
            {
                App.SidePageSlide300.Start(null, DockPanel1, _cancel1.Token);
            });
        }
        else if (e.PropertyName == "SideClose")
        {
            _cancel1.Cancel();
            _cancel1.Dispose();
            _cancel1 = new();
            App.SidePageSlide300.Start(DockPanel1, null, _cancel1.Token);
            StackPanel1.IsVisible = false;
        }
    }
}
