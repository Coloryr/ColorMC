using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Player.Decoder.Mp3;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.Player;

public static class Media
{
    private static IPlayer? player;

    private static CancellationTokenSource cancel = new();

    private static bool play = false;

    public static float Volume
    {
        set
        {
            if (player != null)
            {
                player.Volume = value;
            }
        }
    }

    /// <summary>
    /// 初始化播放器
    /// </summary>
    public static unsafe void Init()
    {
        if (ColorMCGui.RunType != RunType.Program)
        {
            return;
        }

        if (SystemInfo.Os != OsType.Windows)
        {
            player = new OpenalPlayer();
        }
        else
        {
            player = new NAudioPlayer();
        }
    }

    /// <summary>
    /// 关闭播放器
    /// </summary>
    public static async void Close()
    {
        Stop();

        await Task.Run(() =>
        {
            while (play)
            {
                Thread.Sleep(10);
            }
        });

        player?.Close();
    }

    /// <summary>
    /// 暂停
    /// </summary>
    public static void Pause() => player?.Pause();

    /// <summary>
    /// 播放
    /// </summary>
    public static void Play() => player?.Play();

    /// <summary>
    /// 停止
    /// </summary>
    public static void Stop()
    {
        cancel.Cancel();

        player?.Stop();
    }

    /// <summary>
    /// 播放
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static async Task<(bool, string?)> PlayWAV(string filePath)
    {
        if (player == null)
            return (false, null);

        Stop();

        await Task.Run(() =>
        {
            while (play)
            {
                Thread.Sleep(10);
            }
        });

        cancel = new();

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
                        player?.Write(numChannels, bitsPerSample, temp, length, sampleRate);
                        if (cancel.IsCancellationRequested)
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

        return (true, null);
    }

    /// <summary>
    /// 播放
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static async Task<(bool, string?)> PlayMp3(Stream stream)
    {
        if (player == null)
            return (false, null);

        Stop();

        await Task.Run(() =>
        {
            while (play)
            {
                Thread.Sleep(10);
            }
        });

        cancel = new();

        var decoder = new Mp3Decoder(stream);
        if (!decoder.Load())
        {
            return (false, "mp3 file error");
        }

        _ = Task.Run(() =>
        {
            try
            {
                play = true;
                int count = 0;
                while (true)
                {
                    if (cancel.IsCancellationRequested)
                        break;
                    var frame = decoder.DecodeFrame();
                    if (frame == null || frame.len <= 0 || cancel.IsCancellationRequested)
                    {
                        break;
                    }

                    player?.Write(2, 16, frame.buff, frame.len, decoder.outputFrequency);
                    count++;
                }
                decoder.Dispose();
                play = false;
            }
            catch (Exception e)
            {
                Logs.Error("mp3 decode error", e);
            }
        }, cancel.Token);

        return (true, null);
    }

    /// <summary>
    /// 播放
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public static async Task<(bool, string?)> PlayMp3(string file)
    {
        if (player == null)
            return (false, null);

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
        if (player == null)
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
            Logs.Error("play error", e);
            return (false, null);
        }
    }
}