using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Player.Decoder.Mp3;

namespace ColorMC.Gui.Player;

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
    /// 音量
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
    /// 初始化播放器
    /// </summary>
    public static unsafe void Init()
    {
        if (SystemInfo.Os == OsType.Windows)
        {
            s_player = new NAudioPlayer();
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
    public static async Task<(bool, string?)> PlayWAV(string filePath)
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

        return (true, null);
    }

    /// <summary>
    /// 播放Mp3
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static async Task<(bool, string?)> PlayMp3(Stream stream)
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
                while (true)
                {
                    if (s_cancel.IsCancellationRequested)
                        break;
                    var frame = decoder.DecodeFrame();
                    if (frame == null || frame.len <= 0 || s_cancel.IsCancellationRequested)
                    {
                        break;
                    }

                    s_player?.Write(2, 16, frame.buff, frame.len, decoder.outputFrequency);
                    count++;
                }
                decoder.Dispose();
                Decoding = false;
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("Gui.Error29"), e);
            }
        }, s_cancel.Token);

        return (true, null);
    }

    /// <summary>
    /// 播放
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static async Task<(bool, string?)> PlayMp3(string file)
    {
        if (s_player == null)
            return (false, null);

        s_musicFile = file;

        var reader = File.OpenRead(file);
        return await PlayMp3(reader);
    }

    /// <summary>
    /// 播放
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static async Task<(bool, string?)> PlayUrl(string url)
    {
        if (s_player == null)
            return (false, null);

        try
        {
            var res = await BaseClient.DownloadClient.GetAsync(url);
            if (res.StatusCode == HttpStatusCode.Redirect)
            {
                var url1 = res.Headers.Location;
                res = await BaseClient.DownloadClient.GetAsync(url1);
                return await PlayMp3(res.Content.ReadAsStream());
            }

            return await PlayMp3(res.Content.ReadAsStream());
        }
        catch (Exception e)
        {
            Logs.Error(App.Lang("Gui.Error30"), e);
            return (false, null);
        }
    }

    /// <summary>
    /// 播放结束
    /// </summary>
    public static void PlayEnd()
    {
        if (!string.IsNullOrWhiteSpace(s_musicFile))
        {
            _ = PlayMp3(s_musicFile);
        }
    }
}