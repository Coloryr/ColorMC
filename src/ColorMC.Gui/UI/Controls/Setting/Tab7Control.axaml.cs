using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class Tab7Control : UserControl
{
    public Easing SlideEasing = new CircularEaseInOut();

    public Tab7Control()
    {
        InitializeComponent();
    }

    public async void Start()
    {
        Image1.Opacity = 0;
        StackPanel1.Opacity = 0;
        StackPanel2.Opacity = 0;

        await Task.Delay(500);

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
                            Property = TranslateTransform.XProperty,
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
                            Property = TranslateTransform.XProperty,
                            Value = 0d
                        }
                    },
                    Cue = new Cue(1d)
                }
            },
            Duration = TimeSpan.FromMilliseconds(500)
        };

        _ = animation.RunAsync(Image1);
        await animation1.RunAsync(StackPanel1);
        await animation1.RunAsync(StackPanel2);
    }
}
