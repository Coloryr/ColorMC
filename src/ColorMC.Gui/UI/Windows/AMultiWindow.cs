using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Controls.Error;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Windows;

public abstract class AMultiWindow : ABaseWindow, IBaseWindow
{
    private WindowNotificationManager windowNotification;

    public override BaseUserControl ICon => _con;

    private BaseUserControl _con;

    public BaseModel Model => (DataContext as BaseModel)!;

    public abstract HeadControl Head { get; }

    private bool _isClose;

    protected void InitMultiWindow(BaseUserControl con)
    {
        InitBaseWindow();

        var model = new BaseModel(con.UseName);
        model.PropertyChanged += Model_PropertyChanged;
        DataContext = model;
        _con = con;

        if (_con is UserControl con1)
        {
            SetChild(con1);
        }

        Closed += UserWindow_Closed;
        Activated += Window_Activated;
        Closing += SelfBaseWindow_Closing;
        PropertyChanged += SelfBaseWindow_PropertyChanged;
        PointerReleased += SelfBaseWindow_PointerReleased;
        PointerPressed += SelfBaseWindow_PointerPressed;

        ImageManager.PicUpdate += PicUpdate;

        if (con is ErrorControl)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
        else
        {
            FindGoodPos();
        }

        PicUpdate();
    }

    protected abstract void SetChild(Control control);

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
        if (e.PropertyName == BaseModel.InfoShow)
        {
            windowNotification.Show(Model.NotifyText);
        }
        else if (e.PropertyName == BaseModel.GetTopLevelName
            && DataContext is BaseModel model)
        {
            model.SetTopLevel(GetTopLevel(this));
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

    private void SelfBaseWindow_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == WindowStateProperty)
        {
            Head.WindowStateChange(WindowState);
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
        var basewindow = WindowManager.LastWindow;

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

    private void UserWindow_Closed(object? sender, EventArgs e)
    {
        ImageManager.PicUpdate -= PicUpdate;

        ((ICon as UserControl)?.DataContext as TopModel)?.Close();
        DataContext = null;
        ICon.Closed();

        if (WindowManager.LastWindow == this)
        {
            WindowManager.LastWindow = null;
        }
        
        App.Clear();
        App.TestClose();
    }

    private void Window_Activated(object? sender, EventArgs e)
    {
        WindowManager.LastWindow = this;
        //App.TopLevel = this;
    }

    private void PicUpdate()
    {
        WindowManager.UpdateWindow(Model);

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
