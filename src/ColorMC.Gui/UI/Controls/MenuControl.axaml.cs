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
    private bool _switch1 = false;

    private CancellationTokenSource _cancel = new();
    private CancellationTokenSource _cancel1 = new();

    private int _now = -1;

    public MenuControl()
    {
        InitializeComponent();

        StackPanel1.PointerPressed += StackPanel1_PointerPressed;
        StackPanel2.PointerPressed += StackPanel2_PointerPressed;

        DataContextChanged += MenuControl_DataContextChanged;
    }

    private void MenuControl_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MenuModel model)
        {
            model.PropertyChanged += Model_PropertyChanged;
        }
    }

    protected abstract Control ViewChange(bool iswhell, int old, int index);

    private void StackPanel2_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as MenuModel)!.OpenSide();
    }

    private void StackPanel1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as MenuModel)!.CloseSide();
    }

    private void Go(Control to)
    {
        _cancel.Cancel();
        _cancel.Dispose();

        _cancel = new();

        var model = (DataContext as MenuModel)!;

        if (_now == -1)
        {
            Content1.Child = to;
            return;
        }

        if (!_switch1)
        {
            Content2.Child = to;
            _ = App.PageSlide500.Start(Content1, Content2, _now < model.NowView, _cancel.Token);
        }
        else
        {
            Content1.Child = to;
            _ = App.PageSlide500.Start(Content2, Content1, _now < model.NowView, _cancel.Token);
        }

        _switch1 = !_switch1;
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == MenuModel.SideOpen)
        {
            _cancel1.Cancel();
            _cancel1.Dispose();
            _cancel1 = new();

            StackPanel1.IsVisible = true;
            Dispatcher.UIThread.Post(() =>
            {
                App.SidePageSlide300.Start(null, DockPanel1, _cancel1.Token);
            });
        }
        else if (e.PropertyName == MenuModel.SideClose)
        {
            _cancel1.Cancel();
            _cancel1.Dispose();
            _cancel1 = new();
            App.SidePageSlide300.Start(DockPanel1, null, _cancel1.Token);
            StackPanel1.IsVisible = false;
        }
        else if (e.PropertyName == MenuModel.NowViewName)
        {
            var model = (DataContext as MenuModel)!;
            Go(ViewChange(model.IsWhell, _now, model.NowView));
            _now = model.NowView;
        }
    }
}
