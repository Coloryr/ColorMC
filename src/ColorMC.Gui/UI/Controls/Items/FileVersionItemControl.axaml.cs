using System;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Controls.Items;

public partial class FileVersionItemControl : UserControl
{
    public FileVersionItemControl()
    {
        InitializeComponent();

        PointerPressed += FileItemControl_PointerPressed;
        PointerReleased += FileItemControl_PointerReleased;
        DoubleTapped += FileItemControl_DoubleTapped;
        PointerEntered += FileItemControl_PointerEntered;
        PointerExited += FileItemControl_PointerExited;
        PointerMoved += FileItemControl_PointerMoved;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        Dispatcher.UIThread.Post(FadeIn);
    }

    private void FileItemControl_PointerMoved(object? sender, PointerEventArgs e)
    {
        LongPressed.Cancel();
    }

    private void FileItemControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        LongPressed.Released();
    }

    private void FileItemControl_PointerExited(object? sender, PointerEventArgs e)
    {
        if (DataContext is FileVersionItemModel model)
        {
            model.Top = false;
        }
    }

    private void FileItemControl_PointerEntered(object? sender, PointerEventArgs e)
    {
        if (DataContext is FileVersionItemModel model)
        {
            model.Top = true;
        }
    }

    private void FileItemControl_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        (DataContext as FileVersionItemModel)?.Install();
    }

    private void FileItemControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not FileVersionItemModel model)
        {
            return;
        }
        model.SetSelect();

        var ev = e.GetCurrentPoint(this);
        if (ev.Properties.IsXButton1Pressed)
        {
            model.Back();
            e.Handled = true;
        }
        else if (ev.Properties.IsXButton2Pressed)
        {
            model.Next();
            e.Handled = true;
        }
    }

    private void FadeIn()
    {
        var animation = new Animation
        {
            FillMode = FillMode.Forward,
            Easing = new CircularEaseInOut(),
            Children =
            {
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter
                        {
                            Property = OpacityProperty,
                            Value = 0.0d
                        },
                        new Setter
                        {
                            Property = TranslateTransform.YProperty,
                            Value = 10d
                        }
                    },
                    Cue = new Cue(0d)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter
                        {
                            Property = OpacityProperty,
                            Value = 1.0d
                        },
                        new Setter
                        {
                            Property = TranslateTransform.YProperty,
                            Value = 10d
                        }
                    },
                    Cue = new Cue(0.5d)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter
                        {
                            Property = TranslateTransform.YProperty,
                            Value = 0d
                        }
                    },
                    Cue = new Cue(1d)
                }
            },
            Duration = TimeSpan.FromMilliseconds(500)
        };

        animation.RunAsync(this);
    }
}