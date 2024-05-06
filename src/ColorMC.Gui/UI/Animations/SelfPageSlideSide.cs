using System;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;

namespace ColorMC.Gui.UI.Animations;

/// <summary>
/// Transitions between two pages by sliding them horizontally or vertically.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SelfPageSlide"/> class.
/// </remarks>
/// <param name="duration">The duration of the animation.</param>
/// <param name="orientation">The axis on which the animation should occur</param>
public class SelfPageSlideSide(TimeSpan duration)
{
    public bool Mirror { get; set; }

    /// <summary>
    /// Gets the duration of the animation.
    /// </summary>
    public TimeSpan Duration { get; set; } = duration;

    /// <summary>
    /// Gets or sets element entrance easing.
    /// </summary>
    public Easing SlideEasing { get; set; } = new CircularEaseInOut();

    /// <inheritdoc />
    public void Start(Visual? from, Visual? to, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (from != null)
        {
            double end = from.Bounds.Width;
            var animation = new Animation
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
                                Property = TranslateTransform.XProperty,
                                Value = 0d
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
                                Property = TranslateTransform.XProperty,
                                Value = Mirror ? end - 1 : -end + 1
                            }
                        },
                        Cue = new Cue(1d)
                    },
                    new KeyFrame
                    {
                        Setters =
                        {
                            new Setter
                            {
                                Property = Visual.IsVisibleProperty,
                                Value = false
                            }
                        },
                        Cue = new Cue(1d)
                    }
                },
                Duration = Duration
            };
            animation.RunAsync(from, cancellationToken);
        }

        if (to != null)
        {
            double end = to.Bounds.Width;
            to.IsVisible = true;
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
                                Property = TranslateTransform.XProperty,
                                Value = Mirror ? end - 1 : -end + 1
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
                                Property = TranslateTransform.XProperty,
                                Value = 0d
                            }
                        },
                        Cue = new Cue(1d)
                    }
                },
                Duration = Duration
            };
            animation.RunAsync(to, cancellationToken);
        }
    }
}