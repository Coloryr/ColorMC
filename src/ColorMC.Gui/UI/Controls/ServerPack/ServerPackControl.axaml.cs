using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UI.Model.ServerPack;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System.ComponentModel;
using System.IO;
using ColorMC.Core.LaunchPath;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.ServerPack;

public partial class ServerPackControl : UserControl, IUserControl
{
    private readonly GameSettingObj _obj;

    private readonly Tab1Control _tab1 = new();
    private readonly Tab2Control _tab2 = new();
    private readonly Tab3Control _tab3 = new();
    private readonly Tab4Control _tab4 = new();

    private bool _switch1 = false;

    private CancellationTokenSource _cancel = new();

    private int _now;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    private Bitmap _icon;
    public Bitmap GetIcon() => _icon;

    public string Title => string.Format(App.GetLanguage("ServerPackWindow.Title"),
           _obj.Name);

    public ServerPackControl()
    {
        InitializeComponent();
    }

    public ServerPackControl(GameSettingObj obj) : this()
    {
        _obj = obj;

        StackPanel1.PointerPressed += StackPanel1_PointerPressed;
        StackPanel2.PointerPressed += StackPanel2_PointerPressed;

        Content1.Content = _tab1;
    }

    private void StackPanel2_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as ServerPackModel)!.OpenSide();
    }

    private void StackPanel1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as ServerPackModel)!.CloseSide();
    }

    public void Opened()
    {
        Window.SetTitle(Title);

        _tab2.Opened();
        _tab3.Opened();
        _tab4.Opened();

        var icon = _obj.GetIconFile();
        if (File.Exists(icon))
        {
            _icon = new(icon);
            Window.SetIcon(_icon);
        }
    }

    private void Go(UserControl to)
    {
        _cancel.Cancel();
        _cancel.Dispose();

        _cancel = new();

        var model = (DataContext as ServerPackModel)!;

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
        amodel.PropertyChanged += Amodel_PropertyChanged;
        DataContext = amodel;
    }

    private void Amodel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "NowView")
        {
            var model = (DataContext as ServerPackModel)!;
            switch (model.NowView)
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

            _now = model.NowView;
        }
        else if (e.PropertyName == "SideOpen")
        {
            App.CrossFade100.Start(null, StackPanel1);
        }
        else if (e.PropertyName == "SideClose")
        {
            StackPanel1.IsVisible = false;
        }
    }
}
