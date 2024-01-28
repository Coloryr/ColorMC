using Avalonia.Controls.Shapes;
using Silk.NET.OpenAL;
using System;
using System.Threading;

namespace ColorMC.Gui.Player;

/// <summary>
/// OpenAl输出
/// </summary>
public class OpenalPlayer : IPlayer
{
    private readonly uint _alSource;
    private readonly IntPtr _device;
    private readonly IntPtr _context;
    private readonly AL al;
    private readonly ALContext alc;

    private bool _isPlay = false;

    public float Volume
    {
        set
        {
            al.SetSourceProperty(_alSource, SourceFloat.Gain, value);
        }
    }

    public OpenalPlayer()
    {
        alc = ALContext.GetApi();
        al = AL.GetApi();
        // Get the default device, then go though all devices and select the AL soft device if it exists.
        unsafe
        {
            string deviceName = alc.GetContextProperty(null, GetContextString.DeviceSpecifier);

            _device = new(alc.OpenDevice(deviceName));
            _context = new(alc.CreateContext((Device*)_device, null));
            alc.MakeContextCurrent(_context);

            _alSource = al.GenSource();
        }

        CheckALError();

        new Thread(() =>
        {
            while (!App.IsClose)
            {
                al.GetSourceProperty(_alSource, GetSourceInteger.BuffersQueued, out int value);
                al.GetSourceProperty(_alSource, GetSourceInteger.BuffersProcessed, out int value1);
                if (value - value1 > 100)
                {
                    uint temp;
                    unsafe
                    {
                        al.SourceUnqueueBuffers(_alSource, 1, &temp);
                        al.DeleteBuffer(temp);
                    }
                    Thread.Sleep(10);
                }
                al.GetSourceProperty(_alSource, GetSourceInteger.SourceState, out int state);
                if (_isPlay && value == 0 && Media.Decoding == false &&
                    (SourceState)state == SourceState.Stopped)
                {
                    _isPlay = false;
                    Media.PlayEnd();
                }
            }
        })
        {
            Name = "ColorMC_OpenAL"
        }.Start();
    }

    public void Close()
    {
        al.DeleteSource(_alSource);

        unsafe
        {
            alc.MakeContextCurrent(null);
            alc.DestroyContext((Context*)_context);
            alc.CloseDevice((Device*)_device);
        }
    }

    public void CheckALError()
    {
        AudioError error = al.GetError();
        if (error != AudioError.NoError)
        {
            App.ShowError($"ALError", new Exception(error.ToString()));
        }
    }

    public void Pause()
    {
        al.SourcePause(_alSource);
    }

    public void Play()
    {
        al.SourcePlay(_alSource);
    }

    public void Stop()
    {
        al.SetSourceProperty(_alSource, SourceFloat.Gain, 0);
        al.SourceStop(_alSource);

        al.GetSourceProperty(_alSource, GetSourceInteger.BuffersQueued, out int value);
        while (value > 0)
        {
            unsafe
            {
                uint temp;
                al.SourceUnqueueBuffers(_alSource, 1, &temp);
                al.DeleteBuffer(temp);
                value--;
            }
        }
    }

    public void Write(int numChannels, int bitsPerSample, byte[] buff, int length, int sampleRate)
    {
        BufferFormat format;

        if (numChannels == 1)
        {
            if (bitsPerSample == 8)
                format = BufferFormat.Mono8;
            else if (bitsPerSample == 16)
                format = BufferFormat.Mono16;
            else
            {
                return;
            }
        }
        else if (numChannels == 2)
        {
            if (bitsPerSample == 8)
                format = BufferFormat.Stereo8;
            else if (bitsPerSample == 16)
                format = BufferFormat.Stereo16;
            else
            {
                return;
            }
        }
        else
        {
            return;
        }

        unsafe
        {
            var alBuffer = al.GenBuffer();
            fixed (void* ptr = buff)
            {
                al.BufferData(alBuffer, format, ptr, length, sampleRate);
            }
            al.SourceQueueBuffers(_alSource, 1, &alBuffer);
            al.GetSourceProperty(_alSource, GetSourceInteger.SourceState, out int state);
            if ((SourceState)state != SourceState.Playing)
            {
                al.SourcePlay(_alSource);
                _isPlay = true;
            }
        }
    }
}
