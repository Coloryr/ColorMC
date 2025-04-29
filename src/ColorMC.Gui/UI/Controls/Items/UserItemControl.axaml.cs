using System;
using System.ComponentModel;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Media.Transformation;
using Avalonia.Styling;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.Items;

/// <summary>
/// 用户子项目
/// </summary>
public partial class UserItemControl : UserControl
{
    private Point _last;
    private bool _press;
    private double _now;
    private double _now1;

    private CancellationTokenSource _cancel;

    public UserItemControl()
    {
        InitializeComponent();

        DataContextChanged += UserItemControl_DataContextChanged;

        DoubleTapped += UserItemControl_DoubleTapped;
        PointerPressed += UserItemControl_PointerPressed;
        PointerReleased += UserItemControl_PointerReleased;

        Border1.PointerPressed += Border1_PointerPressed;
        Border1.PointerReleased += Border1_PointerReleased;
    }

    private void UserItemControl_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is UserDisplayModel model)
        {
            model.PropertyChanged += Model_PropertyChanged;
        }
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == UserDisplayModel.NameReload)
        {
            _cancel?.Cancel();
            _cancel?.Dispose();
            _cancel = new();
            if (Border1.RenderTransform is TransformGroup group)
            {
                foreach (var item in group.Children)
                {
                    if (item is RotateTransform rotate)
                    {
                        rotate.Angle = 0;
                    }
                    else if (item is Rotate3DTransform rotate3d)
                    {
                        rotate3d.AngleX = 0;
                        rotate3d.AngleY = 0;
                    }
                }
            }
        }
    }

    private void Border1_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var p = e.GetCurrentPoint(this);
        if (p.Properties.IsLeftButtonPressed == false && _press)
        {
            _press = false;
            var temp = p.Position - _last;
            _last = new();
            Rote(temp);
        }
    }

    private void Border1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var p = e.GetCurrentPoint(this);
        if (p.Properties.IsLeftButtonPressed)
        {
            _last = p.Position;
            _press = true;
        }
    }

    private void UserItemControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        LongPressed.Released();
    }

    private void UserItemControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var model = (DataContext as UserDisplayModel)!;

        var pro = e.GetCurrentPoint(this);

        if (pro.Properties.IsRightButtonPressed)
        {
            Flyout((sender as Control)!);
        }
        else
        {
            LongPressed.Pressed(() => Flyout((sender as Control)!));
        }
    }

    private void UserItemControl_DoubleTapped(object? sender, TappedEventArgs e)
    {
        (DataContext as UserDisplayModel)?.Select();
    }

    private void Flyout(Control control)
    {
        var model = (DataContext as UserDisplayModel)!;
        _ = new UserFlyout(control, model);
    }

    private void Rote(Point point)
    {
        if (GuiConfigUtils.Config.Head.Type is HeadType.Head3D_A or HeadType.Head3D_B)
        {
            if (Math.Abs(point.X) > 50)
            {
                var temp = point.X * 5;
                var ani = new Animation()
                {
                    FillMode = FillMode.Forward,
                    Easing = new CubicEaseOut(),
                    Duration = TimeSpan.FromSeconds(3),
                    Children =
                    {
                        new KeyFrame()
                        {
                            Setters =
                            {
                                new Setter(RotateTransform.AngleProperty, _now)
                            },
                            Cue = new Cue(0d)
                        },
                        new KeyFrame()
                        {
                            Setters =
                            {
                                new Setter(RotateTransform.AngleProperty, temp)
                            },
                            Cue = new Cue(1d)
                        }
                    }
                };

                _cancel?.Cancel();
                _cancel?.Dispose();
                _cancel = new();
                ani.RunAsync(Border1, _cancel.Token);
                _now = temp % 360;
            }
        }
        else
        {
            if (Math.Abs(point.X) > 50 && Math.Abs(point.Y) < 50)
            {
                var temp = point.X * 5;
                if (temp % 90 == 0)
                {
                    temp += 10;
                }
                var ani = new Animation()
                {
                    FillMode = FillMode.Forward,
                    Easing = new CubicEaseOut(),
                    Duration = TimeSpan.FromSeconds(3),
                    Children =
                    {
                        new KeyFrame()
                        {
                            Setters =
                            {
                                new Setter(Rotate3DTransform.AngleYProperty, _now)
                            },
                            Cue = new Cue(0d)
                        },
                        new KeyFrame()
                        {
                            Setters =
                            {
                                new Setter(Rotate3DTransform.AngleYProperty, temp)
                            },
                            Cue = new Cue(1d)
                        }
                    }
                };

                _cancel?.Cancel();
                _cancel?.Dispose();
                _cancel = new();
                ani.RunAsync(Border1, _cancel.Token);
                _now = temp % 360;
            }
            else if (Math.Abs(point.X) < 50 && Math.Abs(point.Y) > 50)
            {
                var temp = point.Y * 5;
                if (temp % 90 == 0)
                {
                    temp += 10;
                }
                var ani = new Animation()
                {
                    FillMode = FillMode.Forward,
                    Easing = new CubicEaseOut(),
                    Duration = TimeSpan.FromSeconds(3),
                    Children =
                    {
                        new KeyFrame()
                        {
                            Setters =
                            {
                                new Setter(Rotate3DTransform.AngleXProperty, _now1)
                            },
                            Cue = new Cue(0d)
                        },
                        new KeyFrame()
                        {
                            Setters =
                            {
                                new Setter(Rotate3DTransform.AngleXProperty, temp)
                            },
                            Cue = new Cue(1d)
                        }
                    }
                };

                _cancel?.Cancel();
                _cancel?.Dispose();
                _cancel = new();
                ani.RunAsync(Border1, _cancel.Token);
                _now1 = temp % 360;
            }
            else if (Math.Abs(point.X) > 50 && Math.Abs(point.Y) > 50)
            {
                var temp = point.X * 5;
                var temp1 = point.Y * 5;
                if (temp % 90 == 0)
                {
                    temp += 10;
                }
                if (temp1 % 90 == 0)
                {
                    temp1 += 10;
                }
                var ani = new Animation()
                {
                    FillMode = FillMode.Forward,
                    Easing = new CubicEaseOut(),
                    Duration = TimeSpan.FromSeconds(3),
                    Children =
                    {
                        new KeyFrame()
                        {
                            Setters =
                            {
                                new Setter(Rotate3DTransform.AngleYProperty, _now),
                                new Setter(Rotate3DTransform.AngleXProperty, _now1)
                            },
                            Cue = new Cue(0d)
                        },
                        new KeyFrame()
                        {
                            Setters =
                            {
                                new Setter(Rotate3DTransform.AngleYProperty, temp),
                                new Setter(Rotate3DTransform.AngleXProperty, temp1)
                            },
                            Cue = new Cue(1d)
                        }
                    }
                };

                _cancel?.Cancel();
                _cancel?.Dispose();
                _cancel = new();
                ani.RunAsync(Border1, _cancel.Token);
                _now = temp % 360;
                _now1 = temp1 % 360;
            }
        }
    }
}
