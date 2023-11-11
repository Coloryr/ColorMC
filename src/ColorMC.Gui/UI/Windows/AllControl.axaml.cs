using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Controls.Custom;
using ColorMC.Gui.UI.Controls.Main;
using ColorMC.Gui.UI.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Windows;

public partial class AllControl : UserControl, IBaseWindow
{
    private IUserControl _baseControl;
    private IUserControl _nowControl;

    private readonly List<Control> controls = new();

    public IBaseWindow Window => this;

    public IUserControl ICon => _nowControl;

    public BaseModel Model => (DataContext as BaseModel)!;

    public AllControl()
    {
        InitializeComponent();

        DataContext = new BaseModel("AllControl");

        if (SystemInfo.Os == OsType.Linux)
        {
            ResizeButton.IsVisible = true;
        }

        Model.AddBackCall(Back);

        PointerPressed += AllControl_PointerPressed;
        ResizeButton.AddHandler(PointerPressedEvent, ResizeButton_PointerPressed, RoutingStrategies.Tunnel);

        App.PicUpdate += Update;

        Update();
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
    }

    public void Closed()
    {
        Controls.Content = null;
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
            Controls.Content = con1;
            Dispatcher.UIThread.Post(() =>
            {
                _baseControl.Opened();
            });
        }
        else
        {
            var con1 = Controls.Content;
            var con2 = (con as Control)!;
            Controls.Content = null;
            if (con1 is not (MainControl or CustomControl) && con1 is Control con3)
            {
                controls.Add(con3);
            }
            Controls.Content = con2;
            App.CrossFade300.Start(null, con2);

            con.Opened();
        }

        _nowControl = con;
        SetTitle(_nowControl.Title);
        SetIcon(_nowControl.GetIcon());

        ButtonUp();
    }

    public void Active(IUserControl con)
    {
        var con1 = (con as Control)!;

        controls.Remove(con1);
        var con2 = Controls.Content as Control;
        controls.Add(con2!);
        Controls.Content = con1;

        _nowControl = con;
        SetTitle(_nowControl.Title);
        SetIcon(_nowControl.GetIcon());

        ButtonUp();
    }

    public async void Close(IUserControl con)
    {
        var res = await con.Closing();
        if (res)
        {
            return;
        }

        var con1 = Controls.Content as Control;
        var con2 = (con as Control)!;
        if (con1 == con2)
        {
            Controls.Content = null;
        }
        controls.Remove(con2);
        if (Controls.Content == null)
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
            Controls.Content = con1;
        }

        SetTitle(_nowControl.Title);
        SetIcon(_nowControl.GetIcon());

        ((con as UserControl)?.DataContext as TopModel)?.TopClose();
        con.Closed();

        ButtonUp();
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

    private void ButtonUp()
    {
        if (SystemInfo.Os == OsType.Android)
        {
            return;
        }
        if (controls.Count > 0 || _nowControl is not (MainControl or CustomControl))
        {
            Model.HeadBackDisplay = true;
        }
        else
        {
            Model.HeadBackDisplay = false;
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
}
