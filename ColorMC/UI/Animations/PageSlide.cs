using Avalonia.Animation.Easings;
using Avalonia.Animation;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.UI.Animations;

/// <summary>
/// Transitions between two pages by sliding them horizontally or vertically.
/// </summary>
public class PageSlide : IPageTransition
{

    /// <summary>
    /// Initializes a new instance of the <see cref="PageSlide"/> class.
    /// </summary>
    /// <param name="duration">The duration of the animation.</param>
    /// <param name="orientation">The axis on which the animation should occur</param>
    public PageSlide(TimeSpan duration)
    {
        Duration = duration;
    }

    /// <summary>
    /// Gets the duration of the animation.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets element entrance easing.
    /// </summary>
    public Easing SlideInEasing { get; set; } = new LinearEasing();

    /// <summary>
    /// Gets or sets element exit easing.
    /// </summary>
    public Easing SlideOutEasing { get; set; } = new LinearEasing();

    /// <inheritdoc />
    public virtual async Task Start(Visual? from, Visual? to, bool forward, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        var tasks = new List<Task>();
        var parent = GetVisualParent(from, to);
        var distance = parent.Bounds.Width ;
        var translateProperty = TranslateTransform.XProperty;

        if (from != null)
        {
            double end = forward ? -distance : distance;
            var animation = new Animation
            {
                Easing = SlideOutEasing,
                FillMode = FillMode.Forward,
                Children =
                    {
                        new KeyFrame
                        {
                            Setters = { new Setter { Property = translateProperty, Value = 0d } },
                            Cue = new Cue(0d)
                        },
                        new KeyFrame
                        {
                            Setters =
                            {
                                new Setter
                                {
                                    Property = translateProperty,
                                    Value = end * 0.9
                                }
                            },
                            Cue = new Cue(0.5)
                        },
                        new KeyFrame
                        {
                            Setters =
                            {
                                new Setter
                                {
                                    Property = translateProperty,
                                    Value = end
                                }
                            },
                            Cue = new Cue(1d)
                        }
                    },
                Duration = Duration
            };
            tasks.Add(animation.RunAsync(from, null, cancellationToken));
        }

        if (to != null)
        {
            to.IsVisible = true;
            double end = forward ? distance : -distance;
            var animation = new Animation
            {
                FillMode = FillMode.Forward,
                Easing = SlideInEasing,
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
                                    Value = end * 0.1
                                }
                            },
                            Cue = new Cue(0.5)
                        },
                        new KeyFrame
                        {
                            Setters = { new Setter { Property = translateProperty, Value = 0d } },
                            Cue = new Cue(1d)
                        }
                    },
                Duration = Duration
            };
            tasks.Add(animation.RunAsync(to, null, cancellationToken));
        }

        await Task.WhenAll(tasks);

        if (from != null && !cancellationToken.IsCancellationRequested)
        {
            from.IsVisible = false;
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
    protected static IVisual GetVisualParent(IVisual? from, IVisual? to)
    {
        var p1 = (from ?? to)!.VisualParent;
        var p2 = (to ?? from)!.VisualParent;

        if (p1 != null && p2 != null && p1 != p2)
        {
            throw new ArgumentException("Controls for PageSlide must have same parent.");
        }

        return p1 ?? throw new InvalidOperationException("Cannot determine visual parent.");
    }
}