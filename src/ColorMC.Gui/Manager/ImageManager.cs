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
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using MinecraftSkinRender;
using MinecraftSkinRender.Image;
using SkiaSharp;

namespace ColorMC.Gui.Manager;

/// <summary>
/// 图片缓存
/// </summary>
public static class ImageManager
{
    /// <summary>
    /// 游戏图标图片
    /// </summary>
    public static Bitmap GameIcon { get; private set; }
    /// <summary>
    /// 加载图标图片
    /// </summary>
    public static Bitmap LoadBitmap { get; private set; }
    /// <summary>
    /// 图标图片
    /// </summary>
    public static Bitmap Icon
    {
        get
        {
            if (GuiConfigUtils.Config.ServerCustom.CustomIcon)
            {
                return _customIcon ?? GameIcon;
            }

            return GameIcon;
        }
    }
    /// <summary>
    /// 窗口图标
    /// </summary>
    public static WindowIcon WindowIcon
    {
        get
        {
            if (GuiConfigUtils.Config.ServerCustom.CustomIcon)
            {
                return _customWindowIcon ?? _windowIcon;
            }

            return _windowIcon;
        }
    }

    /// <summary>
    /// 背景图片
    /// </summary>
    public static Bitmap? BackBitmap { get; private set; }
    /// <summary>
    /// 头像图片
    /// </summary>
    public static Bitmap? HeadBitmap { get; private set; }
    /// <summary>
    /// 皮肤图片
    /// </summary>
    public static SKBitmap? SkinBitmap { get; private set; }
    /// <summary>
    /// 披风图片
    /// </summary>
    public static SKBitmap? CapeBitmap { get; private set; }

    /// <summary>
    /// 星标图标
    /// </summary>
    public static readonly string[] Stars = ["/Resource/Icon/Item/StarFill.svg", "/Resource/Icon/Item/Star.svg"];
    /// <summary>
    /// 音乐播放暂停图标
    /// </summary>
    public static readonly string[] MusicIcons = ["/Resource/Icon/Button/Play.svg", "/Resource/Icon/Button/Pause.svg"];
    /// <summary>
    /// 最大化最小化图标
    /// </summary>
    public static readonly string[] MaxWindowsIcon = ["/Resource/Icon/Window/Maximize.svg", "/Resource/Icon/Window/Restore.svg"];
    /// <summary>
    /// Macos最大化最小化图标
    /// </summary>
    public static readonly string[] MaxMacosIcon = ["/Resource/Icon/Window/ArrowExpand.svg", "/Resource/Icon/Window/ArrowCollapse.svg"];
    /// <summary>
    /// 表情图标
    /// </summary>
    public static readonly string[] EmoticonIcon = ["/Resource/Icon/Emoticon/EmoticonHappy.svg", "/Resource/Icon/Emoticon/EmoticonConfused.svg",
        "/Resource/Icon/Emoticon/EmoticonExcited.svg", "/Resource/Icon/Emoticon/EmoticonSad.svg", "/Resource/Icon/Emoticon/EmoticonWink.svg",
        "/Resource/Icon/Emoticon/EmoticonFrown.svg"];
    /// <summary>
    /// 缓存路径
    /// </summary>
    private static string s_local;

    private static Bitmap? _customIcon;
    private static Bitmap? _startIcon;
    private static WindowIcon _customWindowIcon;
    private static WindowIcon _windowIcon;

    public static string ImagePath => s_local;

    /// <summary>
    /// 游戏实例图标
    /// </summary>
    private static readonly Dictionary<Guid, Bitmap> s_gameIcons = [];
    /// <summary>
    /// 角色皮肤
    /// </summary>
    private static readonly Dictionary<UserKeyObj, StringRes> s_userSkins = [];
    /// <summary>
    /// 角色披风
    /// </summary>
    private static readonly Dictionary<UserKeyObj, string?> s_userCapes = [];
    /// <summary>
    /// 角色皮肤获取锁
    /// </summary>
    private static readonly Dictionary<UserKeyObj, TaskCompletionSource> s_userGets = [];
    private static readonly Dictionary<string, Bitmap> s_blocks = [];

    /// <summary>
    /// 初始化图片缓存
    /// </summary>
    public static void Init()
    {
        //加载游戏图标
        using var asset = AssetLoader.Open(new Uri("resm:ColorMC.Gui.Resource.Pic.game.png"));
        GameIcon = new Bitmap(asset);

        //加载程序图标
        using var asset1 = AssetLoader.Open(new Uri(SystemInfo.Os == OsType.MacOs
            ? "resm:ColorMC.Gui.macicon.ico" : "resm:ColorMC.Gui.icon.ico"));
        _windowIcon = new(asset1!);

        //加载图片
        using var asset2 = AssetLoader.Open(new Uri("resm:ColorMC.Gui.Resource.Pic.load.png"));
        LoadBitmap = new(asset2!);

        s_local = Path.Combine(ColorMCGui.BaseDir, GuiNames.NameImageDir);

        Directory.CreateDirectory(s_local);

        LoadIcon();
        LoadStartIcon();
    }

    /// <summary>
    /// 加载自定义启动器图标
    /// </summary>
    public static void LoadIcon()
    {
        var path = GuiConfigUtils.Config.ServerCustom.IconFile;
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        path = Path.Combine(ColorMCGui.BaseDir, path);
        if (!File.Exists(path))
        {
            return;
        }

        _customIcon = new(path);
        _customWindowIcon = new(_customIcon);

        WindowManager.ReloadIcon();
    }

    /// <summary>
    /// 加载自定义启动界面图标
    /// </summary>
    public static void LoadStartIcon()
    {
        var path = GuiConfigUtils.Config.ServerCustom.StartIconFile;
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        path = Path.Combine(ColorMCGui.BaseDir, path);
        if (!File.Exists(path))
        {
            return;
        }

        _startIcon = new(path);
    }

    /// <summary>
    /// 从网络获取皮肤
    /// </summary>
    /// <param name="key"></param>
    /// <param name="login"></param>
    /// <returns></returns>
    private static async Task GetSkinAsync(UserKeyObj key, LoginObj login)
    {
        var task = new TaskCompletionSource();
        s_userGets[key] = task;
        var temp1 = await PlayerSkinAPI.DownloadSkinAsync(login);
        if (temp1.Skin != null)
        {
            s_userSkins[key] = new StringRes { Data = temp1.Skin, State = temp1.IsNewSlim };
        }
        else
        {
            s_userSkins[key] = new StringRes();
        }
        if (temp1.Cape != null)
        {
            s_userCapes[key] = temp1.Cape;
        }
        task.SetResult();
    }

    /// <summary>
    /// 获取用户披风
    /// </summary>
    /// <param name="login"></param>
    /// <returns></returns>
    public static async Task<string?> GetUserCapeAsync(LoginObj login)
    {
        var key = login.GetKey();
        if (s_userGets.TryGetValue(key, out var temp))
        {
            await temp.Task;
        }

        if (s_userCapes.TryGetValue(key, out var file))
        {
            return file;
        }

        await GetSkinAsync(key, login);

        if (s_userCapes.TryGetValue(key, out file))
        {
            return file;
        }

        return null;
    }

    /// <summary>
    /// 获取皮肤
    /// </summary>
    /// <param name="login"></param>
    /// <returns></returns>
    public static async Task<StringRes> GetUserSkinAsync(LoginObj login)
    {
        var key = login.GetKey();
        if (s_userGets.TryGetValue(key, out var temp))
        {
            await temp.Task;
        }

        if (s_userSkins.TryGetValue(key, out var file))
        {
            return file;
        }

        await GetSkinAsync(key, login);

        if (s_userSkins.TryGetValue(key, out file))
        {
            return file;
        }

        return new StringRes();
    }

    /// <summary>
    /// 生成皮肤图片
    /// </summary>
    /// <param name="bitmap">皮肤图片</param>
    /// <param name="issim">是否为纤细</param>
    /// <returns></returns>
    public static Bitmap GenSkinImage(SKBitmap bitmap, bool issim)
    {
        using var img = Skin2DTypeB.MakeSkinImage(bitmap, issim ? SkinType.NewSlim : null);
        return img.ToBitmap();
    }

    /// <summary>
    /// 生成头像图片
    /// </summary>
    public static Bitmap GenHeadImage(SKBitmap bitmap)
    {
        var config = GuiConfigUtils.Config.Head;
        if (config.Type == HeadType.Head3D_A)
        {
            using var img = Skin3DHeadTypeA.MakeHeadImage(bitmap);
            return img.ToBitmap();
        }
        else if (config.Type == HeadType.Head3D_B)
        {
            using var img = Skin3DHeadTypeB.MakeHeadImage(bitmap, config.X, config.Y);
            return img.ToBitmap();
        }
        else if (config.Type == HeadType.Head2D_B)
        {
            using var img = Skin2DHeadTypeB.MakeHeadImage(bitmap);
            return img.ToBitmap();
        }
        else
        {
            using var img = Skin2DHeadTypeA.MakeHeadImage(bitmap);
            return img.ToBitmap();
        }
    }

    /// <summary>
    /// 生成皮肤头像
    /// </summary>
    /// <param name="login">登录的账户</param>
    public static async Task LoadSkinHeadAsync(LoginObj login)
    {
        var file = await GetUserSkinAsync(login);
        var file1 = await GetUserCapeAsync(login);
        if (file.Data == null || !File.Exists(file.Data))
        {
            SetDefaultHead();
        }
        else
        {
            try
            {
                var old = SkinBitmap;
                var old1 = HeadBitmap;
                SkinBitmap = SKBitmap.Decode(file.Data);
                HeadBitmap = GenHeadImage(SkinBitmap);
                old?.Dispose();
                old1?.Dispose();
            }
            catch (Exception e)
            {
                Logs.Error(string.Format(LangUtils.Get("App.Text92"), file), e);
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
                Logs.Error(string.Format(LangUtils.Get("App.Text93"), file), e);
            }
        }

        EventManager.OnSkinChange();
    }

    /// <summary>
    /// 重新获取皮肤头像
    /// </summary>
    public static void ReloadHead()
    {
        if (SkinBitmap == null)
        {
            return;
        }
        var old = HeadBitmap;
        HeadBitmap = GenHeadImage(SkinBitmap);
        old?.Dispose();
        EventManager.OnSkinChange();
    }

    /// <summary>
    /// 设置默认头像
    /// </summary>
    public static void SetDefaultHead()
    {
        RemoveSkin();
        HeadBitmap = null;
        EventManager.OnSkinChange();
    }

    /// <summary>
    /// 删除背景图
    /// </summary>
    public static void RemoveImage()
    {
        var image = BackBitmap;
        BackBitmap = null;
        EventManager.OnBGChange();
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
        //bing壁纸
        if (string.IsNullOrWhiteSpace(file))
        {
            file = "https://api.dujin.org/bing/1920.php";
        }
        //自定义壁纸
        if (config.EnableBG)
        {
            BackBitmap = await MakeBackImage(file, config.BackEffect,
                config.BackLimit ? config.BackLimitValue : 100);
        }

        EventManager.OnBGChange();
        ThemeManager.Init();
        FunctionUtils.RunGC();
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
    /// 获取方块图片
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>图片</returns>
    private static Bitmap? GetGameBlockIcon(GameSettingObj obj)
    {
        var block = GameManager.GetGameBlock(obj);
        if (!string.IsNullOrWhiteSpace(block) && BlockTexUtils.Unlocks.List.Contains(block))
        {
            return GetBlockIconWithKey(block);
        }

        return null;
    }

    /// <summary>
    /// 获取方块图片
    /// </summary>
    /// <param name="key">方块ID</param>
    /// <returns>图片</returns>
    private static Bitmap? GetBlockIconWithKey(string key)
    {
        if (s_blocks.TryGetValue(key, out var image))
        {
            return image;
        }
        var file = BlockTexUtils.GetTexWithKey(key);
        if (file != null && File.Exists(file))
        {
            var icon = new Bitmap(file);
            s_blocks.Add(key, icon);

            return icon;
        }

        return null;
    }

    /// <summary>
    /// 重载游戏实例图标
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>图标</returns>
    public static void ReloadImage(GameSettingObj obj)
    {
        if (s_gameIcons.Remove(obj.UUID, out var temp))
        {
            Dispatcher.UIThread.Post(temp.Dispose);
        }
        var icon = GetGameIcon(obj);
        EventManager.OnGameIconChange(obj.UUID);
    }

    /// <summary>
    /// 获取游戏实例图标
    /// </summary>
    /// <param name="obj">游戏实例</param>
    /// <returns>图标</returns>
    public static Bitmap? GetGameIcon(GameSettingObj obj)
    {
        var file = obj.GetIconFile();
        var ex = File.Exists(file);
        if (!ex && GetGameBlockIcon(obj) is { } img)
        {
            return img;
        }
        if (s_gameIcons.TryGetValue(obj.UUID, out var image))
        {
            return image;
        }
        if (ex)
        {
            using var stream = PathHelper.OpenRead(file)!;
            var icon = Bitmap.DecodeToWidth(stream, 70);
            s_gameIcons.Add(obj.UUID, icon);

            return icon;
        }

        return null;
    }

    /// <summary>
    /// 获取方块图片
    /// </summary>
    /// <param name="key">方块ID</param>
    /// <returns>图片</returns>
    public static Bitmap? GetBlockIcon(string key, string path)
    {
        if (s_blocks.TryGetValue(key, out var image))
        {
            return image;
        }
        var file = BlockTexUtils.GetTex(path);
        if (File.Exists(file))
        {
            var icon = new Bitmap(file);
            s_blocks.Add(key, icon);

            return icon;
        }

        return null;
    }

    /// <summary>
    /// 加载图片
    /// </summary>
    /// <param name="url">网址</param>
    /// <returns>位图</returns>
    public static async Task<Bitmap?> LoadAsync(string url, int zoom)
    {
        if (!Directory.Exists(s_local))
        {
            Directory.CreateDirectory(s_local);
        }
        //存在缓存
        var sha1 = HashHelper.GenSha256(url);
        string file = Path.Combine(s_local, sha1) + ".png";

        if (!File.Exists(file))
        {
            try
            {
                //从网络读取图片
                using var data1 = await CoreHttpClient.GetStreamAsync(url);
                if (data1 == null)
                {
                    return null;
                }
                using var temp = new MemoryStream();
                await data1.CopyToAsync(temp);
                temp.Seek(0, SeekOrigin.Begin);
                using var image1 = SKBitmap.Decode(temp);
                using var data = image1.Encode(SKEncodedImageFormat.Png, 100);
                await PathHelper.WriteBytesAsync(file, data.AsStream());
            }
            catch (Exception e)
            {
                Logs.Error(LangUtils.Get("App.Text91"), e);
                return null;
            }
        }

        if (zoom > 0)
        {
            using var temp = PathHelper.OpenRead(file);
            if (temp == null)
            {
                return null;
            }
            return Bitmap.DecodeToWidth(temp, zoom);
        }
        else
        {
            return new Bitmap(file);
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
            Stream? stream = null;
            try
            {
                if (file.StartsWith("https://") || file.StartsWith("http://"))
                {
                    var res = await CoreHttpClient.GetAsync(file);
                    stream = await res.Content.ReadAsStreamAsync();
                }
                else if (file.StartsWith("ColorMC.Gui"))
                {
                    var assm = Assembly.GetExecutingAssembly();
                    stream = assm.GetManifestResourceStream(file)!;
                }
                else
                {
                    stream = PathHelper.OpenRead(file);
                }
                if (stream == null)
                {
                    return null;
                }
                //是否需要限制图片大小或者高斯模糊
                if (value > 0 || (lim != 100 && lim > 0))
                {
                    var image = SKBitmap.Decode(stream);
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
                    return new Bitmap(stream);
                }
            }
            catch (Exception e)
            {
                Logs.Error(LangUtils.Get("App.Text90"), e);
                return null;
            }
            finally
            {
                stream?.Close();
                stream?.Dispose();
            }
        });
    }

    public static Bitmap? GetCustomIcon()
    {
        return _customIcon;
    }

    public static Bitmap? GetStartIcon()
    {
        return _startIcon;
    }

    public static Bitmap? GenCapeImage(SKBitmap cape)
    {
        using var temp = Cape2DTypaA.MakeCapeImage(cape);
        return temp.ToBitmap();
    }

    public static void Open(string url)
    {
        var sha1 = HashHelper.GenSha256(url);
        string file = Path.Combine(s_local, sha1) + ".png";
        if (File.Exists(file))
        {
            PathBinding.OpenPicFile(file);
        }
    }
}
