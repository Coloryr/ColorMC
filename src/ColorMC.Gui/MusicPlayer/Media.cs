using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Animation;
using ColorMC.Core.Net;
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

    private static PlayState _playState;

    /// <summary>
    /// 播放状态
    /// </summary>
    public static PlayState PlayState
    {
        get
        {
            return _playState;
        }
        set
        {
            if (_playState == value)
            {
                return;
            }
            _playState = value;
            //切换状态
            if (_playState == PlayState.Run)
            {
                s_player?.Play();
            }
            else if (_playState == PlayState.Pause)
            {
                s_player?.Pause();
            }
            else if (_playState == PlayState.Stop)
            {
                s_cancel.Cancel();
                s_player?.Stop();
            }
        }
    }

    /// <summary>
    /// 当前播放时间
    /// </summary>
    public static TimeSpan NowTime { get; private set; }
    /// <summary>
    /// 歌曲长度
    /// </summary>
    public static TimeSpan MusicTime { get; private set; }

    /// <summary>
    /// 输出流
    /// </summary>
    private static SdlPlayer? s_player;
    /// <summary>
    /// 取消播放
    /// </summary>
    private static CancellationTokenSource s_cancel = new();

    /// <summary>
    /// 播放线程
    /// </summary>
    private static readonly Thread s_thread = new(Run)
    {
        Name = "ColorMC Music"
    };
    /// <summary>
    /// 播放通知
    /// </summary>
    private static readonly Semaphore s_semaphore = new(0, 2);
    /// <summary>
    /// 解码器
    /// </summary>
    private static IDecoder decoder;
    /// <summary>
    /// 是否关闭
    /// </summary>
    private static bool s_isClose;

    /// <summary>
    /// 初始化播放器
    /// </summary>
    public static void Init()
    {
        try
        {
            if (SdlUtils.SdlInit)
            {
                s_player = new SdlPlayer(SdlUtils.Sdl);
            }
        }
        catch
        {
            s_player = null;
            return;
        }

        App.OnClose += Close;

        s_thread.Start();
    }

    /// <summary>
    /// 关闭播放器
    /// </summary>
    public static async void Close()
    {
        s_isClose = true;
        PlayState = PlayState.Stop;

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
    /// 播放器运行
    /// </summary>
    private static void Run()
    {
        while (!s_isClose)
        {
            s_semaphore.WaitOne();
            try
            {
                Decoding = true;
            Play:
                int count = 0;
                PlayState = PlayState.Run;
                while (true)
                {
                    //解码然后写进声卡
                    if (s_cancel.IsCancellationRequested)
                    {
                        break;
                    }
                    var frame = decoder.DecodeFrame();
                    if (frame == null || frame.Length <= 0 || s_cancel.IsCancellationRequested)
                    {
                        break;
                    }

                    s_player?.Write(frame);

                    NowTime += TimeSpan.FromSeconds(frame.Time);

                    count++;
                }
                s_player?.WaitDone();
                if (Loop)
                {
                    //循环播放
                    NowTime = TimeSpan.Zero;
                    decoder.Reset();
                    goto Play;
                }
                PlayState = PlayState.Stop;
                decoder.Dispose();
                Decoding = false;
            }
            catch (Exception e)
            {
                PlayState = PlayState.Stop;
                Logs.Error(App.Lang("MediaPlayer.Error1"), e);
            }
        }
    }

    /// <summary>
    /// 播放音乐
    /// </summary>
    /// <param name="stream">文件流</param>
    /// <param name="type">播放类型</param>
    /// <returns>播放结果</returns>
    /// <exception cref="Exception">文件错误</exception>
    private static async Task<MusicPlayRes> Play(Stream stream, MediaType type)
    {
        PlayState = PlayState.Stop;

        //等待播放结束
        await Task.Run(() =>
        {
            while (Decoding)
            {
                Thread.Sleep(10);
            }
        });

        s_cancel = new();

        //解码文件
        decoder = type switch
        {
            MediaType.Wav => new WavFile(stream),
            MediaType.Mp3 => new Mp3File(stream),
            MediaType.Flac => new FlacFile(stream),
            _ => throw new Exception("unknow file decoder")
        };
        if (!decoder.IsChek)
        {
            return new MusicPlayRes()
            {
                Message = App.Lang("MediaPlayer.Error5")
            };
        }

        //获取文件长度
        MusicTime = TimeSpan.FromSeconds(decoder.GetTimeCount());
        NowTime = TimeSpan.Zero;

        //开始解码并播放
        s_semaphore.Release();

        return new MusicPlayRes()
        {
            Res = true,
            MusicInfo = decoder.GetInfo()
        };
    }

    /// <summary>
    /// 播放文件
    /// </summary>
    /// <param name="file">文件</param>
    /// <returns>播放结果</returns>
    private static async Task<MusicPlayRes> Play(string file)
    {
        var reader = File.OpenRead(file);
        var type = TestMediaType(reader);
        reader.Seek(0, SeekOrigin.Begin);
        return await Play(reader, type);
    }

    /// <summary>
    /// 获取音乐文件类型
    /// </summary>
    /// <param name="stream">流</param>
    /// <returns>类型</returns>
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
    /// 播放网址
    /// </summary>
    /// <param name="url">网址</param>
    /// <returns>播放结果</returns>
    private static async Task<MusicPlayRes> PlayUrl(string url)
    {
        var stream = new MemoryStream();
        var res = await CoreHttpClient.GetAsync(url);
        if (res.StatusCode == HttpStatusCode.Redirect)
        {
            var url1 = res.Headers.Location;
            res = await CoreHttpClient.GetAsync(url1!);
            await res.Content.ReadAsStream().CopyToAsync(stream);
        }
        else
        {
            await res.Content.ReadAsStream().CopyToAsync(stream);
        }

        var type = TestMediaType(stream);
        stream.Seek(0, SeekOrigin.Begin);
        return await Play(stream, type);
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

        bool play = false;
        Volume = 0;

        try
        {
            if (file.StartsWith("http"))
            {
                res = await PlayUrl(file);
            }
            else if (File.Exists(file))
            {
                res = await Play(file);
            }

            if (res.Res)
            {
                play = true;
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