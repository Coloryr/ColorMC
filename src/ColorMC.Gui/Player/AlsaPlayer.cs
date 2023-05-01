using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using static ColorMC.Gui.Player.Decoder.Mp3.Mp3Decoder;

namespace ColorMC.Gui.Player;

public enum snd_pcm_stream_t
{
    SND_PCM_STREAM_PLAYBACK = 0,
    SND_PCM_STREAM_CAPTURE = 1,
    SND_PCM_STREAM_LAST = SND_PCM_STREAM_CAPTURE,
}

public enum snd_pcm_format_t
{
    SND_PCM_FORMAT_UNKNOWN = -1,
    SND_PCM_FORMAT_S8 = 0,
    SND_PCM_FORMAT_U8 = 1,
    SND_PCM_FORMAT_S16_LE = 2,
    SND_PCM_FORMAT_S16_BE = 3,
    SND_PCM_FORMAT_U16_LE = 4,
    SND_PCM_FORMAT_U16_BE = 5,
    SND_PCM_FORMAT_S24_LE = 6,
    SND_PCM_FORMAT_S24_BE = 7,
    SND_PCM_FORMAT_U24_LE = 8,
    SND_PCM_FORMAT_U24_BE = 9,
    SND_PCM_FORMAT_S32_LE = 10,
    SND_PCM_FORMAT_S32_BE = 11,
    SND_PCM_FORMAT_U32_LE = 12,
    SND_PCM_FORMAT_U32_BE = 13,
    SND_PCM_FORMAT_FLOAT_LE = 14,
    SND_PCM_FORMAT_FLOAT_BE = 15,
    SND_PCM_FORMAT_FLOAT64_LE = 16,
    SND_PCM_FORMAT_FLOAT64_BE = 17,
    SND_PCM_FORMAT_IEC958_SUBFRAME_LE = 18,
    SND_PCM_FORMAT_IEC958_SUBFRAME_BE = 19,
    SND_PCM_FORMAT_MU_LAW = 20,
    SND_PCM_FORMAT_A_LAW = 21,
    SND_PCM_FORMAT_IMA_ADPCM = 22,
    SND_PCM_FORMAT_MPEG = 23,
    SND_PCM_FORMAT_GSM = 24,
    SND_PCM_FORMAT_SPECIAL = 31,
    SND_PCM_FORMAT_S24_3LE = 32,
    SND_PCM_FORMAT_S24_3BE = 33,
    SND_PCM_FORMAT_U24_3LE = 34,
    SND_PCM_FORMAT_U24_3BE = 35,
    SND_PCM_FORMAT_S20_3LE = 36,
    SND_PCM_FORMAT_S20_3BE = 37,
    SND_PCM_FORMAT_U20_3LE = 38,
    SND_PCM_FORMAT_U20_3BE = 39,
    SND_PCM_FORMAT_S18_3LE = 40,
    SND_PCM_FORMAT_S18_3BE = 41,
    SND_PCM_FORMAT_U18_3LE = 42,
    SND_PCM_FORMAT_U18_3BE = 43,
    SND_PCM_FORMAT_G723_24 = 44,
    SND_PCM_FORMAT_G723_24_1B = 45,
    SND_PCM_FORMAT_G723_40 = 46,
    SND_PCM_FORMAT_G723_40_1B = 47,
    SND_PCM_FORMAT_DSD_U8 = 48,
    SND_PCM_FORMAT_DSD_U16_LE = 49,
    SND_PCM_FORMAT_DSD_U32_LE = 50,
    SND_PCM_FORMAT_DSD_U16_BE = 51,
    SND_PCM_FORMAT_DSD_U32_BE = 52,
    SND_PCM_FORMAT_LAST = SND_PCM_FORMAT_DSD_U32_BE,
}

public enum snd_pcm_access_t
{
    SND_PCM_ACCESS_MMAP_INTERLEAVED = 0,
    SND_PCM_ACCESS_MMAP_NONINTERLEAVED = 1,
    SND_PCM_ACCESS_MMAP_COMPLEX = 2,
    SND_PCM_ACCESS_RW_INTERLEAVED = 3,
    SND_PCM_ACCESS_RW_NONINTERLEAVED = 4,
    SND_PCM_ACCESS_LAST = SND_PCM_ACCESS_RW_NONINTERLEAVED,
}

public enum snd_mixer_selem_channel_id
{
    SND_MIXER_SCHN_UNKNOWN = -1,
    SND_MIXER_SCHN_FRONT_LEFT = 0,
    SND_MIXER_SCHN_FRONT_RIGHT = 1,
    SND_MIXER_SCHN_REAR_LEFT = 2,
    SND_MIXER_SCHN_REAR_RIGHT = 3,
    SND_MIXER_SCHN_FRONT_CENTER = 4,
    SND_MIXER_SCHN_WOOFER = 5,
    SND_MIXER_SCHN_SIDE_LEFT = 6,
    SND_MIXER_SCHN_SIDE_RIGHT = 7,
    SND_MIXER_SCHN_REAR_CENTER = 8,
    SND_MIXER_SCHN_LAST = 31,
    SND_MIXER_SCHN_MONO = SND_MIXER_SCHN_FRONT_LEFT
}

public static class InteropAlsa
{
    const string AlsaLibrary = "libasound";

    [DllImport(AlsaLibrary)]
    public static extern IntPtr snd_strerror(int errnum);

    [DllImport(AlsaLibrary)]
    public static extern int snd_pcm_open(ref IntPtr pcm, string name, snd_pcm_stream_t stream, int mode);

    [DllImport(AlsaLibrary)]
    public static extern int snd_pcm_start(IntPtr pcm);

    [DllImport(AlsaLibrary)]
    public static extern int snd_pcm_pause(IntPtr pcm, int enable);

    [DllImport(AlsaLibrary)]
    public static extern int snd_pcm_resume(IntPtr pcm);

    [DllImport(AlsaLibrary)]
    public static extern int snd_pcm_drain(IntPtr pcm);

    [DllImport(AlsaLibrary)]
    public static extern int snd_pcm_drop(IntPtr pcm);

    [DllImport(AlsaLibrary)]
    public static extern int snd_pcm_close(IntPtr pcm);

    [DllImport(AlsaLibrary)]
    public static extern int snd_pcm_recover(IntPtr pcm, int err, int silent);

    [DllImport(AlsaLibrary)]
    public static extern int snd_pcm_writei(IntPtr pcm, IntPtr buffer, ulong size);

    [DllImport(AlsaLibrary)]
    public static extern int snd_pcm_readi(IntPtr pcm, IntPtr buffer, ulong size);

    [DllImport(AlsaLibrary)]
    public static extern int snd_pcm_set_params(IntPtr pcm, snd_pcm_format_t format, snd_pcm_access_t access, uint channels, uint rate, int soft_resample, uint latency);

    [DllImport(AlsaLibrary)]
    public static extern int snd_pcm_hw_params_malloc(ref IntPtr @params);

    [DllImport(AlsaLibrary)]
    public static extern int snd_pcm_hw_params_any(IntPtr pcm, IntPtr @params);

    [DllImport(AlsaLibrary)]
    public static extern int snd_pcm_hw_params_set_access(IntPtr pcm, IntPtr @params, snd_pcm_access_t access);

    [DllImport(AlsaLibrary)]
    public static extern int snd_pcm_hw_params_set_format(IntPtr pcm, IntPtr @params, snd_pcm_format_t val);

    [DllImport(AlsaLibrary)]
    public static extern int snd_pcm_hw_params_set_channels(IntPtr pcm, IntPtr @params, uint val);

    [DllImport(AlsaLibrary)]
    public static extern unsafe int snd_pcm_hw_params_set_rate_near(IntPtr pcm, IntPtr @params, uint* val, int* dir);

    [DllImport(AlsaLibrary)]
    public static extern int snd_pcm_hw_params(IntPtr pcm, IntPtr @params);

    [DllImport(AlsaLibrary)]
    public static extern unsafe int snd_pcm_hw_params_get_period_size(IntPtr @params, ulong* frames, int* dir);

    [DllImport(AlsaLibrary)]
    public static extern unsafe int snd_pcm_hw_params_set_period_size_near(IntPtr pcm, IntPtr @params, ulong* frames, int* dir);

    [DllImport(AlsaLibrary)]
    public static extern int snd_mixer_open(ref IntPtr mixer, int mode);

    [DllImport(AlsaLibrary)]
    public static extern int snd_mixer_close(IntPtr mixer);

    [DllImport(AlsaLibrary)]
    public static extern int snd_mixer_attach(IntPtr mixer, string name);

    [DllImport(AlsaLibrary)]
    public static extern int snd_mixer_load(IntPtr mixer);

    [DllImport(AlsaLibrary)]
    public static extern int snd_mixer_selem_register(IntPtr mixer, IntPtr options, IntPtr classp);

    [DllImport(AlsaLibrary)]
    public static extern IntPtr snd_mixer_first_elem(IntPtr mixer);

    [DllImport(AlsaLibrary)]
    public static extern IntPtr snd_mixer_elem_next(IntPtr elem);

    [DllImport(AlsaLibrary)]
    public static extern string snd_mixer_selem_get_name(IntPtr elem);

    [DllImport(AlsaLibrary)]
    public static extern void snd_mixer_selem_id_alloca(IntPtr ptr);

    [DllImport(AlsaLibrary)]
    public static extern unsafe int snd_mixer_selem_get_playback_volume(IntPtr elem, snd_mixer_selem_channel_id channel, long* value);

    [DllImport(AlsaLibrary)]
    public static extern int snd_mixer_selem_set_playback_volume(IntPtr elem, snd_mixer_selem_channel_id channel, long value);

    [DllImport(AlsaLibrary)]
    public static extern int snd_mixer_selem_set_playback_volume_all(IntPtr elem, long value);

    [DllImport(AlsaLibrary)]
    public static extern int snd_mixer_selem_set_playback_switch_all(IntPtr elem, int value);

    [DllImport(AlsaLibrary)]
    public static extern unsafe int snd_mixer_selem_get_playback_volume_range(IntPtr elem, long* min, long* max);

    [DllImport(AlsaLibrary)]
    public static extern int snd_mixer_selem_set_playback_volume_range(IntPtr elem, long min, long max);

    [DllImport(AlsaLibrary)]
    public static extern unsafe int snd_mixer_selem_get_capture_volume(IntPtr elem, snd_mixer_selem_channel_id channel, long* value);

    [DllImport(AlsaLibrary)]
    public static extern int snd_mixer_selem_set_capture_volume(IntPtr elem, snd_mixer_selem_channel_id channel, long value);

    [DllImport(AlsaLibrary)]
    public static extern int snd_mixer_selem_set_capture_volume_all(IntPtr elem, long value);

    [DllImport(AlsaLibrary)]
    public static extern int snd_mixer_selem_set_capture_switch_all(IntPtr elem, int value);

    [DllImport(AlsaLibrary)]
    public static extern unsafe int snd_mixer_selem_get_capture_volume_range(IntPtr elem, long* min, long* max);

    [DllImport(AlsaLibrary)]
    public static extern int snd_mixer_selem_set_capture_volume_range(IntPtr elem, long min, long max);
}


public class AlsaPlayer : IPlayer
{
    static readonly object PlaybackInitializationLock = new();
    static readonly object RecordingInitializationLock = new();
    static readonly object MixerInitializationLock = new();

    public long PlaybackVolume { get => GetPlaybackVolume(); set => SetPlaybackVolume(value); }
    public bool PlaybackMute { get => _playbackMute; set => SetPlaybackMute(value); }

    bool _playbackMute;
    IntPtr _playbackPcm;
    IntPtr pcm;
    IntPtr _mixer;
    IntPtr _mixelElement;
    IntPtr arg;
    int dir;
    bool _wasDisposed;

    public float Volume { set => value = value; }

    public AlsaPlayer()
    {
        OpenPlaybackPcm("default");
    }

    void SetPlaybackVolume(long volume)
    {
        OpenMixer("default");

        ThrowErrorMessage(InteropAlsa.snd_mixer_selem_set_playback_volume(_mixelElement, snd_mixer_selem_channel_id.SND_MIXER_SCHN_FRONT_LEFT, volume), "CanNotSetVolume");
        ThrowErrorMessage(InteropAlsa.snd_mixer_selem_set_playback_volume(_mixelElement, snd_mixer_selem_channel_id.SND_MIXER_SCHN_FRONT_RIGHT, volume), "CanNotSetVolume");

        CloseMixer();
    }

    unsafe long GetPlaybackVolume()
    {
        long volumeLeft;
        long volumeRight;

        OpenMixer("default");

        ThrowErrorMessage(InteropAlsa.snd_mixer_selem_get_playback_volume(_mixelElement, snd_mixer_selem_channel_id.SND_MIXER_SCHN_FRONT_LEFT, &volumeLeft), "CanNotSetVolume");
        ThrowErrorMessage(InteropAlsa.snd_mixer_selem_get_playback_volume(_mixelElement, snd_mixer_selem_channel_id.SND_MIXER_SCHN_FRONT_RIGHT, &volumeRight), "CanNotSetVolume");

        CloseMixer();

        return (volumeLeft + volumeRight) / 2;
    }

    void SetPlaybackMute(bool isMute)
    {
        _playbackMute = isMute;

        OpenMixer("default");

        ThrowErrorMessage(InteropAlsa.snd_mixer_selem_set_playback_switch_all(_mixelElement, isMute ? 0 : 1), "CanNotSetMute");

        CloseMixer();
    }

    void OpenPlaybackPcm(string name)
    {
        if (_playbackPcm != default)
            return;

        lock (PlaybackInitializationLock)
            ThrowErrorMessage(InteropAlsa.snd_pcm_open(ref _playbackPcm, name, snd_pcm_stream_t.SND_PCM_STREAM_PLAYBACK, 0), "CanNotOpenPlayback");
    }

    void ClosePlaybackPcm()
    {
        if (_playbackPcm == default)
            return;

        ThrowErrorMessage(InteropAlsa.snd_pcm_drain(_playbackPcm), "CanNotDropDevice");
        ThrowErrorMessage(InteropAlsa.snd_pcm_close(_playbackPcm), "CanNotCloseDevice");

        _playbackPcm = default;
    }

    void OpenMixer(string name)
    {
        if (_mixer != default)
            return;

        lock (MixerInitializationLock)
        {
            ThrowErrorMessage(InteropAlsa.snd_mixer_open(ref _mixer, 0), "CanNotOpenMixer");
            ThrowErrorMessage(InteropAlsa.snd_mixer_attach(_mixer, name), "CanNotAttachMixer");
            ThrowErrorMessage(InteropAlsa.snd_mixer_selem_register(_mixer, IntPtr.Zero, IntPtr.Zero), "CanNotRegisterMixer");
            ThrowErrorMessage(InteropAlsa.snd_mixer_load(_mixer), "CanNotLoadMixer");

            _mixelElement = InteropAlsa.snd_mixer_first_elem(_mixer);
        }
    }

    void CloseMixer()
    {
        if (_mixer == default)
            return;

        ThrowErrorMessage(InteropAlsa.snd_mixer_close(_mixer), "CanNotCloseMixer");

        _mixer = default;
        _mixelElement = default;
    }

    public void Dispose()
    {
        if (_wasDisposed)
            return;

        _wasDisposed = true;

        ClosePlaybackPcm();
        CloseMixer();
    }

    void ThrowErrorMessage(int errorNum, string message)
    {
        if (errorNum >= 0)
            return;

        var errorMsg = Marshal.PtrToStringAnsi(InteropAlsa.snd_strerror(errorNum));
        throw new Exception($"{message}. Error {errorNum}. {errorMsg}.");
    }

    public void Close()
    {

    }

    public void Pause()
    {

    }

    public void Play()
    {

    }

    public void Stop()
    {

    }

    public unsafe void Write(int numChannels, int bitsPerSample, byte[] buff, int length, int sampleRate)
    {
        if (arg == default)
        {
            ThrowErrorMessage(InteropAlsa.snd_pcm_hw_params_malloc(ref arg), "CanNotAllocateParameters");
            ThrowErrorMessage(InteropAlsa.snd_pcm_hw_params_any(pcm, arg), "CanNotFillParameters");
            ThrowErrorMessage(InteropAlsa.snd_pcm_hw_params_set_access(pcm, arg, snd_pcm_access_t.SND_PCM_ACCESS_RW_INTERLEAVED), "CanNotSetAccessMode");

            var formatResult = bitsPerSample / 8 switch
            {
                1 => InteropAlsa.snd_pcm_hw_params_set_format(pcm, arg, snd_pcm_format_t.SND_PCM_FORMAT_U8),
                2 => InteropAlsa.snd_pcm_hw_params_set_format(pcm, arg, snd_pcm_format_t.SND_PCM_FORMAT_S16_LE),
                3 => InteropAlsa.snd_pcm_hw_params_set_format(pcm, arg, snd_pcm_format_t.SND_PCM_FORMAT_S24_LE),
                _ => throw new Exception("BitsPerSampleError")
            };
            ThrowErrorMessage(formatResult, "CanNotSetFormat");

            ThrowErrorMessage(InteropAlsa.snd_pcm_hw_params_set_channels(pcm, arg, (uint)numChannels), "CanNotSetChannel");

            uint val = (uint)sampleRate;

            fixed (int* dirP = &dir)
                ThrowErrorMessage(InteropAlsa.snd_pcm_hw_params_set_rate_near(pcm, arg, &val, dirP), "CanNotSetRate");

            ThrowErrorMessage(InteropAlsa.snd_pcm_hw_params(pcm, arg), "CanNotSetHwParams");
        }
        ulong frames = (ulong)length;

        fixed (int* dirP = &dir)
            ThrowErrorMessage(InteropAlsa.snd_pcm_hw_params_get_period_size(arg, &frames, dirP), "CanNotGetPeriodSize");

        fixed (byte* buffer = buff)
        {
            ThrowErrorMessage(InteropAlsa.snd_pcm_writei(_playbackPcm, (IntPtr)buffer, frames), "CanNotWriteToDevice");
        }
    }
}
