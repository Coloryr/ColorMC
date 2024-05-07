using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.Error;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Windows;

public partial class SelfBaseWindow : Window, IBaseWindow
{
    private WindowNotificationManager windowNotification;

    public IUserControl ICon { get; set; }

    public BaseModel Model => (DataContext as BaseModel)!;

    private bool _isClose;

    public SelfBaseWindow()
    {
        InitializeComponent();
    }

    public SelfBaseWindow(IUserControl con)
    {
        InitializeComponent();

        DataContext = new BaseModel(con.UseName);

        ICon = con;

        if (SystemInfo.Os == OsType.Linux)
        {
            ResizeButton.IsVisible = true;
            SystemDecorations = SystemDecorations.BorderOnly;
        }

        Icon = App.Icon;

        if (ICon is UserControl con1)
        {
            MainControl.Child = con1;
        }

        AddHandler(KeyDownEvent, Window_KeyDown, RoutingStrategies.Tunnel);

        Closed += UserWindow_Closed;
        Opened += UserWindow_Opened;
        Activated += Window_Activated;
        Closing += SelfBaseWindow_Closing;
        PropertyChanged += SelfBaseWindow_PropertyChanged;
        ResizeButton.AddHandler(PointerPressedEvent, ResizeButton_PointerPressed, RoutingStrategies.Tunnel);
        Model.PropertyChanged += Model_PropertyChanged;
        PointerReleased += SelfBaseWindow_PointerReleased;
        PointerPressed += SelfBaseWindow_PointerPressed;

        App.PicUpdate += Update;

        if (con is ErrorControl)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
        else
        {
            FindGoodPos();
        }

        Update();
    }

    private void SelfBaseWindow_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        ICon.IPointerPressed(e);
    }

    private void SelfBaseWindow_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        ICon.IPointerReleased(e);
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == BaseModel.InfoName)
        {
            windowNotification.Show(Model.NotifyText);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        windowNotification = new WindowNotificationManager(this)
        {
            Position = NotificationPosition.TopRight,
            MaxItems = 3,
            Margin = new(0, 30, 0, 0)
        };
    }

    private void ResizeButton_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        e.Handled = true;
        BeginResizeDrag(WindowEdge.SouthEast, e);
    }

    private void SelfBaseWindow_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == WindowStateProperty)
        {
            ICon.WindowStateChange(WindowState);
            Head.WindowStateChange(WindowState);
            if (SystemInfo.Os == OsType.Windows)
            {
                if (WindowState == WindowState.Maximized)
                {
                    Padding = new Thickness(8);
                }
                else
                {
                    Padding = new Thickness(0);
                }
            }
        }
    }

    private void SelfBaseWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (ICon == null || _isClose == true)
        {
            return;
        }

        e.Cancel = true;

        Dispatcher.UIThread.Post(async () =>
        {
            var res = await ICon.Closing();
            if (!res)
            {
                _isClose = true;
                Close();
            }
        });
    }

    public void SetTitle(string temp)
    {
        Model.Title = temp;
    }

    private void FindGoodPos()
    {
        var basewindow = App.LastWindow;

        if (basewindow == null || basewindow.WindowState == WindowState.Minimized)
            return;

        var pos = basewindow.Position;
        var sec = basewindow.Screens.ScreenFromWindow(basewindow);
        if (sec == null)
        {
            return;
        }
        var area = sec.WorkingArea;
        int x, y;
        if (pos.X > area.Width / 2)
        {
            x = pos.X - 100;
        }
        else
        {
            x = pos.X + 100;
        }

        if (pos.Y > area.Height / 2)
        {
            y = pos.Y - 40;
        }
        else
        {
            y = pos.Y + 40;
        }

        Position = new(x, y);
    }

    private async void Window_KeyDown(object? sender, KeyEventArgs e)
    {
        if (await ICon.OnKeyDown(sender, e))
        {
            e.Handled = true;
            return;
        }

        if (SystemInfo.Os == OsType.MacOS && e.KeyModifiers == KeyModifiers.Control)
        {
            switch (e.Key)
            {
                case Key.OemComma:
                    App.ShowSetting(SettingType.Normal);
                    break;
                case Key.Q:
                    App.Close();
                    break;
                case Key.M:
                    WindowState = WindowState.Minimized;
                    break;
                case Key.W:
                    Close();
                    break;
            }
        }
    }

    private void UserWindow_Opened(object? sender, EventArgs e)
    {
        ICon.Opened();
    }

    private void UserWindow_Closed(object? sender, EventArgs e)
    {
        App.PicUpdate -= Update;

        ((ICon as UserControl)?.DataContext as TopModel)?.TopClose();
        DataContext = null;
        ICon.Closed();

        if (App.LastWindow == this)
        {
            App.LastWindow = null;
        }
        if (App.TopLevel == this)
        {
            if (ConfigBinding.WindowMode())
            {
                App.TopLevel = GetTopLevel(App.AllWindow);
            }
            else
            {
                var win = App.GetMainWindow();
                App.TopLevel = win as Window;
            }
        }

        App.Clear();
        FuntionUtils.RunGC();
    }

    private void Window_Activated(object? sender, EventArgs e)
    {
        App.LastWindow = this;
        App.TopLevel = this;
    }

    private void Update()
    {
        App.UpdateWindow(Model);

        ICon.Update();
    }

    public void SetIcon(Bitmap icon)
    {
        Model.SetIcon(icon);
    }

    public void SetSize(int width, int height)
    {
        Width = width;
        Height = height;
    }
}
