using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;
using ColorMC.Gui.Utils;
using System;

namespace ColorMC.Gui.UI.Animations;

/// <summary>
/// UI项目动画
/// </summary>
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
                            Value = GuiConfigUtils.Config.Style.AmFade ? 0.0d : 1.0d
                        },
                        new Setter
                        {
                            Property = TranslateTransform.YProperty,
                            Value = 20d
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
                            Property = TranslateTransform.YProperty,
                            Value = -10d
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
                            Value = 0d
                        }
                    },
                    Cue = new Cue(1d)
                }
            },
            Duration = TimeSpan.FromMilliseconds(GuiConfigUtils.Config.Style.AmTime)
        };
    }
}
