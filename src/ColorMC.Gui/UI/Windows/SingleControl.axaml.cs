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
/// ������ģʽ������
/// </summary>
public partial class SingleControl : UserControl, IBaseWindow, IBaseControl
{
    /// <summary>
    /// ��ײ����
    /// </summary>
    private BaseUserControl _baseControl;
    /// <summary>
    /// ��ǰ�����
    /// </summary>
    private BaseUserControl _nowControl;

    /// <summary>
    /// �����б�
    /// </summary>
    private readonly List<Control> controls = [];
    /// <summary>
    /// ��ǰ��ʾ�Ľ���
    /// </summary>
    public BaseUserControl ICon => _nowControl;
    /// <summary>
    /// ��������ģ��
    /// </summary>
    public BaseModel Model => (DataContext as BaseModel)!;
    /// <summary>
    /// ����ID
    /// </summary>
    public string WindowId { get; init; }
    /// <summary>
    /// �������Ͻ���ʾ
    /// </summary>
    private WindowNotificationManager _windowNotification;

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
        _windowNotification = new WindowNotificationManager(level)
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
            _windowNotification.Show(new TextBlock()
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

    public void ControlOpened()
    {
        PicUpdate();
    }

    /// <summary>
    /// ���һ����ʾҳ��
    /// </summary>
    /// <param name="con"></param>
    public void Add(BaseUserControl con)
    {
        //�Ƿ�Ϊ�ײ�ҳ��
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

        //���ö�Ӧ��Ϣ
        _nowControl = con;
        SetTitle(_nowControl.Title);
        SetIcon(_nowControl.GetIcon());
    }

    /// <summary>
    /// ��ҳ����ʾ��������
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
    /// �ر�һ��ҳ��
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

        ((con as UserControl)?.DataContext as TopModel)?.Close();
        con.Closed();

        Model.PopBack();

        App.Clear();
    }

    /// <summary>
    /// �ص���һ��ҳ��
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
    /// ���±���ͼ
    /// </summary>
    private void PicUpdate()
    {
        WindowManager.UpdateWindow(Model);
    }
    /// <summary>
    /// ���ñ���
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
    /// ����ͼ��
    /// </summary>
    /// <param name="icon"></param>
    public void SetIcon(Bitmap icon)
    {
        Model.SetIcon(icon);
    }

    /// <summary>
    /// ���ô���״̬
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
