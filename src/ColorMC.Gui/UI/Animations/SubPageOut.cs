using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Animations;

public static class SubPageOut
{
    private static Animation s_animation;

    private static Animation Make()
    {
        return s_animation ??= new Animation
        {
            Duration = TimeSpan.FromSeconds(0.3),
            Easing = new CubicEaseIn(), // 缓动：慢速开始，加速离开
            FillMode = FillMode.Forward,
            Children =
            {
                // 0% 进度：原位
                new KeyFrame
                {
                    Cue = new Cue(0),
                    Setters =
                    {
                        new Setter(TranslateTransform.YProperty, 0.0),
                        new Setter(Visual.OpacityProperty, 1.0)
                    }
                },
                // 100% 进度：下方 150px，透明
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters =
                    {
                        new Setter(TranslateTransform.YProperty, 150.0),
                        new Setter(Visual.OpacityProperty, 0.0)
                    }
                }
            }
        };
    }

    public static Task Start(Control control, CancellationToken token = default)
    {
        if (!GuiConfigUtils.Config.Style.EnableAm)
        {
            return Task.CompletedTask;
        }

        return Make().RunAsync(control, token);
    }
}
