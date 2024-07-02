using System;
using System.ComponentModel;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Windows;

namespace ColorMC.Gui.UI.Controls;

public abstract partial class MenuControl : BaseUserControl
{
    private CancellationTokenSource _cancel = new();
    private CancellationTokenSource _cancel1 = new();

    private bool _switch1 = false;

    private readonly BaseMenuControl _control;
    private readonly MenuSideControl _sideControl;

    private int _now = -1;

    public MenuControl()
    {
        _control = new();
        _sideControl = new();

        DataContextChanged += MenuControl_DataContextChanged;
        SizeChanged += MenuControl_SizeChanged;
        
        _control.SidePanel3.PointerPressed += SidePanel2_PointerPressed;
        
        Content = _control;
    }

    private void SidePanel2_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var model = (DataContext as MenuModel)!;
        model.CloseSide();
    }

    protected abstract Control ViewChange(bool iswhell, int old, int index);

    private void MenuControl_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        var model = (DataContext as MenuModel)!;
        if (e.NewSize.Width > 500)
        {
            model.TopSide = false;
            _control.SidePanel2.Child = null;
            _control.SidePanel1.Child = _sideControl;
        }
        else
        {
            model.TopSide = true;
            _control.SidePanel1.Child = null;
            _control.SidePanel2.Child = _sideControl;
        }
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == MenuModel.SideOpen)
        {
            _cancel1.Cancel();
            _cancel1.Dispose();
            _cancel1 = new();

            _control.SidePanel3.IsVisible = true;
            Dispatcher.UIThread.Post(() =>
            {
                App.SidePageSlide300.Start(null, _control.SidePanel2, _cancel1.Token);
            });
        }
        else if (e.PropertyName == MenuModel.SideClose)
        {
            _cancel1.Cancel();
            _cancel1.Dispose();
            _cancel1 = new();
            App.SidePageSlide300.Start(_control.SidePanel2, null, _cancel1.Token);
            _control.SidePanel3.IsVisible = false;
        }

        if (e.PropertyName == MenuModel.NowViewName)
        {
            var model = (DataContext as MenuModel)!;
            Go(ViewChange(model.IsWhell, _now, model.NowView));
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

    private void MenuControl_DataContextChanged(object? sender, EventArgs e)
    {
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
            _ = App.PageSlide500.Start(Content1, Content2, dir1, token);
        }
        else
        {
            Content1.Child = control;
            _ = App.PageSlide500.Start(Content2, Content1, dir1, token);
        }
    }
}
