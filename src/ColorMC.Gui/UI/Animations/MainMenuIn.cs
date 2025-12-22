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

public static class MainMenuIn
{
    private static Animation s_animation;

    private static Animation Make()
    {
        return s_animation ??= new Animation
        {
            Duration = TimeSpan.FromSeconds(0.4),
            Easing = new CubicEaseOut(), // 缓动弹出
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0),
                    Setters = {
                        new Setter(ScaleTransform.ScaleXProperty, 0.9),
                        new Setter(ScaleTransform.ScaleYProperty, 0.9),
                        new Setter(Visual.OpacityProperty, 0.0)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1),
                    Setters = {
                        new Setter(ScaleTransform.ScaleXProperty, 1.0),
                        new Setter(ScaleTransform.ScaleYProperty, 1.0),
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
