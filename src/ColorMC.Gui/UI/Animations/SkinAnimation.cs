using Avalonia.Threading;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.Skin;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Animations;

/// <summary>
/// 皮肤的动画
/// </summary>
public class SkinAnimation
{
    private bool run;
    private bool start;
    private int frame = 0;
    private readonly SkinRender Render;

    public Vector3 Arm;
    public Vector3 Leg;
    public Vector3 Head;

    public SkinAnimation(SkinRender render)
    {
        Render = render;
        run = true;

        Arm.X = 40;
    }

    /// <summary>
    /// 停止
    /// </summary>
    public void Stop()
    {
        start = false;
        run = false;
    }

    /// <summary>
    /// 开始
    /// </summary>
    public void Start()
    {
        start = true;
    }

    /// <summary>
    /// 暂停
    /// </summary>
    public void Pause()
    {
        start = false;
    }

    public void Tick()
    {
        if (start)
        {
            frame++;
            if (frame > 120)
            {
                frame = 0;
            }

            if (frame <= 60)
            {
                //0 360
                //-180 180
                Arm.Y = frame * 6 - 180;
                //0 180
                //90 -90
                Leg.Y = 90 - frame * 3;
                //-30 30
                if (Render.model.SteveModelType == SkinType.NewSlim)
                {
                    Head.Z = 0;
                    Head.X = frame - 30;
                }
                else
                {
                    Head.X = 0;
                    Head.Z = frame - 30;
                }
            }
            else
            {
                //360 720
                //180 -180
                Arm.Y = 540 - frame * 6;
                //180 360
                //-90 90
                Leg.Y = frame * 3 - 270;
                //30 -30
                if (Render.model.SteveModelType == SkinType.NewSlim)
                {
                    Head.Z = 0;
                    Head.X = 90 - frame;
                }
                else
                {
                    Head.X = 0;
                    Head.Z = 90 - frame;
                }
            }
        }

        Task.Run(() =>
        {
            Thread.Sleep(15);
            if (run)
            {
                Dispatcher.UIThread.Invoke(Render.RequestNextFrameRendering);
            }
        });
        
    }
}