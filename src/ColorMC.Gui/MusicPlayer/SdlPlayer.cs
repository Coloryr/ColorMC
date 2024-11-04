using System;
using System.Runtime.InteropServices;
using Silk.NET.SDL;
using Thread = System.Threading.Thread;

namespace ColorMC.Gui.MusicPlayer;

/// <summary>
/// SDL输出
/// </summary>
public class SdlPlayer : IPlayer
{
    private uint _deviceId;
    private AudioSpec audioSpec;
    private Sdl _sdl;
    private bool deviceOpen;
    private unsafe AudioCVT cvt;

    private int _lastChannel;
    private int _lastFreq;
    private int _lastBps;
    private int _lastLen;

    private int _tickCount;

    public float Volume { get; set; }

    public SdlPlayer(Sdl sdl)
    {
        _sdl = sdl;
        unsafe
        {
            AudioSpec spec1;
            var spec = new AudioSpec()
            {
                Format = Sdl.AudioS16Sys,
                Freq = 44100,
                Channels = 2,
                Samples = 1024
            };

            _deviceId = _sdl.OpenAudioDevice((byte*)0, 0, &spec, &spec1, (int)Sdl.AudioAllowAnyChange);
            if (_deviceId < 2)
            {
                return;
            }

            audioSpec = spec1;
            deviceOpen = true;
        }
    }

    public void Close()
    {
        if (_deviceId >= 2)
        {
            _sdl.CloseAudioDevice(_deviceId);
        }
    }

    public void Pause()
    {
        if (!deviceOpen)
        {
            return;
        }
        _sdl.PauseAudioDevice(_deviceId, 1);
    }

    public void Play()
    {
        if (!deviceOpen)
        {
            return;
        }
        _sdl.PauseAudioDevice(_deviceId, 0);
    }

    public void Stop()
    {
        if (!deviceOpen)
        {
            return;
        }

        _sdl.ClearQueuedAudio(_deviceId);
    }


    private void AudioMakeCov(int chn, int freq, int bps)
    {
        if (_lastChannel == chn && _lastFreq == freq && _lastBps == bps)
        {
            return;
        }

        ushort in_format;
        if (bps == 1)
        {
            in_format = Sdl.AudioU8;
        }
        else if (bps == 2)
        {
            in_format = Sdl.AudioS16Sys;
        }
        else if (bps == 4)
        {
            in_format = Sdl.AudioS32Sys;
        }
        else
        {
            throw new Exception("bps is Unsupported format");
        }

        unsafe
        {
            if (cvt.Buf != null)
            {
                _sdl.Free(cvt.Buf);
            }
            fixed (AudioCVT* ptr = &cvt)
            {
                if (_sdl.BuildAudioCVT(ptr, in_format, (byte)chn, freq,
                    audioSpec.Format, audioSpec.Channels, audioSpec.Freq) < 0)
                {
                    throw new Exception("cvt create fail");
                }
            }
            _lastChannel = chn;
            _lastFreq = freq;
            _lastBps = bps;
        }

        int bps1 = 0;
        switch (audioSpec.Format)
        {
            case Sdl.AudioS8:
            case Sdl.AudioU8:
                bps1 = 1;
                break;
            case Sdl.AudioS16:
            case Sdl.AudioS16Msb:
            case Sdl.AudioU16Msb:
                bps1 = 2;
                break;
            case Sdl.AudioS32:
            case Sdl.AudioS32Msb:
            case Sdl.AudioF32Sys:
            case Sdl.AudioF32Msb:
                bps1 = 4;
                break;
        }

        _tickCount = (int)(audioSpec.Freq * 0.01 * audioSpec.Channels * bps1);
    }

    private byte[] AudioCov(byte[] input, int length)
    {
        unsafe
        {
            if (_lastLen != length)
            {
                if (cvt.Buf != null)
                {
                    _sdl.Free(cvt.Buf);
                }

                cvt.Buf = (byte*)_sdl.Malloc((nuint)(length * cvt.LenMult));
                _lastLen = length;
            }

            Marshal.Copy(input, 0, new nint(cvt.Buf), length);
            cvt.Len = length;

            fixed (AudioCVT* ptr = &cvt)
            {
                if (_sdl.ConvertAudio(ptr) < 0)
                {
                    throw new Exception("Cov Fail");
                }
            }

            var buffer = new byte[cvt.LenCvt];

            fixed (byte* ptr = buffer)
            {
                int val = (int)(Volume * Sdl.MixMaxvolume);
                _sdl.MixAudioFormat(ptr, cvt.Buf, audioSpec.Format, (uint)cvt.LenCvt, val);
            }

            return buffer;
        }
    }

    public void Write(int numChannels, int bitsPerSample, byte[] buff, int length, int sampleRate)
    {
        AudioMakeCov(numChannels, sampleRate, bitsPerSample / 8);
        var data = AudioCov(buff, length);

        _sdl.QueueAudio<byte>(_deviceId, data, (uint)data.Length);

        uint size = _sdl.GetQueuedAudioSize(_deviceId);
        while (size > _tickCount)
        {
            Thread.Sleep(5);
            size = _sdl.GetQueuedAudioSize(_deviceId);
        }
    }

    public void WaitDone()
    {
        while (_sdl.GetQueuedAudioSize(_deviceId) > 0)
        {
            Thread.Sleep(100);
        }
    }
}
