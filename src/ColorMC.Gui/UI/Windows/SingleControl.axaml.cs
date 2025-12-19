using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
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

namespace ColorMC.Gui.UI.Windows;

/// <summary>
/// 单窗口模式主界面
/// </summary>
public partial class SingleControl : UserControl, IBaseWindow, IBaseControl
{
    /// <summary>
    /// 最底层界面
    /// </summary>
    private BaseUserControl _baseControl;
    /// <summary>
    /// 当前层界面
    /// </summary>
    private BaseUserControl _nowControl;

    /// <summary>
    /// 界面列表
    /// </summary>
    private readonly List<Control> controls = [];
    /// <summary>
    /// 当前显示的界面
    /// </summary>
    public BaseUserControl ICon => _nowControl;
    /// <summary>
    /// 基础窗口模型
    /// </summary>
    public WindowModel Model => (DataContext as WindowModel)!;
    /// <summary>
    /// 窗口ID
    /// </summary>
    public string WindowId { get; init; }
    /// <summary>
    /// 窗口右上角提示
    /// </summary>
    private WindowNotificationManager _windowNotification;

    public SingleControl()
    {
        InitializeComponent();

        WindowId = ToString() ?? "SingleControl";

        var model = new WindowModel("AllControl");
        model.PropertyChanged += Model_PropertyChanged;
        DataContext = model;

        PointerPressed += AllControl_PointerPressed;
        PointerReleased += AllControl_PointerReleased;

        EventManager.BGChange += PicUpdate;

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
        _windowNotification = new WindowNotificationManager(level)
        {
            Position = NotificationPosition.TopRight,
            MaxItems = 3,
            Margin = new(0, 30, 0, 0)
        };
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == WindowModel.NameInfoShow)
        {
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

    public void ControlOpened()
    {
        PicUpdate();
    }

    /// <summary>
    /// 添加一个显示页面
    /// </summary>
    /// <param name="con"></param>
    public void Add(BaseUserControl con)
    {
        //是否为底层页面
        if (_baseControl == null)
        {
            _baseControl = con;
            var con1 = (_baseControl as Control)!;
            Controls.Child = con1;
            Dispatcher.UIThread.Post(() =>
            {
                _baseControl.ControlOpened();
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
            con.ControlOpened();
        }

        //设置对应信息
        _nowControl = con;
        SetTitle(_nowControl.Title);
        SetIcon(_nowControl.GetIcon());
    }

    /// <summary>
    /// 让页面显示在最上面
    /// </summary>
    /// <param name="con"></param>
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

    /// <summary>
    /// 关闭一个页面
    /// </summary>
    /// <param name="con"></param>
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

        ((con as UserControl)?.DataContext as ControlModel)?.Close();
        con.Closed();

        Model.PopBack();

        App.Clear();
    }

    /// <summary>
    /// 回到上一个页面
    /// </summary>
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

    /// <summary>
    /// 更新背景图
    /// </summary>
    private void PicUpdate()
    {
        WindowManager.UpdateWindow(Model);
    }
    /// <summary>
    /// 设置标题
    /// </summary>
    /// <param name="data"></param>
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

    /// <summary>
    /// 设置图标
    /// </summary>
    /// <param name="icon"></param>
    public void SetIcon(Bitmap icon)
    {
        Model.SetIcon(icon);
    }

    /// <summary>
    /// 设置窗口状态
    /// </summary>
    /// <param name="windowState"></param>
    public void ControlStateChange(WindowState windowState)
    {
        ICon.ControlStateChange(windowState);
        Head.WindowStateChange(windowState);
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
