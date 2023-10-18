using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using System;
using System.Threading;

namespace ColorMC.Gui.UI.Animations;

/// <summary>
/// Transitions between two pages by sliding them horizontally or vertically.
/// </summary>
public class SelfPageSlideSide
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SelfPageSlide"/> class.
    /// </summary>
    /// <param name="duration">The duration of the animation.</param>
    /// <param name="orientation">The axis on which the animation should occur</param>
    public SelfPageSlideSide(TimeSpan duration)
    {
        Duration = duration;
    }

    public bool Mirror { get; set; }

    /// <summary>
    /// Gets the duration of the animation.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets element entrance easing.
    /// </summary>
    public Easing SlideEasing { get; set; } = new CircularEaseInOut();

    /// <inheritdoc />
    public virtual void Start(Visual? from, Visual? to, CancellationToken cancellationToken)
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
                                Value = 0
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
                                Value = 0
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

    /// <summary>
    /// Gets the common visual parent of the two control.
    /// </summary>
    /// <param name="from">The from control.</param>
    /// <param name="to">The to control.</param>
    /// <returns>The common parent.</returns>
    /// <exception cref="ArgumentException">
    /// The two controls do not share a common parent.
    /// </exception>
    /// <remarks>
    /// Any one of the parameters may be null, but not both.
    /// </remarks>
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