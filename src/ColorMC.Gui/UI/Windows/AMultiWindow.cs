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
using ColorMC.Gui.UI.Controls.Error;
using ColorMC.Gui.UI.Model;
using System;
using System.ComponentModel;

namespace ColorMC.Gui.UI.Windows;

/// <summary>
/// 基础多窗口
/// </summary>
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

        var model = new BaseModel(con.WindowId);
        model.PropertyChanged += Model_PropertyChanged;
        DataContext = model;
        _con = con;

        con.SetBaseModel(model);

        if (_con is UserControl con1)
        {
            SetChild(con1);
        }

        Closed += AMultiWindow_Closed;
        Activated += AMultiWindow_Activated;
        Closing += AMultiWindow_Closing;
        PropertyChanged += AMultiWindow_PropertyChanged;
        PointerReleased += AMultiWindow_PointerReleased;
        PointerPressed += AMultiWindow_PointerPressed;

        ImageManager.BGUpdate += AMultiWindow_PicUpdate;

        if (con is ErrorControl)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
        else
        {
            FindGoodPos();
        }

        AMultiWindow_PicUpdate();
    }

    protected abstract void SetChild(Control control);

    private void AMultiWindow_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        ICon.IPointerPressed(e);
    }

    private void AMultiWindow_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        ICon.IPointerReleased(e);
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (ColorMCGui.IsClose)
        {
            return;
        }
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

    private void AMultiWindow_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == WindowStateProperty)
        {
            Head.WindowStateChange(WindowState);
        }
    }

    private void AMultiWindow_Closing(object? sender, WindowClosingEventArgs e)
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
        if (SetWindowState())
        {
            return;
        }

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

    private void AMultiWindow_Closed(object? sender, EventArgs e)
    {
        WindowManager.ClosedWindow(this);

        ImageManager.BGUpdate -= AMultiWindow_PicUpdate;

        ((ICon as UserControl)?.DataContext as TopModel)?.Close();
        DataContext = null;
        ICon.Closed();

        App.Clear();
        App.TestClose();
    }

    private void AMultiWindow_Activated(object? sender, EventArgs e)
    {
        WindowManager.ActivatedWindow(this);
    }

    private void AMultiWindow_PicUpdate()
    {
        WindowManager.UpdateWindow(Model);

        ICon.Update();
    }

    public void SetIcon(Bitmap icon)
    {
        Model.SetIcon(icon);
    }

    //public void SetSize(int width, int height)
    //{
    //    Width = width;
    //    Height = height;
    //}

    public void ReloadIcon()
    {
        Model.SetIcon(_con.GetIcon());
        Icon = ImageManager.WindowIcon;
    }
}
