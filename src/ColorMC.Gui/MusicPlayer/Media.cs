using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Player.Decoder.Mp3;
using ColorMC.Gui.Utils;

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

    /// <summary>
    /// 输出流
    /// </summary>
    private static IPlayer? s_player;

    private static CancellationTokenSource s_cancel = new();

    /// <summary>
    /// 是否在解码中
    /// </summary>
    public static bool Decoding { get; private set; }

    /// <summary>
    /// 音量 0 - 1
    /// </summary>
    public static float Volume
    {
        set 
        {
            if (s_player != null)
            {
                s_player.Volume = value;
            }
        }
    }

    /// <summary>
    /// 循环播放
    /// </summary>
    public static bool Loop { get; set; }

    public static TimeSpan NowTime { get; private set; } = TimeSpan.Zero;
    public static TimeSpan MusicTime { get; private set; } = TimeSpan.Zero;

    public static string MusicName { get; private set; }

    /// <summary>
    /// 初始化播放器
    /// </summary>
    public static void Init()
    {
        if (SystemInfo.Os != OsType.MacOS)
        {
            if (SdlUtils.SdlInit)
            {
                s_player = new SdlPlayer(SdlUtils.Sdl);
            }
            else
            {
                return;
            }
        }
        else
        {
            s_player = new OpenALPlayer();
        }

        App.OnClose += Close;
    }

    /// <summary>
    /// 关闭播放器
    /// </summary>
    public static async void Close()
    {
        Stop();

        await Task.Run(() =>
        {
            while (Decoding)
            {
                Thread.Sleep(10);
            }
        });

        s_player?.Close();
    }

    /// <summary>
    /// 暂停
    /// </summary>
    public static void Pause()
    {
        s_player?.Pause();
    }

    /// <summary>
    /// 播放
    /// </summary>
    public static void Play()
    {
        s_player?.Play();
    }

    /// <summary>
    /// 停止
    /// </summary>
    public static void Stop()
    {
        s_cancel.Cancel();

        s_player?.Stop();
    }

    /// <summary>
    /// 播放
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private static async Task<(bool, string?)> PlayWAV(string filePath)
    {
        //没有音频输出
        if (s_player == null)
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

                        s_player?.Write(numChannels, bitsPerSample, temp, length, sampleRate);


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
                s_player?.WaitDone();
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
    private static async Task<(bool, string?)> PlayMp3(Stream stream, bool isurl)
    {
        if (s_player == null)
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

        if (isurl)
        {
            stream = new BufferedStream(stream);
        }

        var decoder = new Mp3Decoder(stream);
        if (!decoder.Load())
        {
            return (false, "mp3 file error");
        }
        
        MusicTime = TimeSpan.FromMilliseconds(decoder.GetTimeCount());
        NowTime = TimeSpan.Zero;

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

                    s_player?.Write(decoder.outputChannels, 16, frame.Buff, frame.Len, decoder.outputFrequency);

                    NowTime += TimeSpan.FromMilliseconds(frame.Time);

                    count++;
                }
                decoder.Dispose();
                Decoding = false;
                if (Loop)
                {
                    s_player?.WaitDone();
                    Task.Run(() =>
                    {
                        if (isurl)
                        {
                            _ = PlayUrl(s_musicFile);
                        }
                        else
                        {
                            _ = PlayMp3(s_musicFile);
                        }
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
        if (s_player == null)
        {
            return (false, null);
        }

        var reader = File.OpenRead(file);
        return await PlayMp3(reader, false);
    }

    /// <summary>
    /// 播放
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private static async Task<(bool, string?)> PlayUrl(string url)
    {
        if (s_player == null)
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
                return await PlayMp3(res.Content.ReadAsStream(), true);
            }

            return await PlayMp3(res.Content.ReadAsStream(), true);
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
        if (s_player == null)
        {
            return;
        }

        MusicName = Path.GetFileName(file);

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