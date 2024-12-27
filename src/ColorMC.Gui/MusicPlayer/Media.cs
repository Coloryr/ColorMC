using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.MusicPlayer.Decoder;
using ColorMC.Gui.MusicPlayer.Decoder.Flac;
using ColorMC.Gui.MusicPlayer.Decoder.Mp3;
using ColorMC.Gui.MusicPlayer.Decoder.Wav;
using ColorMC.Gui.MusicPlayer.Players;
using ColorMC.Gui.Objs;
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

    private static async Task<MusicPlayRes> Play(Stream stream, bool isurl, MediaType type)
    {
        Stop();

        await Task.Run(() =>
        {
            while (Decoding)
            {
                Thread.Sleep(10);
            }
        });

        s_cancel = new();

        IDecoder decoder = type switch
        {
            MediaType.Wav => new WavFile(stream),
            MediaType.Mp3 => new Mp3File(stream),
            MediaType.Flac => new FlacFile(stream),
            _ => throw new Exception("unknow file decoder")
        };
        if (!decoder.IsFile)
        {
            return new MusicPlayRes()
            {
                Message = App.Lang("MediaPlayer.Error5")
            };
        }

        MusicTime = TimeSpan.FromSeconds(decoder.GetTimeCount());
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
                    if (frame == null || frame.Length <= 0 || s_cancel.IsCancellationRequested)
                    {
                        break;
                    }

                    s_player?.Write(frame);

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
                            _ = Play(s_musicFile);
                        }
                    });
                }
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("MediaPlayer.Error1"), e);
            }
        }, s_cancel.Token);

        return new MusicPlayRes()
        {
            Res = true,
            MusicInfo = decoder.GetInfo()
        };
    }

    private static async Task<MusicPlayRes> Play(string file)
    {
        var reader = File.OpenRead(file);
        MediaType type = TestMediaType(reader);
        reader.Seek(0, SeekOrigin.Begin);
        return await Play(reader, false, type);
    }

    private static MediaType TestMediaType(Stream stream)
    {
        var temp = new byte[4];
        stream.ReadExactly(temp);
        if (temp[0] == 'R' && temp[1] == 'I' && temp[2] == 'F' && temp[3] == 'F')
        {
            return MediaType.Wav;
        }
        else if (temp[0] == 'f' && temp[1] == 'L' && temp[2] == 'a' && temp[3] == 'C')
        {
            return MediaType.Flac;
        }
        else if (temp[0] == 'I' && temp[1] == 'D' && temp[2] == '3')
        {
            return MediaType.Mp3;
        }
        else if (temp[0] == 0xFF && temp[1] == 0xE0)
        {
            return MediaType.Mp3;
        }

        return MediaType.Unknow;
    }

    /// <summary>
    /// 播放
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    private static async Task<MusicPlayRes> PlayUrl(string url)
    {
        Stream stream;
        var res = await CoreHttpClient.DownloadClient.GetAsync(url);
        if (res.StatusCode == HttpStatusCode.Redirect)
        {
            var url1 = res.Headers.Location;
            res = await CoreHttpClient.DownloadClient.GetAsync(url1);
            stream = new BufferedStream(res.Content.ReadAsStream());
        }
        else
        {
            stream = new BufferedStream(res.Content.ReadAsStream());
        }

        MediaType type = TestMediaType(stream);
        stream.Seek(0, SeekOrigin.Begin);

        return await Play(res.Content.ReadAsStream(), true, type);
    }

    /// <summary>
    /// 播放音乐
    /// </summary>
    /// <param name="file">文件或者网址</param>
    /// <param name="value">是否渐变大声</param>
    /// <param name="value1">最大音量 0-100</param>
    public static async Task<MusicPlayRes> PlayMusic(string file, bool value, int value1)
    {
        if (s_player == null)
        {
            return new MusicPlayRes()
            {
                Message = App.Lang("MediaPlayer.Error4")
            };
        }

        var file1 = Path.GetFileName(file);
        var res = new MusicPlayRes();

        s_musicFile = file;

        bool play = false;
        Volume = 0;

        try
        {
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
                    res = await Play(file);
                    if (res.Res)
                    {
                        play = true;
                    }
                }
            }
        }
        catch (Exception e)
        {
            string text = App.Lang("MediaPlayer.Error2");
            Logs.Error(text, e);
            res.Message = text;
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

        if (res.Res)
        {
            res.Message = file1;
        }

        return res;
    }
}