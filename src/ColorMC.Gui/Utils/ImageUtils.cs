using System.Runtime.InteropServices;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ColorMC.Gui.UI.Controls;
using SkiaSharp;

namespace ColorMC.Gui.Utils;

/// <summary>
/// 图片处理
/// </summary>
public static class ImageUtils
{
    /// <summary>
    /// 转换
    /// </summary>
    /// <param name="colorType"></param>
    /// <returns></returns>
    public static PixelFormat ToPixelFormat(this SKColorType colorType)
    {
        return colorType switch
        {
            SKColorType.Bgra8888 => PixelFormats.Bgra8888,
            SKColorType.Rgb565 => PixelFormats.Rgb565,
            SKColorType.Gray8 => PixelFormats.Gray8,
            _ => PixelFormats.Rgba8888
        };
    }

    /// <summary>
    /// 转换
    /// </summary>
    /// <param name="alphaType"></param>
    /// <returns></returns>
    public static AlphaFormat ToAlphaFormat(this SKAlphaType alphaType)
    {
        return alphaType switch
        {
            SKAlphaType.Opaque => AlphaFormat.Opaque,
            SKAlphaType.Premul => AlphaFormat.Premul,
            _ => AlphaFormat.Unpremul
        };
    }

    /// <summary>
    /// 转换
    /// </summary>
    /// <param name="bitmap"></param>
    /// <returns></returns>
    public static Bitmap ToBitmap(this SKBitmap bitmap)
    {
        return new Bitmap(bitmap.ColorType.ToPixelFormat(), 
        bitmap.AlphaType.ToAlphaFormat(), bitmap.GetPixels(), 
        new(bitmap.Width, bitmap.Height), new(96, 96), bitmap.RowBytes);
    }

    /// <summary>
    /// 转换
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    public static Bitmap ToBitmap(this SKImage image)
    {
        var temp = Marshal.AllocHGlobal(image.Height * image.Info.RowBytes);
        image.ReadPixels(new(image.Width, image.Height, image.ColorType, image.AlphaType), temp);
        var bitmap = new Bitmap(image.ColorType.ToPixelFormat(),image.AlphaType.ToAlphaFormat(), temp,
        new(image.Width, image.Height), new(96, 96), image.Info.RowBytes);
        Marshal.FreeHGlobal(temp);
        return bitmap;
    }

    /// <summary>
    /// 图片等比缩放
    /// </summary>
    /// <param name="image">图片</param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static SKBitmap Resize(SKBitmap image, int width, int height)
    {
        int newWidth;
        int newHeight;

        // 横屏图片
        if (image.Width > image.Height)
        {
            newWidth = width;
            newHeight = (int)((float)image.Height / image.Width * newWidth);
        }
        // 竖屏图片
        else
        {
            newHeight = height;
            newWidth = (int)(newHeight * ((float)image.Width / image.Height));
        }
        return image.Resize(new SKSizeI(newWidth, newHeight), SKFilterQuality.High);
    }
}
