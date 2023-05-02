using OpenTK.Audio.OpenAL;
using System;
using System.Threading;

namespace ColorMC.Gui.Player;

public class OpenalPlayer : IPlayer
{
    private readonly int alSource;
    private ALDevice device;
    private ALContext context;

    public float Volume
    {
        set
        {
            AL.Source(alSource, ALSourcef.Gain, value);
        }
    }

    public OpenalPlayer()
    {
        // Get the default device, then go though all devices and select the AL soft device if it exists.
        string deviceName = ALC.GetString(ALDevice.Null, AlcGetString.DefaultDeviceSpecifier);

        device = ALC.OpenDevice(deviceName);
        int temp = 0;
        context = ALC.CreateContext(device, ref temp);
        ALC.MakeContextCurrent(context);

        AL.GenSource(out alSource);

        CheckALError();

        new Thread(() =>
        {
            while (true)
            {
                AL.GetSource(alSource, ALGetSourcei.BuffersQueued, out int value);
                AL.GetSource(alSource, ALGetSourcei.BuffersProcessed, out int value1);
                if (value - value1 > 100)
                {
                    int temp = AL.SourceUnqueueBuffer(alSource);
                    AL.DeleteBuffer(temp);
                    Thread.Sleep(10);
                }
            }
        }).Start();
    }

    public void Close()
    {
        AL.DeleteSource(alSource);

        ALC.MakeContextCurrent(ALContext.Null);
        ALC.DestroyContext(context);
        ALC.CloseDevice(device);
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
        AL.SourcePause(alSource);
    }

    public void Play()
    {
        AL.SourcePlay(alSource);
    }

    public void Stop()
    {
        AL.Source(alSource, ALSourcef.Gain, 0);
        AL.SourceStop(alSource);

        AL.GetSource(alSource, ALGetSourcei.BuffersQueued, out int value);
        while (value > 0)
        {
            int temp = AL.SourceUnqueueBuffer(alSource);
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
        AL.SourceQueueBuffer(alSource, alBuffer);

        if (AL.GetSourceState(alSource) != ALSourceState.Playing)
        {
            AL.SourcePlay(alSource);
        }
    }
}
