using System;
using System.Numerics;
using Avalonia.Threading;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.Skin.OpenGL;
using ColorMC.Gui.UI.Model.Skin;

namespace ColorMC.Gui.UI.Animations;

/// <summary>
/// 皮肤的动画
/// </summary>
public class SkinAnimation
{
    private int _frame = 0;
    private readonly SkinRender _render;
    private bool _close = false;

    public bool Run { get; set; }

    public Vector3 Arm;
    public Vector3 Leg;
    public Vector3 Head;

    public SkinAnimation(SkinRender render)
    {
        _render = render;

        Arm.X = 40;

        DispatcherTimer.Run(Tick, TimeSpan.FromMilliseconds(20));
    }

    public void Close()
    {
        Run = false;
        _close = true;
    }

    /// <summary>
    /// 进行动画演算
    /// </summary>
    private bool Tick()
    {
        if (Run)
        {
            var model = (_render.DataContext as SkinModel)!;
            _frame++;
            if (_frame > 120)
            {
                _frame = 0;
            }

            if (_frame <= 60)
            {
                //0 360
                //-180 180
                Arm.Y = _frame * 6 - 180;
                //0 180
                //90 -90
                Leg.Y = 90 - _frame * 3;
                //-30 30
                if (model.SteveModelType == SkinType.NewSlim)
                {
                    Head.Z = 0;
                    Head.X = _frame - 30;
                }
                else
                {
                    Head.X = 0;
                    Head.Z = _frame - 30;
                }
            }
            else
            {
                //360 720
                //180 -180
                Arm.Y = 540 - _frame * 6;
                //180 360
                //-90 90
                Leg.Y = _frame * 3 - 270;
                //30 -30
                if (model.SteveModelType == SkinType.NewSlim)
                {
                    Head.Z = 0;
                    Head.X = 90 - _frame;
                }
                else
                {
                    Head.X = 0;
                    Head.Z = 90 - _frame;
                }
            }
        }

        return !_close;
    }
}