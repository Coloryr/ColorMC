using ColorMC.Gui.MusicPlayer.Decoder;
using Silk.NET.SDL;
using System;
using System.Runtime.InteropServices;
using Thread = System.Threading.Thread;

namespace ColorMC.Gui.MusicPlayer.Players;

/// <summary>
/// SDL输出
/// </summary>
public class SdlPlayer
{
    private readonly uint _deviceId;
    private readonly AudioSpec audioSpec;
    private readonly Sdl _sdl;
    private readonly bool deviceOpen;

    private AudioCVT cvt;

    private int _lastChannel;
    private int _lastFreq;
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
                Channels = 2,
                Freq = 48000,
                Format = Sdl.AudioF32Sys
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


    private void AudioMakeCov(int chn, int freq)
    {
        if (_lastChannel == chn && _lastFreq == freq)
        {
            return;
        }

        ushort in_format = Sdl.AudioF32Sys;

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

        _tickCount = (int)(audioSpec.Freq * 0.1 * audioSpec.Channels * bps1);
    }

    private byte[] AudioCov(float[] input, int length)
    {
        unsafe
        {
            if (_lastLen != length)
            {
                if (cvt.Buf != null)
                {
                    _sdl.Free(cvt.Buf);
                }

                cvt.Buf = (byte*)_sdl.Malloc((nuint)(length * cvt.LenMult * sizeof(float)));
                _lastLen = length;
            }

            Marshal.Copy(input, 0, new nint(cvt.Buf), length);
            cvt.Len = length * sizeof(float);

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

    public void Write(SoundPack pack)
    {
        AudioMakeCov(pack.Channel, pack.SampleRate);
        var data = AudioCov(pack.Buff, pack.Length);

        _sdl.QueueAudio<byte>(_deviceId, data, (uint)data.Length);

        uint size = _sdl.GetQueuedAudioSize(_deviceId);
        while (size > _tickCount)
        {
            Thread.Sleep(1);
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
