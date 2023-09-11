using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.GameExport;
using ColorMC.Gui.UI.Windows;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.GameExport;

public partial class GameExportControl : UserControl, IUserControl
{
    private bool _switch1 = false;

    private readonly Tab1Control _tab1 = new();
    private readonly Tab2Control _tab2 = new();
    private readonly Tab3Control _tab3 = new();
    private readonly Tab4Control _tab4 = new();

    private CancellationTokenSource _cancel = new();

    private int _now;

    private GameSettingObj _obj;

    private Bitmap _icon;
    public Bitmap GetIcon() => _icon;

    public IBaseWindow Window => App.FindRoot(VisualRoot);
    public UserControl Con => this;
    public string Title =>
        string.Format(App.GetLanguage("GameExportWindow.Title"), _obj.Name);

    public GameExportControl()
    {
        InitializeComponent();
    }

    public GameExportControl(GameSettingObj obj) : this()
    {
        _obj = obj;

        StackPanel1.PointerPressed += StackPanel1_PointerPressed;
        StackPanel2.PointerPressed += StackPanel2_PointerPressed;

        Content1.Content = _tab1;
    }

    private void StackPanel2_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as GameExportModel)!.OpenSide();
    }

    private void StackPanel1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as GameExportModel)!.CloseSide();
    }

    public async void Opened()
    {
        Window.SetTitle(Title);

        _tab2.Opened();
        _tab4.Opened();

        var model = (DataContext as GameExportModel)!;
        model.Model.Progress(App.GetLanguage("GameExportWindow.Info7"));
        await model.LoadMod();
        model.LoadFile();

        var icon = model.Obj.GetIconFile();
        if (File.Exists(icon))
        {
            _icon = new(icon);
            Window.SetIcon(_icon);
        }
        model.Model.ProgressClose();
    }

    private void Go(UserControl to)
    {
        _cancel.Cancel();
        _cancel.Dispose();

        _cancel = new();

        var model = (DataContext as GameExportModel)!;

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

        App.GameExportWindows.Remove(_obj.UUID);
    }

    public void SetBaseModel(BaseModel model)
    {
        var amodel = new GameExportModel(model, _obj);
        amodel.PropertyChanged += Amodel_PropertyChanged;
        DataContext = amodel;
    }

    private void Amodel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "NowView")
        {
            var model = (DataContext as GameExportModel)!;
            switch (model.NowView)
            {
                case 0:
                    Go(_tab1);
                    break;
                case 1:
                    Go(_tab2);
                    break;
                case 2:
                    Go(_tab3);
                    break;
                case 3:
                    Go(_tab4);
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
