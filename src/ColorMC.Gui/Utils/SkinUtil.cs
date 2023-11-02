using ColorMC.Gui.Objs;
using SkiaSharp;

namespace ColorMC.Gui.Utils;

/// <summary>
/// 皮肤处理
/// </summary>
public static class SkinUtil
{
    /// <summary>
    /// 获取皮肤类型
    /// </summary>
    /// <param name="image">图片</param>
    /// <returns>类型</returns>
    public static SkinType GetTextType(SKBitmap image)
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

    /// <summary>
    /// 是否为1.8新版皮肤
    /// </summary>
    /// <param name="image">图片</param>
    /// <returns></returns>
    private static bool IsSlimSkin(SKBitmap image)
    {
        var scale = image.Width / 64;
        return (image.Check(50 * scale, 16 * scale, 2 * scale, 4 * scale,
            SKColors.Transparent) ||
                image.Check(54 * scale, 20 * scale, 2 * scale, 12 * scale,
                SKColors.Transparent) ||
                image.Check(42 * scale, 48 * scale, 2 * scale, 4 * scale,
                SKColors.Transparent) ||
                image.Check(46 * scale, 52 * scale, 2 * scale, 12 * scale,
                SKColors.Transparent)) ||
                (image.Check(50 * scale, 16 * scale, 2 * scale, 4 * scale,
                SKColors.White) &&
                        image.Check(54 * scale, 20 * scale, 2 * scale, 12 * scale, SKColors.White) &&
                        image.Check(42 * scale, 48 * scale, 2 * scale, 4 * scale, SKColors.White) &&
                        image.Check(46 * scale, 52 * scale, 2 * scale, 12 * scale, SKColors.White)) ||
                (image.Check(50 * scale, 16 * scale, 2 * scale, 4 * scale, SKColors.Black) &&
                        image.Check(54 * scale, 20 * scale, 2 * scale, 12 * scale, SKColors.Black) &&
                        image.Check(42 * scale, 48 * scale, 2 * scale, 4 * scale, SKColors.Black) &&
                        image.Check(46 * scale, 52 * scale, 2 * scale, 12 * scale, SKColors.Black));
    }

    /// <summary>
    /// 检查颜色
    /// </summary>
    /// <param name="image">图片</param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="w"></param>
    /// <param name="h"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    private static bool Check(this SKBitmap image, int x, int y, int w, int h, SKColor color)
    {
        for (int wi = 0; wi < w; wi++)
        {
            for (int hi = 0; hi < h; hi++)
            {
                if (image.GetPixel(x + wi, y + hi) != color)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
