using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Controls.Error;
using ColorMC.Gui.UI.Model;

namespace ColorMC.Gui.UI.Windows;

/// <summary>
/// 基础多窗口
/// </summary>
public abstract class AMultiWindow : ABaseWindow, IBaseWindow
{
    /// <summary>
    /// 右上角通知
    /// </summary>
    private WindowNotificationManager _windowNotification;

    public override BaseUserControl ICon => _con;

    /// <summary>
    /// 基础页面
    /// 真正显示的部分
    /// </summary>
    private BaseUserControl _con;

    /// <summary>
    /// 窗口模型
    /// </summary>
    public WindowModel Model => (DataContext as WindowModel)!;

    /// <summary>
    /// 窗口上方控制栏
    /// </summary>
    public abstract TitleControl Head { get; }

    /// <summary>
    /// 是否关闭
    /// </summary>
    private bool _isClose;

    /// <summary>
    /// 初始化窗口
    /// </summary>
    /// <param name="con"></param>
    protected void InitMultiWindow(BaseUserControl con)
    {
        InitBaseWindow();

        var model = new WindowModel(con.WindowId);
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

        EventManager.BGChange += AMultiWindow_PicUpdate;

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

    /// <summary>
    /// 设置内容
    /// </summary>
    /// <param name="control"></param>
    protected abstract void SetChild(Control control);

    private void AMultiWindow_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        ICon.IPointerPressed(e);
    }

    private void AMultiWindow_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        ICon.IPointerReleased(e);
    }

    /// <summary>
    /// 监听属性
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (ColorMCGui.IsClose)
        {
            return;
        }
        if (e.PropertyName == WindowModel.NameInfoShow)
        {
            //弹出一个提示
            _windowNotification.Show(new TextBlock()
            {
                Margin = new Thickness(20, 0, 20, 0),
                TextWrapping = TextWrapping.Wrap,
                Text = Model.NotifyText
            });
        }
        else if (e.PropertyName == WindowModel.NameGetTopLevel
            && DataContext is WindowModel model)
        {
            model.SetTopLevel(GetTopLevel(this));
        }
        else if (e.PropertyName == nameof(WindowModel.Icon)
            && DataContext is WindowModel model1)
        {
            Icon = new WindowIcon(model1.Icon);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _windowNotification = new WindowNotificationManager(this)
        {
            Position = NotificationPosition.TopRight,
            MaxItems = 3,
            Margin = new(0, 30, 0, 0)
        };
    }

    /// <summary>
    /// 窗口属性
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AMultiWindow_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != WindowStateProperty)
        {
            return;
        }

        if (DataContext is not WindowModel model)
        {
            return;
        }

        if (WindowState == WindowState.Maximized)
        {
            model.MaxIcon = SystemInfo.Os == OsType.MacOS ? ImageManager.MaxMacosIcon[1] : ImageManager.MaxWindowsIcon[1];
        }
        else
        {
            model.MaxIcon = SystemInfo.Os == OsType.MacOS ? ImageManager.MaxMacosIcon[0] :ImageManager.MaxWindowsIcon[0];
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

    /// <summary>
    /// 设置标题
    /// </summary>
    /// <param name="temp"></param>
    public void SetTitle(string temp)
    {
        Model.Title = temp;
    }

    /// <summary>
    /// 查找一个好的位置
    /// </summary>
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

    /// <summary>
    /// 窗口关闭
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AMultiWindow_Closed(object? sender, EventArgs e)
    {
        WindowManager.ClosedWindow(this);

        EventManager.BGChange -= AMultiWindow_PicUpdate;

        ((ICon as UserControl)?.DataContext as ControlModel)?.Close();
        DataContext = null;
        ICon.Closed();

        App.Clear();
        App.TestClose();
    }

    /// <summary>
    /// 窗口最前
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AMultiWindow_Activated(object? sender, EventArgs e)
    {
        WindowManager.ActivatedWindow(this);
    }

    /// <summary>
    /// 更新背景图
    /// </summary>
    private void AMultiWindow_PicUpdate()
    {
        WindowManager.UpdateWindow(Model);

        ICon.Update();
    }

    /// <summary>
    /// 设置图标
    /// </summary>
    /// <param name="icon"></param>
    public void SetIcon(Bitmap icon)
    {
        Model.SetIcon(icon);
    }

    /// <summary>
    /// 重载图标
    /// </summary>
    public void ReloadIcon()
    {
        Model.SetIcon(_con.GetIcon());
        Icon = ImageManager.WindowIcon;
    }
}
