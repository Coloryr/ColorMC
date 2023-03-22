using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Avalonia.VisualTree;
using ColorMC.Core;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Utils.LaunchSetting;
using NAudio.Wave;
using Newtonsoft.Json;
using OpenTK.Audio.OpenAL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui;

public static class OtherUtils
{
    public static string GetName(this SkinType type)
    {
        return type switch
        {
            SkinType.Old => App.GetLanguage("SkinType.Old"),
            SkinType.New => App.GetLanguage("SkinType.New"),
            SkinType.NewSlim => App.GetLanguage("SkinType.New_Slim"),
            _ => App.GetLanguage("SkinType.Other")
        };
    }

    public static string GetName(this FTBType type)
    {
        return type switch
        {
            FTBType.All => App.GetLanguage("FTBType.All"),
            FTBType.Featured => App.GetLanguage("FTBType.Featured"),
            FTBType.Popular => App.GetLanguage("FTBType.Popular"),
            FTBType.Installs => App.GetLanguage("FTBType.Installs"),
            FTBType.Search => App.GetLanguage("FTBType.Search"),
            _ => App.GetLanguage("FTBType.Other")
        };
    }
}

public static class SkinUtil
{
    public static SkinType GetTextType(Image<Rgba32> image)
    {
        if (image.Width >= 64 && image.Height >= 64 && image.Width == image.Height)
        {
            if (IsSlimSkin(image))
            {
                return SkinType.NewSlim;
            }
            else
            {
                return SkinType.New;
            }
        }
        else if (image.Width == image.Height * 2)
        {
            return SkinType.Old;
        }
        else
        {
            return SkinType.Unkonw;
        }
    }

    private static bool IsSlimSkin(Image<Rgba32> image)
    {
        var scale = image.Width / 64;
        return (Check(image, 50 * scale, 16 * scale, 2 * scale, 4 * scale,
            SixLabors.ImageSharp.Color.Transparent) ||
                Check(image, 54 * scale, 20 * scale, 2 * scale, 12 * scale,
                SixLabors.ImageSharp.Color.Transparent) ||
                Check(image, 42 * scale, 48 * scale, 2 * scale, 4 * scale,
                SixLabors.ImageSharp.Color.Transparent) ||
                Check(image, 46 * scale, 52 * scale, 2 * scale, 12 * scale,
                SixLabors.ImageSharp.Color.Transparent)) ||
                (Check(image, 50 * scale, 16 * scale, 2 * scale, 4 * scale,
                SixLabors.ImageSharp.Color.White) &&
                        Check(image, 54 * scale, 20 * scale, 2 * scale, 12 * scale,
                        SixLabors.ImageSharp.Color.White) &&
                        Check(image, 42 * scale, 48 * scale, 2 * scale, 4 * scale,
                        SixLabors.ImageSharp.Color.White) &&
                        Check(image, 46 * scale, 52 * scale, 2 * scale, 12 * scale,
                        SixLabors.ImageSharp.Color.White)) ||
                (Check(image, 50 * scale, 16 * scale, 2 * scale, 4 * scale,
                SixLabors.ImageSharp.Color.Black) &&
                        Check(image, 54 * scale, 20 * scale, 2 * scale, 12 * scale,
                        SixLabors.ImageSharp.Color.Black) &&
                        Check(image, 42 * scale, 48 * scale, 2 * scale, 4 * scale,
                        SixLabors.ImageSharp.Color.Black) &&
                        Check(image, 46 * scale, 52 * scale, 2 * scale, 12 * scale,
                        SixLabors.ImageSharp.Color.Black));
    }

    private static bool Check(Image<Rgba32> image, int x, int y, int w, int h, Rgba32 color)
    {
        for (int wi = 0; wi < w; wi++)
        {
            for (int hi = 0; hi < h; hi++)
            {
                if (image[x + wi, y + hi] != color)
                {
                    return false;
                }
            }
        }

        return true;
    }
}

public static partial class UIUtils
{
    public static T? FindToEnd<T>(this IVisual visual)
    {
        foreach (var item in visual.GetVisualChildren())
        {
            if (item is T t)
            {
                return t;
            }
        }

        foreach (var item in visual.GetVisualChildren())
        {
            var res = FindToEnd<T>(item);
            if (res != null)
            {
                return res;
            }
        }

        return default;
    }

    public static void MakeTran(this Expander expander)
    {
        try
        {
            var item1 = expander.FindToEnd<Border>();
            item1?.Bind(Border.BackgroundProperty, new Binding
            {
                Source = ColorSel.Instance,
                Path = "[TranBack]"
            });
        }
        catch
        {

        }
    }

    public static void MakeTran(this DataGrid grid)
    {
        try
        {
            var item1 = grid.FindToEnd<DataGridColumnHeadersPresenter>();
            if (item1 != null)
            {
                item1.Background = Brush.Parse("#ccffffff");
                foreach (var item in item1.GetVisualChildren())
                {
                    var item2 = item.FindToEnd<TextBlock>();
                    if (item2 != null)
                    {
                        item2.Foreground = Brushes.Black;
                    }
                }
            }
        }
        catch
        {

        }
    }

    public static void MakePadingNull(this Expander expander)
    {
        try
        {
            var item = expander.FindToEnd<Border>();
            if (item != null)
            {
                item.Background = Brushes.Transparent;
                item.Padding = new Thickness(0);
                item.BorderBrush = Brushes.Transparent;
            }
        }
        catch
        {

        }
    }

    public static void MakeThumb(this Slider slider)
    {
        try
        {
            var item = slider.FindToEnd<Thumb>();
            if (item != null)
            {
                var item1 = item.FindToEnd<Border>();
                if (item1 == null)
                    return;

                item1.Bind(Border.BackgroundProperty, new Binding
                {
                    Source = ColorSel.Instance,
                    Path = "[Main]"
                });

                item1.Bind(Border.BorderBrushProperty, new Binding
                {
                    Source = ColorSel.Instance,
                    Path = "[TranBack]"
                });

                item1.BorderThickness = new Thickness(2);
            }
        }
        catch
        {

        }
    }

    public static string MakeString(this List<string>? strings)
    {
        if (strings == null)
            return "";
        string temp = "";
        foreach (var item in strings)
        {
            temp += item + ",";
        }

        if (temp.Length > 0)
        {
            return temp[..^1];
        }

        return temp;
    }

    public static string MakeFileSize(long size)
    {
        if (size > 1000000)
        {
            return $"{(double)size / 1000000:#.000}Mb/s";
        }
        else if (size > 1000)
        {
            return $"{(double)size / 1000:#.000}Kb/s";
        }
        else
        {
            return $"{size}b/s";
        }
    }

    public static string MakeFileSize1(long size)
    {
        if (size > 1000000)
        {
            return $"{(double)size / 1000000:#.000}MB";
        }
        else if (size > 1000)
        {
            return $"{(double)size / 1000:#.000}KB";
        }
        else
        {
            return $"{size}";
        }
    }

    public static Avalonia.Media.Color ToColor(this IBrush brush)
    {
        if (brush is ImmutableSolidColorBrush brush1)
        {
            return brush1.Color;
        }

        return new(255, 255, 255, 255);
    }

    public static (double X, double Y) GetXY(this IVisual? visual)
    {
        if (visual == null)
            return (0, 0);
        var temp = (visual.Bounds.X, visual.Bounds.Y);
        if (visual.GetVisualParent() != null)
        {
            var (X, Y) = GetXY(visual.GetVisualParent());
            temp.X += X;
            temp.Y += Y;
        }

        return temp;
    }

    public static T? FindTop<T>(this IVisual visual) where T : IVisual
    {
        var pan = visual.GetVisualParent();
        while (pan != null)
        {
            if (pan is T t)
            {
                return t;
            }
            pan = pan.GetVisualParent();
        }

        return default;
    }
}

public static class ImageUtils
{
    public static async Task<MemoryStream> MakeHeadImage(string file)
    {
        using var image = await SixLabors.ImageSharp.Image.LoadAsync<Rgba32>(file);
        using var image1 = new Image<Rgba32>(8, 8);
        using var image2 = new Image<Rgba32>(64, 64);

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                image1[i, j] = image[i + 8, j + 8];
            }
        }

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                image1[i, j] = Mix(image1[i, j], image[i + 40, j + 8]);
            }
        }

        for (int i = 0; i < 64; i++)
        {
            for (int j = 0; j < 64; j++)
            {
                image2[i, j] = image1[i / 8, j / 8];
            }
        }

        MemoryStream stream = new();
        await image2.SaveAsBmpAsync(stream);
        return stream;
    }

    private static Rgba32 Mix(Rgba32 rgba, Rgba32 mix)
    {
        double ap = (double)(mix.A / 255);
        double dp = 1 - ap;

        rgba.R = (byte)(mix.R * ap + rgba.R * dp);
        rgba.G = (byte)(mix.G * ap + rgba.G * dp);
        rgba.B = (byte)(mix.B * ap + rgba.B * dp);

        return rgba;
    }


    public static Task<Bitmap?> MakeBackImage(string file, int value, int lim)
    {
        return Task.Run(() =>
        {
            try
            {
                if (value > 0 || lim != 100)
                {
                    using var image = SixLabors.ImageSharp.Image.Load(file);
                    
                    if (lim != 100)
                    {
                        int x = (int)(image.Width * (float)lim / 100);
                        int y = (int)(image.Height * (float)lim / 100);
                        image.Mutate(p =>
                        {
                            p.Resize(x, y);
                        });
                    }

                    if (value > 0)
                    {
                        image.Mutate(p =>
                        {
                            p.GaussianBlur(value);
                        });
                    }

                    using var stream = new MemoryStream();
                    image.SaveAsPng(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return new Bitmap(stream);
                }
                else
                {
                    return new Bitmap(file);
                }

            }
            catch (Exception e)
            {
                Logs.Error(App.GetLanguage("Error1"), e);
                return null;
            }
        });
    }
}

public static class ImageTemp
{
    public static string Local { get; private set; }

    public static void Init(string dir)
    {
        Local = dir + "image/";

        Directory.CreateDirectory(Local);
    }

    public static async Task<Bitmap?> Load(string url, CancellationToken token)
    {
        if (!Directory.Exists(Local))
        {
            Directory.CreateDirectory(Local);
        }
        var sha1 = Funtcions.GenSha256(url);
        if (File.Exists(Local + sha1))
        {
            if (token.IsCancellationRequested)
                return null;

            return new Bitmap(Local + sha1);
        }
        else
        {
            Bitmap? bitmap = null;
            Semaphore semaphore = new(0, 2);
            BaseClient.Poll(url, (res, stream) =>
            {
                if (res)
                {
                    using var temp = File.Create(Local + sha1);
                    stream!.CopyTo(temp);
                }
                semaphore.Release();
            }, token);
            await Task.Run(semaphore.WaitOne);

            if (token.IsCancellationRequested)
                return null;

            if (File.Exists(Local + sha1))
            {
                if (token.IsCancellationRequested)
                    return null;

                return new Bitmap(Local + sha1);
            }

            return null;
        }
    }
}

public static class GuiConfigUtils
{
    public static GuiConfigObj Config { get; set; }

    private static string Name;

    public static void Init(string dir)
    {
        Name = dir + "gui.json";

        Load(Name);
    }

    public static bool Load(string name, bool quit = false)
    {
        if (File.Exists(name))
        {
            try
            {
                Config = JsonConvert.DeserializeObject<GuiConfigObj>(File.ReadAllText(name))!;
            }
            catch (Exception e)
            {
                Logs.Error(App.GetLanguage("Gui.Error17"), e);
            }

            if (Config == null)
            {
                if (quit)
                {
                    return false;
                }

                Config = MakeDefaultConfig();

                SaveNow();
            }

            if (Config.ServerCustom == null)
            {
                if (quit)
                {
                    return false;
                }

                Config.ServerCustom = MakeServerCustomConfig();

                Save();
            }

            if (Config.Render == null
                || Config.Render.Windows == null
                || Config.Render.X11 == null)
            {
                if (quit)
                {
                    return false;
                }

                Config.Render = MakeRenderConfig();

                Save();
            }
        }
        else
        {
            Config = MakeDefaultConfig();

            SaveNow();
        }

        return true;
    }

    public static void SaveNow()
    {
        File.WriteAllText(Name,
                    JsonConvert.SerializeObject(Config, Formatting.Indented));
    }

    public static void Save()
    {
        ConfigSave.AddItem(new()
        {
            Name = "gui.json",
            Local = Name,
            Obj = Config
        });
    }

    public static Render MakeRenderConfig()
    {
        return new()
        {
            Windows = new()
            {
                UseWindowsUIComposition = null,
                UseWgl = null,
                UseCompositor = null,
                UseDeferredRendering = null
            },
            X11 = new()
            {
                UseEGL = null,
                UseGpu = null,
                OverlayPopups = null,
                UseDeferredRendering = null,
                UseCompositor = null,
            }
        };
    }

    public static GuiConfigObj MakeDefaultConfig()
    {
        return new()
        {
            Version = ColorMCCore.Version,
            ColorMain = "#FF5ABED6",
            ColorBack = "#FFF4F4F5",
            ColorTranBack = "#88FFFFFF",
            RGBS = 100,
            RGBV = 100,
            ColorFont1 = "#FFFFFFFF",
            ColorFont2 = "#FF000000",
            ServerCustom = MakeServerCustomConfig(),
            FontDefault = true,
            Render = MakeRenderConfig(),
            BackLimitValue = 50
        };
    }

    public static ServerCustom MakeServerCustomConfig()
    {
        return new()
        {
            MotdColor = "#FFFFFFFF",
            MotdBackColor = "#FF000000",
            Volume = 30
        };
    }
}

public static class Media
{
    private static float volume = 0.1f;
    private static int alSource;
    private static ALDevice device;
    private static ALContext context;
    private static bool ok  =false;
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
        context = ALC.CreateContext(device, (int[])null);
        ALC.MakeContextCurrent(context);

        AL.GenSource(out alSource);

        CheckALError();
        ok = true;
    }

    public static void Close()
    {
        if (!ok)
            return;

        Stop();

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

    public static async Task<(bool, string?)> PlayMp3(Stream stream)
    {
        if (!ok)
            return (false, null);

        using var reader = new Mp3FileReader(stream);
        return await PlayMp3(reader);
    }

    public static async Task<(bool, string?)> PlayMp3(string file)
    {
        if (!ok)
            return (false, null);

        using var reader = new Mp3FileReader(file);
        return await PlayMp3(reader);
    }

    private static async Task<(bool, string?)> PlayMp3(Mp3FileReader reader)
    {
        using var decoder = new AcmMp3FrameDecompressor(reader.Mp3WaveFormat);
        var format = reader.WaveFormat;

        byte[] buffer = new byte[16384 * 4];

        ALFormat format1 = 0;

        if (format.Channels == 1)
        {
            if (format.BitsPerSample == 8)
                format1 = ALFormat.Mono8;
            else if (format.BitsPerSample == 16)
                format1 = ALFormat.Mono16;
            else
            {
                return (false, $"Can't Play mono {format.BitsPerSample} sound.");
            }
        }
        else if (format.Channels == 2)
        {
            if (format.BitsPerSample == 8)
                format1 = ALFormat.Stereo8;
            else if (format.BitsPerSample == 16)
                format1 = ALFormat.Stereo16;
            else
            {
                return (false, $"Can't Play stereo {format.BitsPerSample} sound.");
            }
        }
        else
        {
            return (false, $"Can't play audio with {format.Channels} sound");
        }

        await Task.Run(() =>
        {
            Mp3Frame frame;
            while (true)
            {
                frame = reader.ReadNextFrame();
                if (frame == null)
                {
                    break;
                }
                int decompressed = decoder.DecompressFrame(frame, buffer, 0);
                if (decompressed <= 0)
                {
                    continue;
                }

                AL.GenBuffer(out int alBuffer);

                AL.BufferData(alBuffer, ALFormat.Stereo16, 
                    new ReadOnlySpan<byte>(buffer, 0, decompressed), format.SampleRate);

                AL.SourceQueueBuffer(alSource, alBuffer);

                if (AL.GetSourceState(alSource) != ALSourceState.Playing)
                {
                    AL.SourcePlay(alSource);
                }
            }
        });

        return (true, null);
    }

    public static async Task<(bool, string?)> PlayUrl(string url)
    {
        if (!ok)
            return (false, null);

        var res = await BaseClient.DownloadClient.GetAsync(url);
        if (res.StatusCode == System.Net.HttpStatusCode.Redirect)
        {
            var url1 = res.Headers.Location;
            res = await BaseClient.DownloadClient.GetAsync(url1);
            return await PlayMp3(res.Content.ReadAsStream());
        }

        return await PlayMp3(res.Content.ReadAsStream());
    }
}