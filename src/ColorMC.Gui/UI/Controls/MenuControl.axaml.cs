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
    
    private readonly MenuSideControl _sideControl;

    private int _now = -1;

    public MenuControl(string usename) : base(usename)
    {
        InitializeComponent();

        _sideControl = new();

        DataContextChanged += MenuControl_DataContextChanged;
        SizeChanged += MenuControl_SizeChanged;

        SidePanel3.PointerPressed += SidePanel2_PointerPressed;
        TabPanel.SizeChanged += TabPanel_SizeChanged;
    }

    private void TabPanel_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (DataContext is TopModel model)
        {
            model.WidthChange(0, e.NewSize.Width);
        }
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
            SidePanel2.Child = null;
            SidePanel1.Child = _sideControl;
            TopPanel.Margin = new Thickness(0);
        }
        else
        {
            model.TopSide = true;
            SidePanel1.Child = null;
            SidePanel2.Child = _sideControl;
            TopPanel.Margin = new Thickness(10, 0, 0, 0);
        }
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == MenuModel.NameSideOpen)
        {
            SidePanel3.IsVisible = true;
            SidePanel2.IsVisible = true;
        }
        else if (e.PropertyName == MenuModel.NameSideClose)
        {
            SidePanel2.IsVisible = false;
            SidePanel3.IsVisible = false;
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
            TabPanel.Content1.Child = to;
            return;
        }

        TabPanel.SwitchTo(_switch1, to, _now < model.NowView, _cancel.Token);

        _switch1 = !_switch1;
    }

    private void MenuControl_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MenuModel model)
        {
            model.PropertyChanged += Model_PropertyChanged;
        }
    }
}
