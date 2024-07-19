using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Skin;
using ColorMC.Gui.Utils;

using SkiaSharp;

namespace ColorMC.Gui.Manager;

public static class ImageManager
{
    public static Bitmap GameIcon { get; private set; }
    public static Bitmap LoadIcon { get; private set; }
    public static WindowIcon Icon { get; private set; }

    public static Bitmap? BackBitmap { get; private set; }
    public static SKBitmap? SkinBitmap { get; private set; }
    public static SKBitmap? CapeBitmap { get; private set; }
    public static Bitmap? HeadBitmap { get; private set; }

    private static readonly Dictionary<string, Bitmap> s_gameIcon = [];

    public static void Load()
    {
        {
            using var asset = AssetLoader.Open(new Uri("resm:ColorMC.Gui.Resource.Pic.game.png"));
            GameIcon = new Bitmap(asset);
        }
        {
            using var asset1 = AssetLoader.Open(new Uri(SystemInfo.Os == OsType.MacOS
                ? "resm:ColorMC.Gui.macicon.ico" : "resm:ColorMC.Gui.icon.ico"));
            Icon = new(asset1!);
        }
        {
            using var asset1 = AssetLoader.Open(new Uri("resm:ColorMC.Gui.Resource.Pic.load.png"));
            LoadIcon = new(asset1!);
        }
    }

    public static void LoadSkinHead(string? file, string? file1)
    {
        if (file == null || !File.Exists(file))
        {
            SetDefaultHead();
        }
        else
        {
            try
            {
                var old = SkinBitmap;
                var old1 = HeadBitmap;
                var config = GuiConfigUtils.Config.Head;
                SkinBitmap = SKBitmap.Decode(file);
                using var data = config.Type switch
                {
                    HeadType.Head2D => Skin2DHead.MakeHeadImage(SkinBitmap),
                    HeadType.Head3D_A => Skin3DHeadA.MakeHeadImage(SkinBitmap),
                    HeadType.Head3D_B => Skin3DHeadB.MakeHeadImage(SkinBitmap),
                    _ => throw new IndexOutOfRangeException()
                };
                HeadBitmap = new Bitmap(data);
                old?.Dispose();
                old1?.Dispose();
            }
            catch (Exception e)
            {
                Logs.Error(string.Format(App.Lang("ImageManager.Error1"), file), e);
            }
        }
        if (file1 != null || !File.Exists(file1))
        {
            try
            {
                CapeBitmap = SKBitmap.Decode(file1);
            }
            catch (Exception e)
            {
                Logs.Error(string.Format(App.Lang("ImageManager.Error2"), file), e);
            }
        }
    }

    public static void ReloadSkinHead()
    {
        if (SkinBitmap == null)
        {
            return;
        }
        var old = HeadBitmap;
        var config = GuiConfigUtils.Config.Head;
        using var data = config.Type switch
        {
            HeadType.Head2D => Skin2DHead.MakeHeadImage(SkinBitmap),
            HeadType.Head3D_A => Skin3DHeadA.MakeHeadImage(SkinBitmap),
            HeadType.Head3D_B => Skin3DHeadB.MakeHeadImage(SkinBitmap),
            _ => throw new IndexOutOfRangeException()
        };
        HeadBitmap = new Bitmap(data);
        old?.Dispose();
    }

    public static void SetDefaultHead()
    {
        RemoveSkin();
        HeadBitmap = null;
        App.OnSkinLoad();
    }

    public static void RemoveImage()
    {
        var image = BackBitmap;
        BackBitmap = null;
        App.OnPicUpdate();
        image?.Dispose();
    }

    public static async Task LoadImage()
    {
        var config = GuiConfigUtils.Config;
        RemoveImage();
        var file = config.BackImage;
        if (string.IsNullOrWhiteSpace(file))
        {
            file = "https://api.dujin.org/bing/1920.php";
        }

        if (config.EnableBG)
        {
            BackBitmap = await ImageUtils.MakeBackImage(
                    file, config.BackEffect,
                    config.BackLimit ? config.BackLimitValue : 100);
        }

        App.OnPicUpdate();
        App.ColorChange();
        FuntionUtils.RunGC();
    }

    public static void RemoveSkin()
    {
        SkinBitmap?.Dispose();
        CapeBitmap?.Dispose();
        HeadBitmap?.Dispose();

        HeadBitmap = null;
        SkinBitmap = null;
        CapeBitmap = null;
    }

    public static Bitmap? GetGameIcon(GameSettingObj obj)
    {
        if (s_gameIcon.TryGetValue(obj.UUID, out var image))
        {
            return image;
        }
        var file = obj.GetIconFile();
        if (File.Exists(file))
        {
            var icon = new Bitmap(file);
            s_gameIcon.Add(obj.UUID, icon);

            return icon;
        }

        return null;
    }
}
