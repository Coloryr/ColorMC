using OpenTK.Audio.OpenAL;
using System;
using System.Threading;

namespace ColorMC.Gui.Player;

public class OpenalPlayer : IPlayer
{
    private readonly int _alSource;
    private ALDevice _device;
    private ALContext _context;
    private bool _isPlay = false;

    public float Volume
    {
        set
        {
            AL.Source(_alSource, ALSourcef.Gain, value);
        }
    }

    public OpenalPlayer()
    {
        // Get the default device, then go though all devices and select the AL soft device if it exists.
        string deviceName = ALC.GetString(ALDevice.Null, AlcGetString.DefaultDeviceSpecifier);

        _device = ALC.OpenDevice(deviceName);
        int temp = 0;
        _context = ALC.CreateContext(_device, ref temp);
        ALC.MakeContextCurrent(_context);

        AL.GenSource(out _alSource);

        CheckALError();

        new Thread(() =>
        {
            while (true)
            {
                AL.GetSource(_alSource, ALGetSourcei.BuffersQueued, out int value);
                AL.GetSource(_alSource, ALGetSourcei.BuffersProcessed, out int value1);
                if (value - value1 > 100)
                {
                    int temp = AL.SourceUnqueueBuffer(_alSource);
                    AL.DeleteBuffer(temp);
                    Thread.Sleep(10);
                }
                if (_isPlay && value == 0 && Media.Decoding == false &&
                    AL.GetSourceState(_alSource) == ALSourceState.Stopped)
                {
                    _isPlay = false;
                    Media.PlayEnd();
                }
            }
        }).Start();
    }

    public void Close()
    {
        AL.DeleteSource(_alSource);

        ALC.MakeContextCurrent(ALContext.Null);
        ALC.DestroyContext(_context);
        ALC.CloseDevice(_device);
    }

    public static void CheckALError()
    {
        ALError error = AL.GetError();
        if (error != ALError.NoError)
        {
            App.ShowError($"ALError", new Exception(AL.GetErrorString(error)));
        }
    }

    public void Pause()
    {
        AL.SourcePause(_alSource);
    }

    public void Play()
    {
        AL.SourcePlay(_alSource);
    }

    public void Stop()
    {
        AL.Source(_alSource, ALSourcef.Gain, 0);
        AL.SourceStop(_alSource);

        AL.GetSource(_alSource, ALGetSourcei.BuffersQueued, out int value);
        while (value > 0)
        {
            int temp = AL.SourceUnqueueBuffer(_alSource);
            AL.DeleteBuffer(temp);
            value--;
        }
    }

    public void Write(int numChannels, int bitsPerSample, byte[] buff, int length, int sampleRate)
    {
        ALFormat format;

        if (numChannels == 1)
        {
            if (bitsPerSample == 8)
                format = ALFormat.Mono8;
            else if (bitsPerSample == 16)
                format = ALFormat.Mono16;
            else
            {
                return;
            }
        }
        else if (numChannels == 2)
        {
            if (bitsPerSample == 8)
                format = ALFormat.Stereo8;
            else if (bitsPerSample == 16)
                format = ALFormat.Stereo16;
            else
            {
                return;
            }
        }
        else
        {
            return;
        }

        AL.GenBuffer(out int alBuffer);
        AL.BufferData(alBuffer, format, new ReadOnlySpan<byte>(buff, 0, length), sampleRate);
        AL.SourceQueueBuffer(_alSource, alBuffer);

        if (AL.GetSourceState(_alSource) != ALSourceState.Playing)
        {
            AL.SourcePlay(_alSource);
            _isPlay = true;
        }
    }
}
