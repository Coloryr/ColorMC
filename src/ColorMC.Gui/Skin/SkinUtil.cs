using Avalonia.Controls.Documents;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Skin;

public class SkinUtil
{
    public static ModelSourceTextureType GetTextType(Image<Rgba32> image)
    {
        if(image.Width >= 64 && image.Height >= 64 && image.Width == image.Height)
        {
            if (IsSlimSkin(image))
            {
                return ModelSourceTextureType.RATIO_1_1_SLIM;
            }
            else
            {
                return ModelSourceTextureType.RATIO_1_1;
            }
        }
        else if (image.Width == image.Height * 2)
        {
            return ModelSourceTextureType.RATIO_2_1;
        }
        else
        {
            return ModelSourceTextureType.UNKNOWN;
        }
    }

    public static bool IsSlimSkin(Image<Rgba32> image)
    {
        var scale = image.Width / 64;
        return (Check(image, 50 * scale, 16 * scale, 2 * scale, 4 * scale, Color.Transparent) ||
                Check(image, 54 * scale, 20 * scale, 2 * scale, 12 * scale, Color.Transparent) ||
                Check(image, 42 * scale, 48 * scale, 2 * scale, 4 * scale, Color.Transparent) ||
                Check(image, 46 * scale, 52 * scale, 2 * scale, 12 * scale, Color.Transparent)) ||
                (Check(image, 50 * scale, 16 * scale, 2 * scale, 4 * scale, Color.White) &&
                        Check(image, 54 * scale, 20 * scale, 2 * scale, 12 * scale, Color.White) &&
                        Check(image, 42 * scale, 48 * scale, 2 * scale, 4 * scale, Color.White) &&
                        Check(image, 46 * scale, 52 * scale, 2 * scale, 12 * scale, Color.White)) ||
                (Check(image, 50 * scale, 16 * scale, 2 * scale, 4 * scale, Color.Black) &&
                        Check(image, 54 * scale, 20 * scale, 2 * scale, 12 * scale, Color.Black) &&
                        Check(image, 42 * scale, 48 * scale, 2 * scale, 4 * scale, Color.Black) &&
                        Check(image, 46 * scale, 52 * scale, 2 * scale, 12 * scale, Color.Black));
    }

    public static bool Check(Image<Rgba32> image, int x, int y, int w, int h, Rgba32 color)
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
