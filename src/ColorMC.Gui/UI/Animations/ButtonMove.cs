using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Animations;

/// <summary>
/// Transitions between two pages by sliding them horizontally or vertically.
/// </summary>
public class ButtonMove
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SelfPageSlide"/> class.
    /// </summary>
    /// <param name="duration">The duration of the animation.</param>
    /// <param name="orientation">The axis on which the animation should occur</param>
    public ButtonMove(TimeSpan duration)
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
    public Easing SlideEasing { get; set; } = new SineEaseInOut();

    /// <inheritdoc />
    public virtual void Start(Visual from)
    {
        double movedis = from.Bounds.Width / 3;

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
                            Property = TranslateTransform.YProperty,
                            Value = 0d
                        },
                        new Setter
                        {
                            Property = Visual.OpacityProperty,
                            Value = 0d
                        },
                    },
                    Cue = new Cue(0d)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter
                        {
                            Property = TranslateTransform.YProperty,
                            Value = movedis
                        },
                        new Setter
                        {
                            Property = Visual.OpacityProperty,
                            Value = 1d
                        },
                    },
                    Cue = new Cue(0.1d)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter
                        {
                            Property = TranslateTransform.YProperty,
                            Value = 0
                        }
                    },
                    Cue = new Cue(0.2d)
                },
                 new KeyFrame
                {
                    Setters =
                    {
                        new Setter
                        {
                            Property = TranslateTransform.YProperty,
                            Value = movedis
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
                            Property = TranslateTransform.YProperty,
                            Value = 0
                        }
                    },
                    Cue = new Cue(0.4d)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter
                        {
                            Property = TranslateTransform.YProperty,
                            Value = 0
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
                            Value = movedis
                        }
                    },
                    Cue = new Cue(0.6d)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter
                        {
                            Property = TranslateTransform.YProperty,
                            Value = 0
                        }
                    },
                    Cue = new Cue(0.7d)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter
                        {
                            Property = TranslateTransform.YProperty,
                            Value = movedis
                        }
                    },
                    Cue = new Cue(0.8d)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter
                        {
                            Property = TranslateTransform.YProperty,
                            Value = 0
                        },
                        new Setter
                        {
                            Property = Visual.OpacityProperty,
                            Value = 1d
                        },
                    },
                    Cue = new Cue(0.9d)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter
                        {
                            Property = TranslateTransform.YProperty,
                            Value = 0
                        },
                        new Setter
                        {
                            Property = Visual.OpacityProperty,
                            Value = 0d
                        },
                    },
                    Cue = new Cue(1d)
                }
            },
            Duration = Duration
        };
        animation.RunAsync(from);
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