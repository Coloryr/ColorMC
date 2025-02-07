using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Animations;

public static class CardAnimation
{
    private static Animation Make()
    {
        return new Animation()
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
                            Value = GuiConfigUtils.Config.Style.AmFade ? 0.0d : 1.0d
                        },
                        new Setter
                        {
                            Property = TranslateTransform.XProperty,
                            Value = 400d
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
                            Value = GuiConfigUtils.Config.Style.AmFade ? 1.0d : 1.0d
                        },
                        new Setter
                        {
                            Property = TranslateTransform.XProperty,
                            Value = -5d
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
                            Property = TranslateTransform.XProperty,
                            Value = 0d
                        }
                    },
                    Cue = new Cue(1d)
                }
            },
            Duration = TimeSpan.FromMilliseconds(GuiConfigUtils.Config.Style.AmTime)
        };
    }

    public static void Start(Control control)
    {
        if (!GuiConfigUtils.Config.Style.EnableAm)
        {
            return;
        }

        Make().RunAsync(control);
    }
}
