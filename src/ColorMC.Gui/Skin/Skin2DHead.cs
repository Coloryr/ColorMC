using System.IO;
using ColorMC.Gui.Utils;
using SkiaSharp;

namespace ColorMC.Gui.Skin;

/// <summary>
/// 皮肤处理
/// </summary>
public static class Skin2DHead
{
    /// <summary>
    /// 创建头像图片
    /// </summary>
    /// <param name="file">图片</param>
    /// <returns>图片数据</returns>
    public static Stream MakeHeadImage(SKBitmap image)
    {
        using var image1 = new SKBitmap(8, 8);
        using var image2 = new SKBitmap(128, 128);

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                image1.SetPixel(i, j, image.GetPixel(i + 8, j + 8));
            }
        }

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                image1.SetPixel(i, j, ImageUtils.Mix(image1.GetPixel(i, j), image.GetPixel(i + 40, j + 8)));
            }
        }

        for (int i = 0; i < 128; i++)
        {
            for (int j = 0; j < 128; j++)
            {
                image2.SetPixel(i, j, image1.GetPixel(i / 16, j / 16));
            }
        }

        return image2.Encode(SKEncodedImageFormat.Png, 100).AsStream();
    }
}
