using ColorMC.Gui.Objs;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Utils;

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
