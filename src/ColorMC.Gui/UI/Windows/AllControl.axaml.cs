using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Controls.Custom;
using ColorMC.Gui.UI.Controls.Main;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Windows;

public partial class AllControl : UserControl, IBaseWindow
{
    private IUserControl _baseControl;
    private IUserControl _nowControl;

    private readonly List<Control> controls = [];

    public IBaseWindow Window => this;

    public IUserControl ICon => _nowControl;

    public BaseModel Model => (DataContext as BaseModel)!;

    private WindowNotificationManager windowNotification;

    public AllControl()
    {
        InitializeComponent();

        DataContext = new BaseModel("AllControl");

        if (SystemInfo.Os == OsType.Linux)
        {
            ResizeButton.IsVisible = true;
        }

        PointerPressed += AllControl_PointerPressed;
        PointerReleased += AllControl_PointerReleased;

        ResizeButton.AddHandler(PointerPressedEvent, ResizeButton_PointerPressed, RoutingStrategies.Tunnel);

        App.PicUpdate += Update;

        Update();
    }

    public Task<bool> IKeyDown(object? sender, KeyEventArgs e)
    {
        return ICon.OnKeyDown(sender, e);
    }

    private void AllControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        ICon.IPointerReleased(e);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var level = TopLevel.GetTopLevel(this);
        windowNotification = new WindowNotificationManager(level)
        {
            Position = NotificationPosition.TopRight,
            MaxItems = 3,
            Margin = new(0, 30, 0, 0)
        };
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == BaseModel.InfoName)
        {
            windowNotification.Show(Model.NotifyText);
        }
    }

    private void ResizeButton_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        e.Handled = true;
        (VisualRoot as Window)?.BeginResizeDrag(WindowEdge.SouthEast, e);
    }

    private void AllControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsXButton1Pressed)
        {
            Model.BackClick();
        }

        ICon.IPointerPressed(e);
    }

    public void Closed()
    {
        Controls.Child = null;
    }

    public void Opened()
    {
        Update();
    }

    public void Add(IUserControl con)
    {
        if (_baseControl == null)
        {
            _baseControl = con;
            var con1 = (_baseControl as Control)!;
            Controls.Child = con1;
            Dispatcher.UIThread.Post(() =>
            {
                _baseControl.Opened();
            });
        }
        else
        {
            var con1 = Controls.Child;
            var con2 = (con as Control)!;
            Controls.Child = null;
            if (con1 is not (MainControl or CustomControl) && con1 is { } con3)
            {
                controls.Add(con3);
            }
            Controls.Child = con2;
            App.CrossFade300.Start(null, con2);

            con.Opened();
            Model.AddBackCall(Back);
        }

        _nowControl = con;
        SetTitle(_nowControl.Title);
        SetIcon(_nowControl.GetIcon());
    }

    public void Active(IUserControl con)
    {
        var con1 = (con as Control)!;

        controls.Remove(con1);
        var con2 = Controls.Child;
        controls.Add(con2!);
        Controls.Child = con1;

        _nowControl = con;
        SetTitle(_nowControl.Title);
        SetIcon(_nowControl.GetIcon());
    }

    public async void Close(IUserControl con)
    {
        var res = await con.Closing();
        if (res)
        {
            return;
        }

        var con1 = Controls.Child;
        var con2 = (con as Control)!;
        if (con1 == con2)
        {
            Controls.Child = null;
        }
        controls.Remove(con2);
        if (Controls.Child == null)
        {
            if (controls.Count > 0)
            {
                con1 = controls.Last();
                controls.Remove(con1);
                _nowControl = (con1 as IUserControl)!;
            }
            else
            {
                con1 = (_baseControl as Control)!;
                _nowControl = _baseControl;
            }
            Controls.Child = con1;
        }

        SetTitle(_nowControl.Title);
        SetIcon(_nowControl.GetIcon());

        ((con as UserControl)?.DataContext as TopModel)?.TopClose();
        con.Closed();

        Model.RemoveBack();

        App.Clear();
    }

    private void Back()
    {
        if (_nowControl == null)
        {
            return;
        }

        else
        {
            Close(_nowControl);
        }
    }

    private void Update()
    {
        App.UpdateWindow(Model);
    }

    public void SetTitle(string data)
    {
        if (VisualRoot is SingleWindow win)
        {
            win.Title = data;
        }

        Model.Title = data;
    }

    public async Task<bool> Closing()
    {
        if (_nowControl is MainControl || _nowControl is CustomControl)
            return false;
        if (_nowControl is not IUserControl now)
            return false;

        return await now.Closing();
    }

    public void SetIcon(Bitmap icon)
    {
        Model.SetIcon(icon);
    }

    public void Hide()
    {
        (VisualRoot as Window)?.Hide();
    }

    public void WindowStateChange(WindowState windowState)
    {
        ICon.WindowStateChange(windowState);
        Head.WindowStateChange(windowState);
    }

    public void SetSize(int width, int height)
    {

    }
}
