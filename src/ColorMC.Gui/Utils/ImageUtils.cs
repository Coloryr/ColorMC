using Avalonia.Media.Imaging;
using ColorMC.Core.Helpers;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace ColorMC.Gui.Utils;

/// <summary>
/// 图片处理
/// </summary>
public static class ImageUtils
{
    public static string Local { get; private set; }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir"></param>
    public static void Init(string dir)
    {
        Local = dir + "image/";

        Directory.CreateDirectory(Local);
    }

    /// <summary>
    /// 加载图片
    /// </summary>
    /// <param name="url">网址</param>
    /// <returns>位图</returns>
    public static async Task<Bitmap?> Load(string url)
    {
        if (!Directory.Exists(Local))
        {
            Directory.CreateDirectory(Local);
        }
        var sha1 = HashHelper.GenSha256(url);
        if (File.Exists(Local + sha1))
        {
            return new Bitmap(Local + sha1);
        }
        else
        {
            try
            {
                var data1 = await BaseClient.GetStream(url);
                if (data1.Item1)
                {
                    //var image = Bitmap.DecodeToWidth(data1.Item2!, 80);
                    //image.Save(Local + sha1);
                    //return image;

                    var image1 = Image.Load(data1.Item2!);
                    image1 = Resize(image1, 100, 100);
                    image1.SaveAsPng(Local + sha1);
                    image1.Dispose();
                    return new Bitmap(Local + sha1);
                }

                return null;
            }
            catch (Exception e)
            {
                Logs.Error(App.GetLanguage("Gui.Error24"), e);
            }

            return App.GameIcon;
        }
    }

    /// <summary>
    /// 创建头像图片
    /// </summary>
    /// <param name="file">图片</param>
    /// <returns>图片数据</returns>
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

    /// <summary>
    /// 混合像素
    /// </summary>
    /// <param name="rgba">源</param>
    /// <param name="mix">目标</param>
    /// <returns>结果</returns>
    private static Rgba32 Mix(Rgba32 rgba, Rgba32 mix)
    {
        double ap = mix.A / 255;
        double dp = 1 - ap;

        rgba.R = (byte)(mix.R * ap + rgba.R * dp);
        rgba.G = (byte)(mix.G * ap + rgba.G * dp);
        rgba.B = (byte)(mix.B * ap + rgba.B * dp);

        return rgba;
    }

    /// <summary>
    /// 获得背景图
    /// </summary>
    /// <param name="file">文件</param>
    /// <param name="value">模糊度</param>
    /// <param name="lim">分辨率限制</param>
    /// <returns></returns>
    public static Task<Bitmap?> MakeBackImage(string file, int value, int lim)
    {
        return Task.Run(async () =>
        {
            try
            {
                Stream? stream1 = null;
                if (file.StartsWith("https://") || file.StartsWith("http://"))
                {
                    var res = await BaseClient.DownloadClient.GetAsync(file);
                    stream1 = res.Content.ReadAsStream();
                }
                else if (file.StartsWith("ColorMC.Gui"))
                {
                    var assm = Assembly.GetExecutingAssembly();
                    stream1 = assm.GetManifestResourceStream(file)!;
                }
                else if (SystemInfo.Os == OsType.Android)
                {
                    file = ColorMCGui.RunDir + "BG";
                    stream1 = PathHelper.OpenRead(file);
                }
                else
                {
                    stream1 = PathHelper.OpenRead(file);
                }

                if (stream1 == null)
                {
                    return null;
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

    public static Image Resize(Image image, int width, int height)
    {
        Point pos;
        int newWidth;
        int newHeight;

        // 横屏图片
        if (image.Width > image.Height)
        {
            newWidth = width;
            newHeight = (int)(((float)image.Height / image.Width) * newWidth);

            pos = new Point(0, (height - newHeight) / 2);
        }
        // 竖屏图片
        else
        {
            newHeight = height;
            newWidth = (int)(newHeight * ((float)image.Width / image.Height));

            pos = new Point((width - newWidth) / 2, 0);
        }

        var image1 = new Image<Rgba32>(width, height);
        image.Mutate(a => a.Resize(newWidth, newHeight));
        image1.Mutate(a =>
        {
            a.Fill(Color.Transparent);
            a.DrawImage(image, pos, 1);
        });

        image.Dispose();
        return image1;
    }
}
