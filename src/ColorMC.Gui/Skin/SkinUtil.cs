using ColorMC.Gui.Utils.LaunchSetting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ColorMC.Gui.Skin;

public static class SkinUtil
{
    public static SkinType GetTextType(Image<Rgba32> image)
    {
        if (image.Width >= 64 && image.Height >= 64 && image.Width == image.Height)
        {
            if (IsSlimSkin(image))
            {
                return SkinType.New_Slim;
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
            return SkinType.UNKNOWN;
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

    public static string GetName(this SkinType type)
    {
        return type switch
        {
            SkinType.Old => Localizer.Instance["SkinType.Old"],
            SkinType.New => Localizer.Instance["SkinType.New"],
            SkinType.New_Slim => Localizer.Instance["SkinType.New_Slim"],
            _ => Localizer.Instance["SkinType.Other"]
        };
    }
}
