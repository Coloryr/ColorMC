using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;
using ColorMC.Gui.Utils;

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
            Easing = new BackEaseOut(), // 弹性效果
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0.0),
                    Setters =
                    {
                        new Setter(ScaleTransform.ScaleXProperty, 0.5),
                        new Setter(ScaleTransform.ScaleYProperty, 0.5)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1.0),
                    Setters =
                    {
                        new Setter(ScaleTransform.ScaleXProperty, 1.0),
                        new Setter(ScaleTransform.ScaleYProperty, 1.0)
                    }
                }
            },
            Duration = TimeSpan.FromMilliseconds(GuiConfigUtils.Config.Style.AmTime)
        };
    }
}
