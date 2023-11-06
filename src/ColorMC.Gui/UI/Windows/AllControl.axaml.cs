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

        DataContext = new BaseModel();

        if (SystemInfo.Os == OsType.Linux)
        {
            ResizeButton.IsVisible = true;
        }

        if (SystemInfo.Os == OsType.Android)
        {
            Model.HeadBackDisplay = false;
            Model.HeadDownDisplay = false;
        }
        else
        {
            Model.AddBack(Back);
        }

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
            Back();
        }
    }

    public void Closed()
    {
        Controls.Children.Clear();
    }

    public void Opened()
    {
        Update();
    }

    public void Back()
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

    public void Add(IUserControl con)
    {
        if (_baseControl == null)
        {
            _baseControl = con;
            var con1 = (_baseControl as Control)!;
            Controls.Children.Add(con1);
            Dispatcher.UIThread.Post(() =>
            {
                _baseControl.Opened();
            });
        }
        else
        {
            var con1 = Controls.Children[0];
            var con2 = (con as Control)!;
            Controls.Children.Remove(con1);
            if (con1 is not (MainControl or CustomControl))
            {
                controls.Add(con1);
            }
            Controls.Children.Add(con2);
            App.CrossFade300.Start(null, con2);

            con.Opened();
        }

        _nowControl = con;
        SetTitle(_nowControl.Title);
        SetIcon(_nowControl.GetIcon());

        ButtonUp();
    }

    private void ButtonUp()
    {
        if (SystemInfo.Os == OsType.Android)
        {
            return;
        }
        Model.HeadDownDisplay = false;
        if (controls.Count > 0 || _nowControl is not (MainControl or CustomControl))
        {
            Model.HeadBackDisplay = true;
        }
        else
        {
            Model.HeadBackDisplay = false;
        }
    }

    public void Active(IUserControl con)
    {

        var con1 = (con as Control)!;
        if (Controls.Children.Contains(con1))
            return;

        controls.Remove(con1);
        var con2 = Controls.Children[0];
        Controls.Children.Clear();
        controls.Add(con2);
        Controls.Children.Add(con1);

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

        var con1 = Controls.Children[0];
        var con2 = (con as Control)!;
        if (con1 == con2)
        {
            Controls.Children.Remove(con1);
        }
        controls.Remove(con2);
        if (Controls.Children.Count == 0)
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
            Controls.Children.Add(con1);
        }

        SetTitle(_nowControl.Title);
        SetIcon(_nowControl.GetIcon());

        ((con as UserControl)?.DataContext as TopModel)?.TopClose();
        con.Closed();

        ButtonUp();
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
