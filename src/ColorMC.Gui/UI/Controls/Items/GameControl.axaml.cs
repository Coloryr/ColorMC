using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using ColorMC.Gui.UI.Animations;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.Items;

/// <summary>
/// 游戏实例子项目
/// </summary>
public partial class GameControl : UserControl
{
    /// <summary>
    /// 是否处于按下状态
    /// </summary>
    private bool press;
    /// <summary>
    /// 按下的位置
    /// </summary>
    private Point point;

    public GameControl()
    {
        InitializeComponent();

        PointerEntered += GameControl_PointerEntered;
        PointerExited += GameControl_PointerExited;

        AddHandler(PointerPressedEvent, GameControl_PointerPressed, RoutingStrategies.Tunnel);
        AddHandler(PointerReleasedEvent, GameControl_PointerReleased, RoutingStrategies.Tunnel);

        PointerMoved += GameControl_PointerMoved;
        DoubleTapped += GameControl_DoubleTapped;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (DataContext is GameItemModel model)
        {
            if (model.Opa != 1.0)
            {
                return;
            }
        }

        if (GuiConfigUtils.Config.Style.EnableAm)
        {
            Dispatcher.UIThread.Post(() =>
            {
                ItemAnimation.Make().RunAsync(this);
            });
        }
    }

    private void GameModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GameItemModel.IsDrop) && DataContext is GameItemModel model)
        {
            if (model.IsDrop == true)
            {
                RenderTransform = new ScaleTransform(0.95, 0.95);
            }
            else
            {
                RenderTransform = null;
            }
        }
    }

    private void GameControl_PointerExited(object? sender, PointerEventArgs e)
    {
        if (DataContext is GameItemModel model)
        {
            if (model.OneGame)
            {
                return;
            }
            model.IsOver = false;
        }
    }

    private void GameControl_PointerEntered(object? sender, PointerEventArgs e)
    {
        if (DataContext is GameItemModel model)
        {
            if (!model.OneGame)
            {
                model.IsOver = true;
            }

            model.SetTips();
        }
    }

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
            if (Math.Sqrt(Math.Pow(Math.Abs(pos.X - point.X), 2) + Math.Pow(Math.Abs(pos.Y - point.Y), 2)) > 40)
            {
                var top = TopLevel.GetTopLevel(this);
                if (top != null)
                {
                    model.Move(top, e);
                }

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

            var pro = e.GetCurrentPoint(this);
            if (model.ShowCheck)
            {
                if (pro.Properties.IsLeftButtonPressed)
                {
                    e.Handled = true;
                    model.IsCheck = !model.IsCheck;
                }
                return;
            }
            var select = model.IsSelect;
            model.SetSelect();

            point = pro.Position;

            if (pro.Properties.IsRightButtonPressed)
            {
                e.Handled = true;
                Flyout((sender as Control)!);
            }
        }
    }

    private void Flyout(Control control)
    {
        if (DataContext is GameItemModel model)
        {
            model.Flyout(control);
        }
    }
}
