using System;
using System.ComponentModel;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;

namespace ColorMC.Gui.UI.Controls;

/// <summary>
/// 带有左侧菜单的窗口
/// </summary>
public abstract partial class MenuControl : BaseUserControl
{
    private CancellationTokenSource _cancel = new();

    private bool _switch1 = false;

    private readonly BaseMenuControl _control;
    private readonly MenuSideControl _sideControl;

    private int _now = -1;

    public MenuControl(string usename) : base(usename)
    {
        _control = new();
        _sideControl = new();

        SizeChanged += MenuControl_SizeChanged;

        _control.SidePanel3.PointerPressed += SidePanel2_PointerPressed;

        Content = _control;
    }

    private void SidePanel2_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var model = (DataContext as MenuModel)!;
        model.CloseSide();
    }

    protected abstract Control ViewChange(int old, int index);

    private void MenuControl_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        var model = (DataContext as MenuModel)!;
        if (e.NewSize.Width > 500)
        {
            model.TopSide = false;
            _control.SidePanel2.Child = null;
            _control.SidePanel1.Child = _sideControl;
            _control.TopPanel.Margin = new Thickness(0);
        }
        else
        {
            model.TopSide = true;
            _control.SidePanel1.Child = null;
            _control.SidePanel2.Child = _sideControl;
            _control.TopPanel.Margin = new Thickness(10, 0, 0, 0);
        }
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == MenuModel.NameSideOpen)
        {
            _control.SidePanel3.IsVisible = true;
            _control.SidePanel2.IsVisible = true;
        }
        else if (e.PropertyName == MenuModel.NameSideClose)
        {
            _control.SidePanel2.IsVisible = false;
            _control.SidePanel3.IsVisible = false;
        }

        if (e.PropertyName == MenuModel.NameNowView)
        {
            var model = (DataContext as MenuModel)!;
            Go(ViewChange(_now, model.NowView));
            _now = model.NowView;
        }
    }

    private void Go(Control to)
    {
        _cancel.Cancel();
        _cancel.Dispose();

        _cancel = new();

        var model = (DataContext as MenuModel)!;

        if (_now == -1)
        {
            _control.Content1.Child = to;
            return;
        }

        _control.SwitchTo(_switch1, to, _now < model.NowView, _cancel.Token);

        _switch1 = !_switch1;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is MenuModel model)
        {
            model.PropertyChanged += Model_PropertyChanged;
        }
    }
}

public partial class BaseMenuControl : UserControl
{
    public BaseMenuControl()
    {
        InitializeComponent();
    }

    public void SwitchTo(bool dir, Control control, bool dir1, CancellationToken token)
    {
        if (!dir)
        {
            Content2.Child = control;
            _ = ThemeManager.SelfPageSlideY.Start(Content1, Content2, dir1, token);
        }
        else
        {
            Content1.Child = control;
            _ = ThemeManager.SelfPageSlideY.Start(Content2, Content1, dir1, token);
        }
    }
}
