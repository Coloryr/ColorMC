using System;
using System.Threading;
using ColorMC.Gui.Manager;
using ColorMC.Gui.MusicPlayer.Decoder;
using Silk.NET.OpenAL;
using Silk.NET.OpenAL.Extensions.EXT;

namespace ColorMC.Gui.MusicPlayer.Players;

/// <summary>
/// OpenAl输出
/// </summary>
public class OpenALPlayer : IPlayer
{
    private readonly uint _alSource;
    private readonly unsafe Device* _device;
    private readonly unsafe Context* _context;
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

            _device = alc.OpenDevice(deviceName);
            _context = alc.CreateContext(_device, null);
            alc.MakeContextCurrent(_context);

            _alSource = al.GenSource();

            // 检查是否支持 AL_EXT_FLOAT32 扩展
            if (!al.IsExtensionPresent("AL_EXT_FLOAT32"))
            {
                alc.DestroyContext(_context);
                alc.CloseDevice(_device);
                throw new Exception("AL_EXT_FLOAT32 extension not supported");
            }
        }

        CheckALError();
    }

    public void Close()
    {
        al.DeleteSource(_alSource);

        unsafe
        {
            alc.MakeContextCurrent(null);
            alc.DestroyContext(_context);
            alc.CloseDevice(_device);
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

    public void Write(SoundPack pack)
    {
        FloatBufferFormat format;

        if (pack.Channel == 1)
        {
            format = FloatBufferFormat.Mono;
        }
        else if (pack.Channel == 2)
        {
            format = FloatBufferFormat.Stereo;
        }
        else
        {
            return;
        }

        unsafe
        {
            var alBuffer = al.GenBuffer();
            fixed (void* ptr = pack.Buff)
            {
                al.BufferData(alBuffer, format, ptr, pack.Length * 4, pack.SampleRate);
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
        while (value - value1 > 20);
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