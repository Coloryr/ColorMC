using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using ColorMC.Gui.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Animations;

/// <summary>
/// “≥√Ê«–ªª∂Øª≠
/// </summary>
/// <param name="duration"></param>
public class SelfPageSlideY(TimeSpan duration) : IPageTransition
{
    /// <summary>
    /// Gets the duration of the animation.
    /// </summary>
    public TimeSpan Duration { get; set; } = duration;

    /// <summary>
    /// Gets or sets element entrance easing.
    /// </summary>
    public Easing SlideEasing { get; set; } = new CircularEaseInOut();

    public bool Fade { get; set; }

    /// <inheritdoc />
    public async Task Start(Visual? from, Visual? to, bool forward, CancellationToken cancellationToken)
    {
        if (!GuiConfigUtils.Config.Style.EnableAm)
        {
            if (from != null)
            {
                from.RenderTransform = new TranslateTransform();
                from.IsVisible = false;
                from.Opacity = 0;
            }
            if (to != null)
            {
                to.RenderTransform = new TranslateTransform();
                to.IsVisible = true;
                to.Opacity = 1;
            }
            return;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        var tasks = new List<Task>();
        var parent = GetVisualParent(from, to);
        var distance = parent.Bounds.Height;
        var translateProperty = TranslateTransform.YProperty;

        if (from != null)
        {
            double end = forward ? -distance : distance;
            var animation = Fade ? new Animation
            {
                Easing = SlideEasing,
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = translateProperty,
                                Value = 0d
                            },
                            new Setter
                            {
                                Property = Visual.OpacityProperty,
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
                                Property = Visual.OpacityProperty,
                                Value = 0d
                            }
                        },
                        Cue = new Cue(0.3d)
                    },
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = translateProperty,
                                Value = end
                            },
                            new Setter
                            {
                                Property = Visual.OpacityProperty,
                                Value = 0d
                            }
                        },
                        Cue = new Cue(1d)
                    }
                },
                Duration = Duration
            } : new Animation
            {
                Easing = SlideEasing,
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = translateProperty,
                                Value = 0d
                            },
                            new Setter
                            {
                                Property = Visual.OpacityProperty,
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
                                Property = translateProperty,
                                Value = end
                            },
                            new Setter
                            {
                                Property = Visual.OpacityProperty,
                                Value = 1.0d
                            }
                        },
                        Cue = new Cue(1d)
                    }
                },
                Duration = Duration
            };
            tasks.Add(animation.RunAsync(from, cancellationToken));
        }

        if (to != null)
        {
            to.IsVisible = true;
            double end = forward ? distance : -distance;
            var animation = Fade ? new Animation
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
                                Property = translateProperty,
                                Value = end
                            },
                            new Setter
                            {
                                Property = Visual.OpacityProperty,
                                Value = 0.0d
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
                                Property = Visual.OpacityProperty,
                                Value = 1d
                            }
                        },
                        Cue = new Cue(0.3d)
                    },
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = translateProperty,
                                Value = 0d
                            },
                            new Setter
                            {
                                Property = Visual.OpacityProperty,
                                Value = 1d
                            }
                        },
                        Cue = new Cue(1d)
                    },
                },
                Duration = Duration
            } : new Animation
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
                                Property = translateProperty,
                                Value = end
                            },
                            new Setter
                            {
                                Property = Visual.OpacityProperty,
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
                                Property = translateProperty,
                                Value = 0d
                            },
                            new Setter
                            {
                                Property = Visual.OpacityProperty,
                                Value = 1.0d
                            }
                        },
                        Cue = new Cue(1d)
                    }
                },
                Duration = Duration
            };
            tasks.Add(animation.RunAsync(to, cancellationToken));
        }

        await Task.WhenAll(tasks);

        if (from != null && !cancellationToken.IsCancellationRequested)
        {
            from.IsVisible = false;
        }
    }

    protected static Visual GetVisualParent(Visual? from, Visual? to)
    {
        var p1 = (from ?? to)!.GetVisualParent();
        var p2 = (to ?? from)!.GetVisualParent();

        if (p1 != null && p2 != null && p1 != p2)
        {
            throw new ArgumentException("Controls for PageSlide must have same parent.");
        }

        return p1 ?? throw new InvalidOperationException("Cannot determine visual parent.");
    }
}