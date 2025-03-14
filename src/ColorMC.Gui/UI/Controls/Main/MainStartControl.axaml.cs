using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using System;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Main;

/// <summary>
/// 开始欢迎界面
/// </summary>
public partial class MainStartControl : UserControl
{
    public Easing SlideEasing = new CircularEaseInOut();

    public MainStartControl()
    {
        InitializeComponent();
    }

    public async Task Start(DisplayType type, string? title, Bitmap? image)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            Text1.Text = title.Replace("\\n", "\n");
        }
        else
        {
            Text1.Text = App.Lang("MainWindow.Text12");
        }

        if (image != null)
        {
            Image1.Source = image;
        }
        else
        {
            Image1.Source = ImageManager.GameIcon;
        }

        switch (type)
        {
            case DisplayType.Normal:
                StackPanel1.Orientation = Orientation.Horizontal;
                break;
            case DisplayType.NormalTop:
                StackPanel1.Orientation = Orientation.Vertical;
                break;
            case DisplayType.LeftRight:
                StackPanel1.Orientation = Orientation.Horizontal;
                Start();
                break;
            case DisplayType.TopBottom:
                StackPanel1.Orientation = Orientation.Vertical;
                Start1();
                break;
        }

        await Task.Delay(2000);
    }

    private void Start()
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
        _ = animation.RunAsync(Image1);
        _ = animation1.RunAsync(Grid1);
    }

    private void Start1()
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
            Duration = TimeSpan.FromMilliseconds(1500)
        };
        _ = animation.RunAsync(Image1);
        _ = animation1.RunAsync(Grid1);
    }
}
