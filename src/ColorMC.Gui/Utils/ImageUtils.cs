using Avalonia.Media.Imaging;
using ColorMC.Core.Net;
using ColorMC.Core.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ColorMC.Gui.Utils;

public static class ImageUtils
{
    public static string Local { get; private set; }

    public static void Init(string dir)
    {
        Local = dir + "image/";

        Directory.CreateDirectory(Local);
    }

    public static async Task<Bitmap?> Load(string url)
    {
        if (!Directory.Exists(Local))
        {
            Directory.CreateDirectory(Local);
        }
        var sha1 = FuntionUtils.GenSha256(url);
        if (File.Exists(Local + sha1))
        {
            return new Bitmap(Local + sha1);
        }
        else
        {
            try
            {
                var data1 = await BaseClient.GetBytes(url);
                using var stream = new MemoryStream(data1);
                var image = Bitmap.DecodeToWidth(stream, 80);
                image.Save(Local + sha1);

                return image;
            }
            catch (Exception e)
            {
                Logs.Error(App.GetLanguage("Gui.Error24"), e);
            }

            return App.GameIcon;
        }
    }

    public static async Task<MemoryStream> MakeHeadImage(string file)
    {
        using var image = await Image.LoadAsync<Rgba32>(file);
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
        double ap = mix.A / 255;
        double dp = 1 - ap;

        rgba.R = (byte)(mix.R * ap + rgba.R * dp);
        rgba.G = (byte)(mix.G * ap + rgba.G * dp);
        rgba.B = (byte)(mix.B * ap + rgba.B * dp);

        return rgba;
    }


    public static Task<Bitmap?> MakeBackImage(string file, int value, int lim)
    {
        return Task.Run(async () =>
        {
            try
            {
                Stream stream1;
                if (file.StartsWith("https://") || file.StartsWith("http://"))
                {
                    var res = await BaseClient.DownloadClient.GetAsync(file);
                    stream1 = res.Content.ReadAsStream();
                }
                else if (!File.Exists(file))
                {
                    return null;
                }
                else
                {
                    stream1 = File.OpenRead(file);
                }
                if (value > 0 || lim != 100)
                {
                    using var image = Image.Load(stream1);

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
                    return new Bitmap(stream1);
                }
            }
            catch (Exception e)
            {
                Logs.Error(App.GetLanguage("Gui.Error1"), e);
                return null;
            }
        });
    }
}
