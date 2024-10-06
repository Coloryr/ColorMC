using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ColorMC.Core.Utils;
using ColorMC.Gui.Player.Decoder.Mp3;
using Silk.NET.SDL;
using Thread = System.Threading.Thread;

namespace ColorMC.Gui.MusicPlayer;

/// <summary>
/// 音频播放
/// </summary>
public static class Media
{
    /// <summary>
    /// 音乐路径
    /// </summary>
    private static string s_musicFile;

    private static CancellationTokenSource s_cancel = new();

    /// <summary>
    /// 是否在解码中
    /// </summary>
    public static bool Decoding { get; private set; }

    /// <summary>
    /// 音量 0 - 1
    /// </summary>
    public static float Volume { get; set; }

    /// <summary>
    /// 循环播放
    /// </summary>
    public static bool Loop { get; set; }

    private static uint _deviceId;
    private static AudioSpec audioSpec;
    private static Sdl _sdl;
    private static bool deviceOpen;
    private static unsafe AudioCVT cvt;

    private static int _lastChannel;
    private static int _lastFreq;
    private static int _lastBps;
    private static int _lastLen;

    /// <summary>
    /// 初始化播放器
    /// </summary>
    public static void Init(Sdl sdl)
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

            _deviceId = sdl.OpenAudioDevice((byte*)0, 0, &spec, &spec1, (int)Sdl.AudioAllowAnyChange);
            if (_deviceId < 2)
            {
                return;
            }

            audioSpec = spec1;
            deviceOpen = true;
        }

        App.OnClose += Close;
    }

    /// <summary>
    /// 关闭播放器
    /// </summary>
    public static async void Close()
    {
        if (!deviceOpen)
        {
            return;
        }
        Stop();

        await Task.Run(() =>
        {
            while (Decoding)
            {
                Thread.Sleep(10);
            }
        });

        if (_deviceId >= 2)
        {
            _sdl.CloseAudioDevice(_deviceId);
        }
    }

    /// <summary>
    /// 暂停
    /// </summary>
    public static void Pause()
    {
        if (!deviceOpen)
        {
            return;
        }
        _sdl.PauseAudioDevice(_deviceId, 1);
    }

    /// <summary>
    /// 播放
    /// </summary>
    public static void Play()
    {
        if (!deviceOpen)
        {
            return;
        }
        _sdl.PauseAudioDevice(_deviceId, 0);
    }

    /// <summary>
    /// 停止
    /// </summary>
    public static void Stop()
    {
        if (!deviceOpen)
        {
            return;
        }
        s_cancel.Cancel();

        _sdl.ClearQueuedAudio(_deviceId);
    }

    private static void AudioMakeCov(int chn, int freq, int bps)
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
    }

    private static byte[] AudioCov(byte[] input, int length)
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

    /// <summary>
    /// 播放
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private static async Task<(bool, string?)> PlayWAV(string filePath)
    {
        if (!deviceOpen)
        {
            return (false, null);
        }

        Stop();

        //等待解码器停止
        await Task.Run(() =>
        {
            while (Decoding)
            {
                Thread.Sleep(10);
            }
        });

        s_cancel = new();

        Decoding = true;

        var file = File.OpenRead(filePath);
        var temp = new byte[4];
        file.Read(temp);
        if (temp[0] != 'R' || temp[1] != 'I' || temp[2] != 'F' || temp[3] != 'F')
        {
            return (false, "Given file is not in RIFF format");
        }

        file.Read(temp);
        var chunkSize = BinaryPrimitives.ReadInt32LittleEndian(temp);

        file.Read(temp);
        if (temp[0] != 'W' || temp[1] != 'A' || temp[2] != 'V' || temp[3] != 'E')
        {
            return (false, "Given file is not in WAVE format");
        }

        short numChannels = -1;
        int sampleRate = -1;
        short bitsPerSample = -1;
        int index = 12;

        while (index < file.Length)
        {
            file.Read(temp);
            index += 4;
            var identifier = "" + (char)temp[0] + (char)temp[1] + (char)temp[2] + (char)temp[3];
            file.Read(temp);
            index += 4;
            var size = BinaryPrimitives.ReadInt32LittleEndian(temp);
            if (identifier == "fmt ")
            {
                if (size != 16)
                {
                    return (false, $"Unknown Audio Format with subchunk1 size {size}");
                }
                else
                {
                    file.Read(temp, 0, 2);
                    var audioFormat = BinaryPrimitives.ReadInt16LittleEndian(temp);
                    index += 2;
                    if (audioFormat != 1)
                    {
                        return (false, $"Unknown Audio Format with ID {audioFormat}");
                    }
                    else
                    {
                        file.Read(temp, 0, 2);
                        index += 2;
                        numChannels = BinaryPrimitives.ReadInt16LittleEndian(temp);
                        file.Read(temp);
                        index += 4;
                        sampleRate = BinaryPrimitives.ReadInt32LittleEndian(temp);
                        file.Read(temp);
                        index += 4;
                        file.Read(temp, 0, 2);
                        index += 2;
                        file.Read(temp, 0, 2);
                        index += 2;
                        bitsPerSample = BinaryPrimitives.ReadInt16LittleEndian(temp);
                    }
                }
            }
            else if (identifier == "data")
            {
                int less = size;
                int pack = numChannels * bitsPerSample * 1000;
                temp = new byte[pack];
                _ = Task.Run(() =>
                {
                    for (int a = 0; a < size; a++)
                    {
                        var length = Math.Min(less, pack);
                        file.Read(temp, 0, length);
                        AudioMakeCov(numChannels, bitsPerSample, sampleRate);
                        var data = AudioCov(temp, length);

                        _sdl.QueueAudio<byte>(_deviceId, data, (uint)data.Length);

                        if (s_cancel.IsCancellationRequested)
                            break;

                        a += length;
                        less -= length;
                    }
                    file.Dispose();
                });
                break;
            }
            else
            {
                file.Seek(size, SeekOrigin.Current);
                index += size;
            }
        }

        Decoding = false;

        if (Loop)
        {
            _ = Task.Run(() =>
            {
                while (_sdl.GetQueuedAudioSize(_deviceId) > 0)
                {
                    Thread.Sleep(100);
                }
                Task.Run(() =>
                {
                    Thread.Sleep(500);
                    _ = PlayWAV(s_musicFile);
                });
            });
        }

        return (true, null);
    }

    /// <summary>
    /// 播放Mp3
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    private static async Task<(bool, string?)> PlayMp3(Stream stream)
    {
        if (!deviceOpen)
        {
            return (false, null);
        }

        Stop();

        await Task.Run(() =>
        {
            while (Decoding)
            {
                Thread.Sleep(10);
            }
        });

        s_cancel = new();

        var decoder = new Mp3Decoder(stream);
        if (!decoder.Load())
        {
            return (false, "mp3 file error");
        }

        _ = Task.Run(() =>
        {
            try
            {
                Decoding = true;
                int count = 0;
                Play();
                while (true)
                {
                    if (s_cancel.IsCancellationRequested)
                        break;
                    var frame = decoder.DecodeFrame();
                    if (frame == null || frame.Len <= 0 || s_cancel.IsCancellationRequested)
                    {
                        break;
                    }

                    AudioMakeCov(decoder.outputChannels, decoder.outputFrequency, 2);
                    var data = AudioCov(frame.Buff, frame.Len);

                    _sdl.QueueAudio<byte>(_deviceId, data, (uint)data.Length);

                    while (_sdl.GetQueuedAudioSize(_deviceId) > 50000)
                    {
                        Thread.Sleep(5);
                    }
                    count++;
                }
                decoder.Dispose();
                Decoding = false;
                if (Loop)
                {
                    while (_sdl.GetQueuedAudioSize(_deviceId) > 0)
                    {
                        Thread.Sleep(100);
                    }
                    Task.Run(() =>
                    {
                        Thread.Sleep(500);
                        _ = PlayUrl(s_musicFile);
                    });
                }
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("MediaPlayer.Error1"), e);
            }
        }, s_cancel.Token);

        return (true, null);
    }

    /// <summary>
    /// 播放
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    private static async Task<(bool, string?)> PlayMp3(string file)
    {
        if (!deviceOpen)
        {
            return (false, null);
        }

        var reader = File.OpenRead(file);
        return await PlayMp3(reader);
    }

    /// <summary>
    /// 播放
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private static async Task<(bool, string?)> PlayUrl(string url)
    {
        if (!deviceOpen)
        {
            return (false, null);
        }

        try
        {
            var res = await Core.Net.CoreHttpClient.DownloadClient.GetAsync(url);
            if (res.StatusCode == HttpStatusCode.Redirect)
            {
                var url1 = res.Headers.Location;
                res = await Core.Net.CoreHttpClient.DownloadClient.GetAsync(url1);
                return await PlayMp3(res.Content.ReadAsStream());
            }

            return await PlayMp3(res.Content.ReadAsStream());
        }
        catch (Exception e)
        {
            Logs.Error(App.Lang("MediaPlayer.Error2"), e);
            return (false, null);
        }
    }

    /// <summary>
    /// 播放音乐
    /// </summary>
    /// <param name="file">文件或者网址</param>
    /// <param name="value">是否渐变大声</param>
    /// <param name="value1">最大音量 0-100</param>
    public static async void PlayMusic(string file, bool value, int value1)
    {
        s_musicFile = file;

        bool play = false;
        Volume = 0;

        if (file.StartsWith("http://") || file.StartsWith("https://"))
        {
            await PlayUrl(file);
            play = true;
        }
        else
        {
            file = Path.GetFullPath(file);
            if (File.Exists(file))
            {
                if (file.EndsWith(".mp3"))
                {
                    await PlayMp3(file);
                    play = true;
                }
                else if (file.EndsWith(".wav"))
                {
                    await PlayWAV(file);
                    play = true;
                }
            }
        }
        if (play)
        {
            if (value)
            {
                await Task.Run(() =>
                {
                    for (int a = 0; a < value1; a++)
                    {
                        Volume = (float)a / 100;
                        Thread.Sleep(50);
                    }
                });
            }
            else
            {
                Volume = (float)value1 / 100;
            }
        }
    }
}