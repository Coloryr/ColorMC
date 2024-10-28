using System;
using System.Threading;
using ColorMC.Gui.Manager;
using Silk.NET.OpenAL;

namespace ColorMC.Gui.MusicPlayer;

/// <summary>
/// OpenAl输出
/// </summary>
public class OpenALPlayer : IPlayer
{
    private readonly uint _alSource;
    private readonly IntPtr _device;
    private readonly IntPtr _context;
    private readonly AL al;
    private readonly ALContext alc;

    public float Volume
    {
        set
        {
            al.SetSourceProperty(_alSource, SourceFloat.Gain, value);
        }
    }

    public OpenALPlayer()
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
            WindowManager.ShowError($"ALError", new Exception(error.ToString()));
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
            }
        }

        int value, value1;

        do
        {
            al.GetSourceProperty(_alSource, GetSourceInteger.BuffersQueued, out value);
            al.GetSourceProperty(_alSource, GetSourceInteger.BuffersProcessed, out value1);
            while (value1 > 0)
            {
                uint temp;
                unsafe
                {
                    al.SourceUnqueueBuffers(_alSource, 1, &temp);
                    al.DeleteBuffer(temp);
                }
                value1--;
            }
            Thread.Sleep(5);
        }
        while (value - value1 > 100);
    }

    public void WaitDone()
    {
        int state, value;
        do
        {
            al.GetSourceProperty(_alSource, GetSourceInteger.BuffersQueued, out value);
            al.GetSourceProperty(_alSource, GetSourceInteger.SourceState, out state);
        }
        while (value == 0 && Media.Decoding == false &&
            (SourceState)state == SourceState.Stopped);
    }
}