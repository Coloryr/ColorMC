using System.IO;
using ColorMC.Core.Helpers;
using SkiaSharp;

namespace ColorMC.Gui.Utils;

internal class IcoConverterUtils
{
    /// <summary>
    /// 将 SKBitmap 列表转换为 ICO 格式并保存到文件
    /// </summary>
    /// <param name="bitmaps">包含不同尺寸图像的 SKBitmap 列表</param>
    /// <param name="outputPath">输出 ICO 文件的路径</param>
    public static void ConvertToIco(SKBitmap bitmap, string outputPath)
    {
        using var outputStream = PathHelper.OpenWrite(outputPath, true);
        using var writer = new BinaryWriter(outputStream);
        // 1. 写入 ICO 文件头 (6 bytes)
        writer.Write((ushort)0); // 保留字段 (必须为0)
        writer.Write((ushort)1); // 图像类型 (1 表示图标)
        writer.Write((ushort)1); // 图像数量 (只有一个图像)

        // 2. 准备图像数据
        byte[] pngData;
        using (var pngStream = new MemoryStream())
        {
            // 将 SKBitmap 编码为 PNG 并存入内存流
            bitmap.Encode(pngStream, SKEncodedImageFormat.Png, 100);
            pngData = pngStream.ToArray();
        }

        // 3. 写入图像条目信息 (ICONDIRENTRY) - 16 bytes
        byte width = (byte)(bitmap.Width == 256 ? 0 : bitmap.Width); // 宽度 (0 表示 256)
        byte height = (byte)(bitmap.Height == 256 ? 0 : bitmap.Height); // 高度 (0 表示 256)
        writer.Write(width);
        writer.Write(height);
        writer.Write((byte)0); // 颜色板数量 (通常无调色板，设为0)
        writer.Write((byte)0); // 保留字段 (必须为0)
        writer.Write((ushort)0); // 颜色平面 (通常为0或1)
        writer.Write((ushort)32); // 每像素位数 (32-bit ARGB)
        writer.Write((uint)pngData.Length); // PNG 数据大小
        writer.Write((uint)22); // PNG 数据在文件中的偏移量 (6字节头 + 16字节条目 = 22字节)

        // 4. 写入 PNG 数据
        writer.Write(pngData);
    }

    /// <summary>
    /// 从文件加载 SKBitmap
    /// </summary>
    public static SKBitmap LoadImage(string inputPath)
    {
        using var inputStream = PathHelper.OpenRead(inputPath);
        return SKBitmap.Decode(inputStream);
    }

    /// <summary>
    /// 调整 SKBitmap 的尺寸
    /// </summary>
    public static SKBitmap ResizeImage(SKBitmap original, int newWidth, int newHeight)
    {
        var resized = new SKBitmap(newWidth, newHeight);
        using var canvas = new SKCanvas(resized);
        canvas.DrawBitmap(original, new SKRect(0, 0, newWidth, newHeight));
        return resized;
    }
}
