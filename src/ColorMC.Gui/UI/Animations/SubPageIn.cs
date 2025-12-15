using System;
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

public static class SubPageIn
{
    private static Animation s_animation;

    private static Animation Make()
    {
        return s_animation ??= new Animation
        {
            Duration = TimeSpan.FromSeconds(0.35),
            Easing = new CubicEaseOut(), // 缓动：快速进入，缓慢停止
            FillMode = FillMode.Forward, // 保持动画结束状态
            Children =
            {
                // 0% 进度：位于下方 150px，透明
                new KeyFrame
                {
                    Cue = new Cue(0),
                    Setters =
                    {
                        new Setter(TranslateTransform.YProperty, 150.0),
                        new Setter(Visual.OpacityProperty, 0.0)
                    }
                },
                // 100% 进度：回到原位 0px，不透明
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters =
                    {
                        new Setter(TranslateTransform.YProperty, 0.0),
                        new Setter(Visual.OpacityProperty, 1.0)
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
