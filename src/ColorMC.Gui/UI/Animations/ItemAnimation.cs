using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace ColorMC.Gui.UI.Animations;

public static class ItemAnimation
{
    public static Animation Make()
    {
        return new Animation
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
                            Property = Visual.OpacityProperty,
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
                            Property = Visual.OpacityProperty,
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
    }
}
