using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Controls.Main;
using ColorMC.Gui.UI.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Windows;

public partial class SingleControl : UserControl, IBaseWindow, ITopWindow
{
    private BaseUserControl _baseControl;
    private BaseUserControl _nowControl;

    private readonly List<Control> controls = [];

    public IBaseWindow Window => this;

    public BaseUserControl ICon => _nowControl;

    public BaseModel Model => (DataContext as BaseModel)!;

    public string WindowId { get; init; }

    private WindowNotificationManager windowNotification;

    public SingleControl()
    {
        InitializeComponent();

        WindowId = ToString() ?? "SingleControl";

        var model = new BaseModel("AllControl");
        model.PropertyChanged += Model_PropertyChanged;
        DataContext = model;

        PointerPressed += AllControl_PointerPressed;
        PointerReleased += AllControl_PointerReleased;

        ImageManager.BGUpdate += PicUpdate;

        PicUpdate();
    }

    public Task<bool> OnKeyDown(object? sender, KeyEventArgs e)
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
        if (e.PropertyName == BaseModel.NameInfoShow)
        {
            windowNotification.Show(new TextBlock()
            {
                Margin = new Thickness(20, 0, 20, 0),
                TextWrapping = TextWrapping.Wrap,
                Text = Model.NotifyText
            });
        }
        else if (e.PropertyName == BaseModel.NameGetTopLevel
            && DataContext is BaseModel model)
        {
            model.SetTopLevel(TopLevel.GetTopLevel(this));
        }
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

    public void WindowOpened()
    {
        PicUpdate();
    }

    public void Add(BaseUserControl con)
    {
        if (_baseControl == null)
        {
            _baseControl = con;
            var con1 = (_baseControl as Control)!;
            Controls.Child = con1;
            Dispatcher.UIThread.Post(() =>
            {
                _baseControl.WindowOpened();
            });
        }
        else
        {
            var con1 = Controls.Child;
            var con2 = (con as Control)!;
            Controls.Child = null;
            if (con1 is { } con3)
            {
                controls.Add(con3);
            }
            Controls.Child = con2;
            ThemeManager.CrossFade.Start(null, con2);

            Model.PushBack(Back);
            con.WindowOpened();
        }

        _nowControl = con;
        SetTitle(_nowControl.Title);
        SetIcon(_nowControl.GetIcon());
    }

    public void Active(BaseUserControl con)
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

    public async void Close(BaseUserControl con)
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
                _nowControl = (con1 as BaseUserControl)!;
            }
            else
            {
                con1 = _baseControl;
                _nowControl = _baseControl;
            }
            Controls.Child = con1;
        }

        SetTitle(_nowControl.Title);
        SetIcon(_nowControl.GetIcon());

        ((con as UserControl)?.DataContext as TopModel)?.Close();
        con.Closed();

        Model.PopBack();

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

    private void PicUpdate()
    {
        WindowManager.UpdateWindow(Model);
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
        if (_nowControl is MainControl)
            return false;
        if (_nowControl is not BaseUserControl now)
            return false;

        return await now.Closing();
    }

    public void SetIcon(Bitmap icon)
    {
        Model.SetIcon(icon);
    }

    public void WindowStateChange(WindowState windowState)
    {
        ICon?.WindowStateChange(windowState);
        Head.WindowStateChange(windowState);
    }

    public void SetTopView(Control control)
    {
        TopView.Child = control;
    }

    public void BackToBottom()
    {
        TopView.IsVisible = false;
        BottomView.IsVisible = true;
    }

    public void BackToTop()
    {
        TopView.IsVisible = true;
        BottomView.IsVisible = false;
    }

    public void ReloadIcon()
    {
        Model.SetIcon(_nowControl.GetIcon());
        if (VisualRoot is Window window)
        {
            window.Icon = ImageManager.WindowIcon;
        }
    }
}
