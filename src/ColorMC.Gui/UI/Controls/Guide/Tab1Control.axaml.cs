using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using System;

namespace ColorMC.Gui.UI.Controls.Guide;

public partial class Tab1Control : UserControl
{
    public Easing SlideEasing = new CircularEaseInOut();

    public Tab1Control()
    {
        InitializeComponent();
    }

    public async void Start()
    {
        StackPanel1.Opacity = 0;
        StackPanel2.Opacity = 0;

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
                            Value = 50d
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
            Duration = TimeSpan.FromMilliseconds(500)
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
                            Property = TranslateTransform.YProperty,
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
                            Property = TranslateTransform.YProperty,
                            Value = 0d
                        }
                    },
                    Cue = new Cue(1d)
                }
            },
            Duration = TimeSpan.FromMilliseconds(500)
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
                            Value = 0.0d
                        },
                        new Setter
                        {
                            Property = TranslateTransform.YProperty,
                            Value = 50d
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
                            Property = TranslateTransform.YProperty,
                            Value = 0d
                        }
                    },
                    Cue = new Cue(1d)
                }
            },
            Duration = TimeSpan.FromMilliseconds(500)
        };

        await animation.RunAsync(StackPanel);
        StackPanel1.Opacity = 1;
        await animation1.RunAsync(StackPanel1);
        StackPanel2.Opacity = 1;
        await animation2.RunAsync(StackPanel2);
    }
}
