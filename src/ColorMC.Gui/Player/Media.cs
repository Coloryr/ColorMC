using ColorMC.Core;
using ColorMC.Core.Net;
using ColorMC.Gui.Player.Decoder.Mp3;
using Newtonsoft.Json.Linq;
using OpenTK.Audio.OpenAL;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.Player;

public static class Media
{
    private static float volume = 0.1f;
    private static int alSource;
    private static ALDevice device;
    private static ALContext context;
    private static bool ok = false;
    private static CancellationTokenSource cancel = new();
    public static float Volume
    {
        set
        {
            volume = value;
            AL.Source(alSource, ALSourcef.Gain, volume);
        }
    }

    public static unsafe void Init()
    {
        // Get the default device, then go though all devices and select the AL soft device if it exists.
        string deviceName = ALC.GetString(ALDevice.Null, AlcGetString.DefaultDeviceSpecifier);

        device = ALC.OpenDevice(deviceName);
        int temp = 0;
        context = ALC.CreateContext(device, ref temp);
        ALC.MakeContextCurrent(context);

        AL.GenSource(out alSource);

        CheckALError();
        ok = true;
    }

    public static async void Close()
    {
        if (!ok)
            return;

        Stop();

        await Task.Run(() =>
        {
            while (play)
            {
                Thread.Sleep(10);
            }
        });

        AL.DeleteSource(alSource);

        ALC.MakeContextCurrent(ALContext.Null);
        ALC.DestroyContext(context);
        ALC.CloseDevice(device);
    }

    public static void CheckALError()
    {
        ALError error = AL.GetError();
        if (error != ALError.NoError)
        {
            App.ShowError($"ALError", new Exception(AL.GetErrorString(error)));
        }
    }

    public static void Pause()
    {
        if (!ok)
            return;

        AL.SourcePause(alSource);
    }

    public static void Play()
    {
        if (!ok)
            return;

        AL.SourcePlay(alSource);
    }

    public static void Stop()
    {
        if (!ok)
            return;

        cancel.Cancel();

        AL.Source(alSource, ALSourcef.Gain, 0);
        AL.SourceStop(alSource);

        AL.GetSource(alSource, ALGetSourcei.BuffersQueued, out int value);
        while (value > 0)
        {
            int temp = AL.SourceUnqueueBuffer(alSource);
            AL.DeleteBuffer(temp);
            value--;
        }
    }

    public static unsafe (bool, string?) PlayWAV(string filePath)
    {
        if (!ok)
            return (false, null);

        Stop();

        cancel = new();

        ReadOnlySpan<byte> file = File.ReadAllBytes(filePath);
        int index = 0;
        if (file[index++] != 'R' || file[index++] != 'I' || file[index++] != 'F' || file[index++] != 'F')
        {
            return (false, "Given file is not in RIFF format");
        }

        var chunkSize = BinaryPrimitives.ReadInt32LittleEndian(file.Slice(index, 4));
        index += 4;

        if (file[index++] != 'W' || file[index++] != 'A' || file[index++] != 'V' || file[index++] != 'E')
        {
            return (false, "Given file is not in WAVE format");
        }

        short numChannels = -1;
        int sampleRate = -1;
        int byteRate = -1;
        short blockAlign = -1;
        short bitsPerSample = -1;

        ALFormat format = 0;

        while (index + 4 < file.Length)
        {
            var identifier = "" + (char)file[index++] + (char)file[index++] + (char)file[index++] + (char)file[index++];
            var size = BinaryPrimitives.ReadInt32LittleEndian(file.Slice(index, 4));
            index += 4;
            if (identifier == "fmt ")
            {
                if (size != 16)
                {
                    return (false, $"Unknown Audio Format with subchunk1 size {size}");
                }
                else
                {
                    var audioFormat = BinaryPrimitives.ReadInt16LittleEndian(file.Slice(index, 2));
                    index += 2;
                    if (audioFormat != 1)
                    {
                        return (false, $"Unknown Audio Format with ID {audioFormat}");
                    }
                    else
                    {
                        numChannels = BinaryPrimitives.ReadInt16LittleEndian(file.Slice(index, 2));
                        index += 2;
                        sampleRate = BinaryPrimitives.ReadInt32LittleEndian(file.Slice(index, 4));
                        index += 4;
                        byteRate = BinaryPrimitives.ReadInt32LittleEndian(file.Slice(index, 4));
                        index += 4;
                        blockAlign = BinaryPrimitives.ReadInt16LittleEndian(file.Slice(index, 2));
                        index += 2;
                        bitsPerSample = BinaryPrimitives.ReadInt16LittleEndian(file.Slice(index, 2));
                        index += 2;

                        if (numChannels == 1)
                        {
                            if (bitsPerSample == 8)
                                format = ALFormat.Mono8;
                            else if (bitsPerSample == 16)
                                format = ALFormat.Mono16;
                            else
                            {
                                return (false, $"Can't Play mono {bitsPerSample} sound.");
                            }
                        }
                        else if (numChannels == 2)
                        {
                            if (bitsPerSample == 8)
                                format = ALFormat.Stereo8;
                            else if (bitsPerSample == 16)
                                format = ALFormat.Stereo16;
                            else
                            {
                                return (false, $"Can't Play stereo {bitsPerSample} sound.");
                            }
                        }
                        else
                        {
                            return (false, $"Can't play audio with {numChannels} sound");
                        }
                    }
                }
            }
            else if (identifier == "data")
            {
                var data = file.Slice(index, size);
                index += size;
                AL.GenBuffer(out int alBuffer);

                fixed (byte* pData = data)
                    AL.BufferData(alBuffer, format, pData, size, sampleRate);

                AL.SourceQueueBuffer(alSource, alBuffer);

                if (AL.GetSourceState(alSource) != ALSourceState.Playing)
                {
                    AL.SourcePlay(alSource);
                }
            }
            else
            {
                index += size;
            }
        }

        return (true, null);
    }

    private static bool play = false;

    public static async Task<(bool, string?)> PlayMp3(Stream stream)
    {
        if (!ok)
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

                    AL.GenBuffer(out int alBuffer);

                    AL.BufferData(alBuffer, ALFormat.Stereo16,
                        new ReadOnlySpan<byte>(frame.buff, 0, frame.len), decoder.outputFrequency);

                    AL.SourceQueueBuffer(alSource, alBuffer);

                    if (AL.GetSourceState(alSource) != ALSourceState.Playing)
                    {
                        AL.SourcePlay(alSource);
                    }
                    int value, value1;
                    
                    do
                    {
                        AL.GetSource(alSource, ALGetSourcei.BuffersQueued, out value);
                        AL.GetSource(alSource, ALGetSourcei.BuffersProcessed, out value1);
                        
                        if (value - value1 > 100)
                        {
                            int temp = AL.SourceUnqueueBuffer(alSource);
                            AL.DeleteBuffer(temp);
                            Thread.Sleep(10);
                        }
                    }
                    while (value - value1 > 100);

                    count++;
                }
                decoder.Dispose();
                if (!cancel.IsCancellationRequested)
                {
                    while (true)
                    {
                        AL.GetSource(alSource, ALGetSourcei.BuffersQueued, out int value);
                        AL.GetSource(alSource, ALGetSourcei.BuffersProcessed, out int value1);
                        if (value == value1)
                        {
                            AL.Source(alSource, ALSourcef.Gain, 0);
                            AL.SourceStop(alSource);

                            while (value > 0)
                            {
                                int temp = AL.SourceUnqueueBuffer(alSource);
                                AL.DeleteBuffer(temp);
                                value--;
                            }
                            break;
                        }
                        Thread.Sleep(10);
                    }
                }
                play = false;
            }
            catch (Exception e)
            {
                Logs.Error("mp3 decode error", e);
            }
        }, cancel.Token);

        return (true, null);
    }

    public static async Task<(bool, string?)> PlayMp3(string file)
    {
        if (!ok)
            return (false, null);

        using var reader = File.OpenRead(file);
        return await PlayMp3(reader);
    }

    public static async Task<(bool, string?)> PlayUrl(string url)
    {
        if (!ok)
            return (false, null);

        try
        {
            var res = await BaseClient.DownloadClient.GetAsync(url);
            if (res.StatusCode == System.Net.HttpStatusCode.Redirect)
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