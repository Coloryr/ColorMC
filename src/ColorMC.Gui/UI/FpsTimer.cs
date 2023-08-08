using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using System;
using System.Threading;

namespace ColorMC.Gui.UI;

public class FpsTimer
{
    private readonly OpenGlControlBase _render;
    private readonly Timer _timer;

    public int Fps { get; set; } = 60;
    public Action<int>? FpsTick { private get; init; }
    public bool Pause { get; set; }
    public int NowFps { get; private set; }

    private int _time;
    private bool _run;

    public FpsTimer(OpenGlControlBase render)
    {
        _render = render;
        _run = true;
        _time = (int)((double)1000 / Fps);
        _timer = new(Tick);
        _timer.Change(0, 1000);
        new Thread(() =>
        {
            while (_run)
            {
                if (Pause)
                {
                    Thread.Sleep(100);
                    continue;
                }
                Dispatcher.UIThread.Invoke(() => _render.RequestNextFrameRendering());
                NowFps++;
                Thread.Sleep(_time);
            }
        })
        {
            Name = "ColorMC_Render_Timer"
        }.Start();
    }

    private void Tick(object? state)
    {
        if (NowFps != Fps && _time > 1)
        {
            if (NowFps > Fps)
            {
                _time++;
            }
            else
            {
                _time--;
            }
        }
        FpsTick?.Invoke(NowFps);
        NowFps = 0;
    }

    public void Close()
    {
        _run = false;
        _timer.Dispose();
    }
}
