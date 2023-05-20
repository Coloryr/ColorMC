using Avalonia.Threading;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.Skin;
using System;
using System.Numerics;
using System.Threading;

namespace ColorMC.Gui.UI.Animations;

public class SkinAnimation : IDisposable
{
    private readonly Thread thread;
    private bool run;
    private bool start;
    private readonly Semaphore semaphore = new(0, 2);
    private int frame = 0;
    private readonly SkinRender Render;

    public Vector3 Arm;
    public Vector3 Leg;
    public Vector3 Head;

    public SkinAnimation(SkinRender render)
    {
        Render = render;
        thread = new(Tick)
        {
            Name = "ColorMC-SkinAnimation"
        };
        run = true;
        thread.Start();

        Arm.X = 40;
    }

    public void Dispose()
    {
        run = false;
        thread.Join();
    }

    public void Start()
    {
        start = true;
        semaphore.Release();
    }

    public void Pause()
    {
        start = false;
    }

    private void Tick()
    {
        while (run)
        {
            semaphore.WaitOne();
            while (start)
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

                Dispatcher.UIThread.InvokeAsync(Render.RequestNextFrameRendering).Wait();

                Thread.Sleep(10);
            }
        }
    }
}