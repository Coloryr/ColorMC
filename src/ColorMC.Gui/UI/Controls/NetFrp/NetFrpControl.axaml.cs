using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.NetFrp;
using ColorMC.Gui.UI.Windows;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.NetFrp;

public partial class NetFrpControl : UserControl, IUserControl
{
    private readonly NetFrpTab1Control _tab1 = new();
    private readonly NetFrpTab2Control _tab2 = new();
    private readonly NetFrpTab3Control _tab3 = new();

    private bool _switch1 = false;

    private CancellationTokenSource _cancel = new();
    private CancellationTokenSource _cancel1 = new();

    private int _now;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public string Title => App.Lang("NetFrpWindow.Ttile");

    public NetFrpControl()
    {
        InitializeComponent();

        StackPanel1.PointerPressed += StackPanel1_PointerPressed;
        StackPanel2.PointerPressed += StackPanel2_PointerPressed;
    }

    private void StackPanel2_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as NetFrpModel)!.OpenSide();
    }

    private void StackPanel1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as NetFrpModel)!.CloseSide();
    }
    public void Closed()
    {
        App.NetFrpWindow = null;
    }

    private void Go(UserControl to)
    {
        _cancel.Cancel();
        _cancel.Dispose();

        _cancel = new();

        var model = (DataContext as NetFrpModel)!;

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

    public void Opened()
    {
        Window.SetTitle(Title);

        Content1.Content = _tab1;
        (DataContext as NetFrpModel)?.Load();
    }

    public void SetBaseModel(BaseModel model)
    {
        var amodel = new NetFrpModel(model);
        amodel.PropertyChanged += Amodel_PropertyChanged;
        DataContext = amodel;
    }

    public void SetProcess(Process process, NetFrpLocalModel model1, string ip)
    {
        var model = (DataContext as NetFrpModel)!;
        model.SetProcess(process, model1, ip);
        model.NowView = 2;
    }

    private void Amodel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "NowView")
        {
            var model = (DataContext as NetFrpModel)!;
            switch (model.NowView)
            {
                case 0:
                    Go(_tab1);
                    model.Load();
                    break;
                case 1:
                    Go(_tab2);
                    model.LoadLocal();
                    break;
                case 2:
                    Go(_tab3);
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

    public Task<bool> Closing()
    {
        return (DataContext as NetFrpModel)!.Closing();
    }
}
