using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Skin;
using ColorMC.Gui.Utils;

using SkiaSharp;

namespace ColorMC.Gui.Manager;

/// <summary>
/// 图片缓存
/// </summary>
public static class ImageManager
{
    public const string Name1 = "image";

    /// <summary>
    /// 游戏图标图片
    /// </summary>
    public static Bitmap GameIcon { get; private set; }
    /// <summary>
    /// 加载图标图片
    /// </summary>
    public static Bitmap LoadIcon { get; private set; }
    /// <summary>
    /// 窗口图标
    /// </summary>
    public static WindowIcon WindowIcon { get; private set; }

    /// <summary>
    /// 背景图片
    /// </summary>
    public static Bitmap? BackBitmap { get; private set; }
    /// <summary>
    /// 皮肤图片
    /// </summary>
    public static SKBitmap? SkinBitmap { get; private set; }
    /// <summary>
    /// 披风图片
    /// </summary>
    public static SKBitmap? CapeBitmap { get; private set; }
    /// <summary>
    /// 头像图片
    /// </summary>
    public static Bitmap? HeadBitmap { get; private set; }

    /// <summary>
    /// 游戏实例图标
    /// </summary>
    private static readonly Dictionary<string, Bitmap> s_gameIcon = [];

    /// <summary>
    /// 背景图片更新
    /// </summary>
    public static event Action? BGUpdate;
    /// <summary>
    /// 皮肤更新
    /// </summary>
    public static event Action? SkinChange;

    /// <summary>
    /// 星标SVG
    /// </summary>
    public static readonly string[] Stars = ["/Resource/Icon/Item/star.svg", "/Resource/Icon/Item/star_1.svg"];


    public static readonly string[] MusicIcons = [ "/Resource/Icon/play.svg", "/Resource/Icon/pause.svg" ];

    /// <summary>
    /// 运行路径
    /// </summary>
    private static string s_local;

    public static string GetImagePath() => s_local;

    /// <summary>
    /// 初始化图片缓存
    /// </summary>
    public static void Init()
    {
        {
            using var asset = AssetLoader.Open(new Uri("resm:ColorMC.Gui.Resource.Pic.game.png"));
            GameIcon = new Bitmap(asset);
        }
        {
            using var asset1 = AssetLoader.Open(new Uri(SystemInfo.Os == OsType.MacOS
                ? "resm:ColorMC.Gui.macicon.ico" : "resm:ColorMC.Gui.icon.ico"));
            WindowIcon = new(asset1!);
        }
        {
            using var asset1 = AssetLoader.Open(new Uri("resm:ColorMC.Gui.Resource.Pic.load.png"));
            LoadIcon = new(asset1!);
        }

        s_local = Path.Combine(ColorMCGui.RunDir, Name1);

        Directory.CreateDirectory(s_local);
    }

    /// <summary>
    /// 生成皮肤头像
    /// </summary>
    /// <param name="file">皮肤</param>
    /// <param name="file1">披风</param>
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
                    HeadType.Head3D_A => Skin3DHeadA.MakeHeadImage(SkinBitmap),
                    HeadType.Head3D_B => Skin3DHeadB.MakeHeadImage(SkinBitmap),
                    _ => Skin2DHead.MakeHeadImage(SkinBitmap)
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
        if (file1 != null && File.Exists(file1))
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

        OnSkinLoad();
    }

    /// <summary>
    /// 重新获取皮肤头像
    /// </summary>
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
            HeadType.Head3D_A => Skin3DHeadA.MakeHeadImage(SkinBitmap),
            HeadType.Head3D_B => Skin3DHeadB.MakeHeadImage(SkinBitmap),
            _ => Skin2DHead.MakeHeadImage(SkinBitmap)
        };
        HeadBitmap = new Bitmap(data);
        old?.Dispose();
        OnSkinLoad();
    }

    /// <summary>
    /// 设置默认头像
    /// </summary>
    public static void SetDefaultHead()
    {
        RemoveSkin();
        HeadBitmap = null;
        OnSkinLoad();
    }

    /// <summary>
    /// 删除背景图
    /// </summary>
    public static void RemoveImage()
    {
        var image = BackBitmap;
        BackBitmap = null;
        OnPicUpdate();
        image?.Dispose();
    }

    /// <summary>
    /// 加载背景图
    /// </summary>
    /// <returns></returns>
    public static async Task LoadBGImage()
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
            BackBitmap = await MakeBackImage(file, config.BackEffect,
                config.BackLimit ? config.BackLimitValue : 100);
        }

        OnPicUpdate();
        ThemeManager.Init();
        FuntionUtils.RunGC();
    }

    /// <summary>
    /// 删除皮肤图片
    /// </summary>
    public static void RemoveSkin()
    {
        SkinBitmap?.Dispose();
        CapeBitmap?.Dispose();
        HeadBitmap?.Dispose();

        HeadBitmap = null;
        SkinBitmap = null;
        CapeBitmap = null;
    }

    /// <summary>
    /// 重载游戏实例图标
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>图标</returns>
    public static Bitmap? ReloadImage(GameSettingObj obj)
    {
        s_gameIcon.Remove(obj.UUID, out var temp);
        if (temp != null)
        {
            Dispatcher.UIThread.Post(temp.Dispose);
        }
        return GetGameIcon(obj);
    }

    /// <summary>
    /// 获取游戏实例图标
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>图标</returns>
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

    /// <summary>
    /// 通知背景图修改
    /// </summary>
    public static void OnPicUpdate()
    {
        BGUpdate?.Invoke();
    }

    /// <summary>
    /// 通知皮肤修改
    /// </summary>
    public static void OnSkinLoad()
    {
        SkinChange?.Invoke();
    }

    /// <summary>
    /// 加载图片
    /// </summary>
    /// <param name="url">网址</param>
    /// <returns>位图</returns>
    public static async Task<Bitmap?> Load(string url, bool zoom)
    {
        if (!Directory.Exists(s_local))
        {
            Directory.CreateDirectory(s_local);
        }
        //存在缓存
        var sha1 = HashHelper.GenSha256(url);
        if (File.Exists(s_local + sha1))
        {
            return new Bitmap(s_local + sha1);
        }
        else
        {
            try
            {
                //从网络读取图片
                var data1 = await CoreHttpClient.GetStreamAsync(url);
                if (data1.State)
                {
                    //压缩图片
                    if (zoom)
                    {
                        using var image1 = SKBitmap.Decode(data1.Stream!);
                        using var image2 = ImageUtils.Resize(image1, 100, 100);
                        using var data = image2.Encode(SKEncodedImageFormat.Png, 100);
                        PathHelper.WriteBytes(s_local + sha1, data.AsStream());
                        return new Bitmap(s_local + sha1);
                    }
                    else
                    {
                        PathHelper.WriteBytes(s_local + sha1, data1.Stream!);
                        return new Bitmap(s_local + sha1);
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("ImageUtils.Error2"), e);
            }

            return GameIcon;
        }
    }

    /// <summary>
    /// 获得背景图
    /// </summary>
    /// <param name="file">文件</param>
    /// <param name="value">模糊度</param>
    /// <param name="lim">分辨率限制</param>
    /// <returns></returns>
    private static Task<Bitmap?> MakeBackImage(string file, int value, int lim)
    {
        return Task.Run(async () =>
        {
            try
            {
                Stream? stream1 = null;
                if (file.StartsWith("https://") || file.StartsWith("http://"))
                {
                    var res = await CoreHttpClient.DownloadClient.GetAsync(file);
                    stream1 = res.Content.ReadAsStream();
                }
                else if (file.StartsWith("ColorMC.Gui"))
                {
                    var assm = Assembly.GetExecutingAssembly();
                    stream1 = assm.GetManifestResourceStream(file)!;
                }
                else if (SystemInfo.Os == OsType.Android)
                {
                    file = Path.Combine(ColorMCGui.RunDir, "BG");
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
                //是否需要限制图片大小或者高斯模糊
                if (value > 0 || (lim != 100 && lim > 0))
                {
                    var image = SKBitmap.Decode(stream1);
                    if (lim != 100 && lim > 0)
                    {
                        int x = (int)(image.Width * (float)lim / 100);
                        int y = (int)(image.Height * (float)lim / 100);
                        var img1 = image.Resize(new SKSizeI(x, y), SKFilterQuality.High);
                        image.Dispose();
                        image = img1;
                    }

                    //进行高斯模糊
                    if (value > 0)
                    {
                        var image1 = new SKBitmap(image.Width, image.Height);
                        var canvas = new SKCanvas(image1);

                        var paint = new SKPaint
                        {
                            ImageFilter = SKImageFilter.CreateBlur(value, value)
                        };

                        canvas.DrawBitmap(image, new SKPoint(0, 0), paint);
                        canvas.Flush();
                        image.Dispose();
                        image = image1;
                    }
                    using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                    return new Bitmap(data.AsStream());
                }
                else
                {
                    return new Bitmap(stream1);
                }
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("ImageUtils.Error1"), e);
                return null;
            }
        });
    }
}
