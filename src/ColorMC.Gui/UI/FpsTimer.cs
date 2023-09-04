using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using System;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace ColorMC.Gui.UI;

public class FpsTimer
{
    private readonly OpenGlControlBase _render;
    private readonly Timer _timer;
    private readonly Semaphore _semaphore = new(0, 2);
    private bool _pause = true;

    public int Fps { get; set; } = 60;
    public Action<int>? FpsTick { private get; init; }
    public bool Pause 
    {
        get
        {
            return _pause;
        }
        set
        {
            //暂停 -> 继续
            if (_pause && value == false)
            {
                _pause = false;
                _timer.Start();
                _semaphore.Release();
            }
            else //暂停
            {
                _pause = true;
                _timer.Stop();
            }
        } 
    }
    public int NowFps { get; private set; }

    private int _time;
    private bool _run;

    public FpsTimer(OpenGlControlBase render)
    {
        _render = render;
        _run = true;
        _time = (int)((double)1000 / Fps);
        _timer = new(TimeSpan.FromSeconds(1));
        _timer.BeginInit();
        _timer.AutoReset = true;
        _timer.Elapsed += Timer_Elapsed;
        _timer.EndInit();
        new Thread(() =>
        {
            while (_run)
            {
                if (Pause)
                {
                    _semaphore.WaitOne();
                }
                if (!_run)
                {
                    return;
                }
                Dispatcher.UIThread.Post(() =>
                {
                    _render.RequestNextFrameRendering();
                    NowFps++;
                });
                Thread.Sleep(_time);
            }
        })
        {
            Name = "ColorMC_Render_Timer"
        }.Start();
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        if (!Pause)
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
        }
        FpsTick?.Invoke(NowFps);
        NowFps = 0;
    }

    public void Close()
    {
        _run = false;
        if (Pause)
        {
            Pause = false;
        }
        _timer.Close();
        _timer.Dispose();
    }
}
