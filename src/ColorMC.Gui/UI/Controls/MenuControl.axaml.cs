using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Windows;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls;

public partial class MenuControl : UserControl, IUserControl
{
    private bool _switch1 = false;

    private CancellationTokenSource _cancel = new();
    private CancellationTokenSource _cancel1 = new();

    private int _now = -1;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public virtual string UseName { get; }
    public virtual string Title { get; }

    public MenuControl()
    {
        InitializeComponent();

        StackPanel1.PointerPressed += StackPanel1_PointerPressed;
        StackPanel2.PointerPressed += StackPanel2_PointerPressed;
    }

    virtual protected MenuModel SetModel(BaseModel model) { throw new WarningException(); }
    virtual protected Control ViewChange(bool iswhell, int old, int index) { throw new WarningException(); }

    virtual public void OnKeyDown(object? sender, KeyEventArgs e) { }
    virtual public Bitmap GetIcon() { return App.GameIcon; }
    virtual public void Opened() { }
    virtual public void Closed() { }
    virtual public Task<bool> Closing() { return Task.FromResult(false); }

    private void StackPanel2_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as MenuModel)!.OpenSide();
    }

    private void StackPanel1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as MenuModel)!.CloseSide();
    }

    private void Go(Control to)
    {
        _cancel.Cancel();
        _cancel.Dispose();

        _cancel = new();

        var model = (DataContext as MenuModel)!;

        if (_now == -1)
        {
            Content1.Child = to;
            return;
        }

        if (!_switch1)
        {
            Content2.Child = to;
            _ = App.PageSlide500.Start(Content1, Content2, _now < model.NowView, _cancel.Token);
        }
        else
        {
            Content1.Child = to;
            _ = App.PageSlide500.Start(Content2, Content1, _now < model.NowView, _cancel.Token);
        }

        _switch1 = !_switch1;
    }

    public void SetBaseModel(BaseModel model)
    {
        var model1 = SetModel(model);
        model1.PropertyChanged += Model1_PropertyChanged;
        DataContext = model1;
    }

    private void Model1_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "SideOpen")
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
        else if (e.PropertyName == "WindowClose")
        {
            Window.Close();
        }
        else if (e.PropertyName == "NowView")
        {
            var model = (DataContext as MenuModel)!;
            Go(ViewChange(model.IsWhell, _now, model.NowView));
            _now = model.NowView;
        }
    }
}
