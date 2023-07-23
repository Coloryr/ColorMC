using NAudio.Wave;
using System.Threading;

namespace ColorMC.Gui.Player;

public class NAudioPlayer : IPlayer
{
    public float Volume { set => _waveOut.Volume = value; }

    private readonly WaveOutEvent _waveOut;
    private WaveFormat? _wf;
    private BufferedWaveProvider? _bwp;
    private bool _init = false;
    private bool _stop = false;
    private bool _isPlay = false;

    public NAudioPlayer()
    {
        _waveOut = new();

        new Thread(() =>
        {
            while (!App.IsClose)
            {
                if (_isPlay && Media.Decoding == false &&
                    _bwp?.BufferedDuration.TotalSeconds == 0)
                {
                    _isPlay = false;
                    Media.PlayEnd();
                }
                Thread.Sleep(100);
            }
        })
        {
            Name = "ColorMC_NAudio"
        }.Start();
    }

    public void Close()
    {
        _waveOut.Dispose();
    }

    public void Pause()
    {
        if (_init && _waveOut.PlaybackState != PlaybackState.Paused)
        {
            _waveOut.Pause();
        }
    }

    public void Play()
    {
        if (_init)
        {
            _waveOut.Play();
        }

    }

    public void Stop()
    {
        if (_init)
        {
            _waveOut.Stop();
        }
        _init = false;
        _wf = null;
        _bwp?.ClearBuffer();
        _bwp = null;
        _stop = true;
    }

    public void Write(int numChannels, int bitsPerSample, byte[] buff, int length, int sampleRate)
    {
        if (_wf == null)
        {
            _wf = new WaveFormat(sampleRate, bitsPerSample, numChannels);
            _bwp = new BufferedWaveProvider(_wf);
            _waveOut.Init(_bwp);
            _init = true;
            _stop = false;
        }
        while (_bwp?.BufferedDuration.TotalSeconds > 1)
        {
            Thread.Sleep(800);
        }
        if (_stop)
        {
            return;
        }
        _bwp!.AddSamples(buff, 0, length);
        if (_waveOut.PlaybackState == PlaybackState.Stopped)
        {
            _waveOut.Play();
            _isPlay = true;
        }
    }
}
