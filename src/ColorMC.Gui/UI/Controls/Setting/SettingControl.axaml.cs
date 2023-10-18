using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Setting;
using ColorMC.Gui.UI.Windows;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

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
    private CancellationTokenSource _cancel1 = new();

    private int _now;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public string Title => App.GetLanguage("SettingWindow.Title");

    public SettingControl()
    {
        InitializeComponent();

        StackPanel1.PointerPressed += StackPanel1_PointerPressed;
        StackPanel2.PointerPressed += StackPanel2_PointerPressed;
    }

    private void StackPanel2_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as SettingModel)!.OpenSide();
    }

    private void StackPanel1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as SettingModel)!.CloseSide();
    }

    public void Closed()
    {
        App.SettingWindow = null;
    }

    private void Go(UserControl to)
    {
        _cancel.Cancel();
        _cancel.Dispose();

        _cancel = new();

        var model = (DataContext as SettingModel)!;

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

    public async void GoTo(SettingType type)
    {
        var model = (DataContext as SettingModel)!;
        switch (type)
        {
            case SettingType.SetJava:
                model.NowView = 3;
                break;
            case SettingType.Net:
                model.NowView = 1;
                await model.GameCloudConnect();
                break;
        }
    }

    public void Opened()
    {
        Window.SetTitle(Title);

        Content1.Content = _tab2;
        (DataContext as SettingModel)!.LoadUISetting();
    }

    public void SetBaseModel(BaseModel model)
    {
        var amodel = new SettingModel(model);
        amodel.PropertyChanged += Amodel_PropertyChanged;
        DataContext = amodel;
        _tab7.DataContext = new SettingTab7Model();
    }

    private double x;

    private async void Amodel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "NowView")
        {
            var model = (DataContext as SettingModel)!;
            switch (model.NowView)
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

            _now = model.NowView;
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
