using System;
using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;

namespace ColorMC.Gui.UI.Controls.Main;

public partial class MainStartControl : UserControl
{
    public Easing SlideEasing = new CircularEaseInOut();

    public MainStartControl()
    {
        InitializeComponent();
    }

    public async void Start(Control panel)
    {
        var animation = new Animation
        {
            FillMode = FillMode.Forward,
            Easing = SlideEasing,
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
                            Property = TranslateTransform.XProperty,
                            Value = -50d
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
                        } ,
                        new Setter
                        {
                            Property = TranslateTransform.XProperty,
                            Value = 0d
                        }
                    },
                    Cue = new Cue(1d)
                }
            },
            Duration = TimeSpan.FromMilliseconds(1500)
        };
        var animation1 = new Animation
        {
            FillMode = FillMode.Forward,
            Easing = SlideEasing,
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
                            Property = TranslateTransform.XProperty,
                            Value = 100d
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
                        } ,
                        new Setter
                        {
                            Property = TranslateTransform.XProperty,
                            Value = 0d
                        }
                    },
                    Cue = new Cue(1d)
                }
            },
            Duration = TimeSpan.FromMilliseconds(1500)
        };
        var animation2 = new Animation
        {
            FillMode = FillMode.Forward,
            Easing = SlideEasing,
            Children =
            {
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter
                        {
                            Property = OpacityProperty,
                            Value = 1.0d
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
                            Value = 0.0d
                        }
                    },
                    Cue = new Cue(1d)
                }
            },
            Duration = TimeSpan.FromMilliseconds(500)
        };
        panel.IsVisible = true;
        _ = animation.RunAsync(Image1);
        _ = animation1.RunAsync(Grid1);
        await Task.Delay(2000);
        await animation2.RunAsync(this);
        panel.IsVisible = false;
    }
}
