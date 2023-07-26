using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.Skin;
using System.Numerics;

namespace ColorMC.Gui.UI.Animations;

/// <summary>
/// 皮肤的动画
/// </summary>
public class SkinAnimation
{
    private bool _run;
    private bool _start;
    private int _frame = 0;
    private readonly SkinRender _render;

    public Vector3 Arm;
    public Vector3 Leg;
    public Vector3 Head;

    public SkinAnimation(SkinRender render)
    {
        _render = render;
        _run = true;

        Arm.X = 40;
    }

    /// <summary>
    /// 停止
    /// </summary>
    public void Stop()
    {
        _start = false;
        _run = false;
    }

    /// <summary>
    /// 开始
    /// </summary>
    public void Start()
    {
        _start = true;
    }

    /// <summary>
    /// 暂停
    /// </summary>
    public void Pause()
    {
        _start = false;
    }

    public void Tick()
    {
        if (_start)
        {
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
                if (_render._model.SteveModelType == SkinType.NewSlim)
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
                if (_render._model.SteveModelType == SkinType.NewSlim)
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
    }
}