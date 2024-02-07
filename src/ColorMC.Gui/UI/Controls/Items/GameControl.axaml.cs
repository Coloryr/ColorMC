using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Model.Items;
using System;
using System.ComponentModel;

namespace ColorMC.Gui.UI.Controls.Items;

public partial class GameControl : UserControl
{
    private bool press;

    public GameControl()
    {
        InitializeComponent();

        PointerEntered += GameControl_PointerEntered;
        PointerExited += GameControl_PointerExited;

        AddHandler(PointerPressedEvent, GameControl_PointerPressed, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        AddHandler(PointerReleasedEvent, GameControl_PointerReleased, Avalonia.Interactivity.RoutingStrategies.Tunnel);

        PointerMoved += GameControl_PointerMoved;
        DoubleTapped += GameControl_DoubleTapped;
    }

    private void GameModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "IsDrop" && DataContext is GameItemModel model)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (model.IsDrop == true)
                {
                    RenderTransform = new ScaleTransform(0.95, 0.95);
                }
                else
                {
                    RenderTransform = null;
                }
            });
        }
    }

    private void GameControl_PointerExited(object? sender, PointerEventArgs e)
    {
        if (DataContext is GameItemModel model)
        {
            model.IsOver = false;
        }
    }

    private void GameControl_PointerEntered(object? sender, PointerEventArgs e)
    {
        if (DataContext is GameItemModel model)
        {
            model.IsOver = true;
            model.SetTips();
        }
    }

    private Point point;

    private void GameControl_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (!press)
        {
            return;
        }
        
        var pro = e.GetCurrentPoint(this);
        if (pro.Properties.IsLeftButtonPressed && DataContext is GameItemModel model && !model.IsNew)
        {
            var pos = pro.Position;
            if (Math.Sqrt(Math.Pow(Math.Abs(pos.X - point.X), 2) + Math.Pow(Math.Abs(pos.Y - point.Y), 2)) > 30)
            {
                LongPressed.Cancel();
                model.Move(e);
                e.Handled = true;
            }
        }
    }

    private void GameControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        press = false;

        if (DataContext is GameItemModel model)
        {
            model.IsDrop = false;
        }

        LongPressed.Released();
    }

    private void GameControl_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is GameItemModel model)
        {
            e.Handled = true;
            model.Launch();
        }
    }

    private void GameControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        press = true;
        if (DataContext is GameItemModel model)
        {
            if (model.IsNew)
            {
                return;
            }
            var select = model.IsSelect;
            model.SetSelect();

            var pro = e.GetCurrentPoint(this);
            point = pro.Position;

            if (pro.Properties.IsRightButtonPressed)
            {
                e.Handled = true;
                Flyout((sender as Control)!);
            }
            else
            {
                if (SystemInfo.Os == OsType.Android && !select)
                {
                    return;
                }

                e.Handled = true;
                LongPressed.Pressed(() => Flyout((sender as Control)!));
            }
        }
    }

    private void Flyout(Control control)
    {
        if (DataContext is GameItemModel model)
        {
            Dispatcher.UIThread.Post(() =>
            {
                model.Flyout(control);
            });
        }
    }
}
