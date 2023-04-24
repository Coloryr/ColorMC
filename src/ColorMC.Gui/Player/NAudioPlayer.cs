using NAudio.Wave;
using System.Threading;

namespace ColorMC.Gui.Player;

public class NAudioPlayer : IPlayer
{
    public float Volume { set => waveOut.Volume = value; }

    private WaveOutEvent waveOut;
    private WaveFormat? wf;
    private BufferedWaveProvider? bwp;
    private bool init = false;
    private bool stop = false;

    public NAudioPlayer()
    {
        waveOut = new();
    }

    public void Close()
    {
        waveOut.Dispose();
    }

    public void Pause()
    {
        if (init)
        {
            waveOut.Pause();
        }
    }

    public void Play()
    {
        if (init)
        {
            waveOut.Play();
        }

    }

    public void Stop()
    {
        if (init)
        {
            waveOut.Stop();
        }
        init = false;
        wf = null;
        bwp?.ClearBuffer();
        bwp = null;
        stop = true;
    }

    public void Write(int numChannels, int bitsPerSample, byte[] buff, int length, int sampleRate)
    {
        if (wf == null)
        {
            wf = new WaveFormat(sampleRate, bitsPerSample, numChannels);
            bwp = new BufferedWaveProvider(wf);
            waveOut.Init(bwp);
            init = true;
            stop = false;
        }
        while (bwp?.BufferedDuration.TotalSeconds > 1)
        {
            Thread.Sleep(800);
        }
        if (stop)
        {
            return;
        }
        bwp!.AddSamples(buff, 0, length);
        if (waveOut.PlaybackState == PlaybackState.Stopped)
        {
            waveOut.Play();
        }
    }
}
