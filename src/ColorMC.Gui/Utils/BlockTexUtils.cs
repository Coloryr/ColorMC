using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ColorMC.Core;
using ColorMC.Core.Config;
using ColorMC.Core.Downloader;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using MinecraftSkinRender.Image;
using SharpCompress.Archives.Zip;
using SkiaSharp;

namespace ColorMC.Gui.Utils;

/// <summary>
/// 幸运方块处理
/// </summary>
public static class BlockTexUtils
{
    /// <summary>
    /// 运行路径
    /// </summary>
    private static string s_local;
    /// <summary>
    /// 配置文件
    /// </summary>
    private static string s_file;
    /// <summary>
    /// 方块背包存储
    /// </summary>
    private static string s_unlockFile;

    /// <summary>
    /// 方块贴图列表
    /// </summary>
    public static BlocksObj Blocks { get; private set; }
    /// <summary>
    /// 方块背包
    /// </summary>
    public static BlockUnlockObj Unlocks { get; private set; }

    /// <summary>
    /// 随机数
    /// </summary>
    private static readonly Random s_random = new();

    /// <summary>
    /// 贴图顶点 Vertice
    /// </summary>
    private static readonly SKPoint3[] s_cubeVertices =
    [
        // Front face
        new SKPoint3(-1, -1,  1),
        new SKPoint3( 1, -1,  1),
        new SKPoint3( 1,  1,  1),
        new SKPoint3(-1,  1,  1),
        // Back face
        new SKPoint3(-1, -1, -1),
        new SKPoint3( 1, -1, -1),
        new SKPoint3( 1,  1, -1),
        new SKPoint3(-1,  1, -1),
    ];

    /// <summary>
    /// 定义立方体索引 Index
    /// </summary>
    private static readonly ushort[] s_cubeIndices =
    [
        0, 4, 7, 3, // Back face
        0, 4, 5, 1, // Bottom face
        0, 1, 2, 3, // Right face
        3, 7, 6, 2, // Top face
        4, 5, 6, 7, // Left face
        1, 5, 6, 2, // Front face
    ];

    /// <summary>
    /// 定义原始图像的四个顶点 UV
    /// </summary>
    private static readonly SKPoint[] s_sourceUV =
    [
        // Back face
        new SKPoint(0, 1),
        new SKPoint(1, 1),
        new SKPoint(1, 0),
        new SKPoint(0, 0),
        // Bottom face
        new SKPoint(1, 0),
        new SKPoint(0, 0),
        new SKPoint(0, 1),
        new SKPoint(1, 1),
        // Right face
        new SKPoint(1, 1),
        new SKPoint(0, 1),
        new SKPoint(0, 0),
        new SKPoint(1, 0),
        // Top face
        new SKPoint(0, 0),
        new SKPoint(0, 1),
        new SKPoint(1, 1),
        new SKPoint(1, 0),
        // Left face
        new SKPoint(0, 1),
        new SKPoint(1, 1),
        new SKPoint(1, 0),
        new SKPoint(0, 0),
        // Front face
        new SKPoint(1, 1),
        new SKPoint(0, 1),
        new SKPoint(0, 0),
        new SKPoint(1, 0),
    ];

    private static DateTime _checkTime;

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        s_local = Path.Combine(ColorMCGui.BaseDir, GuiNames.NameBlockDir);
        if (!Directory.Exists(s_local))
        {
            Directory.CreateDirectory(s_local);
        }

        s_file = Path.Combine(s_local, GuiNames.NameBlockFile);

        s_unlockFile = Path.Combine(InnerPath.Inner, GuiNames.NameBlockFile);

        LoadState();
        LoadUnlock();

        new Thread(CheckTime)
        {
            Name = "ColorMC Block Day Check",
            IsBackground = true
        }.Start();
    }

    /// <summary>
    /// 检查日期线程
    /// </summary>
    private static void CheckTime()
    {
        _checkTime = DateTime.Now;
        while (true)
        {
            var time = DateTime.Now;
            if (_checkTime.Year != time.Year
                || _checkTime.Month != time.Month
                || _checkTime.Day != time.Day)
            {
                _checkTime = time;
                WindowManager.MainWindow?.ReloadBlock();
            }

            Thread.Sleep(1000);
        }
    }

    /// <summary>
    /// 是否已经抽过了
    /// </summary>
    /// <returns>今日是否已经抽取</returns>
    public static bool IsGet()
    {
        var time = DateTime.Now;
        if (Unlocks.Time.Year != time.Year
            || Unlocks.Time.Month != time.Month
            || Unlocks.Time.Day != time.Day)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 设置今日抽取
    /// </summary>
    /// <param name="key">方块ID</param>
    public static void SetToday(string key)
    {
        Unlocks.List.Add(key);
        Unlocks.Time = DateTime.Now;
        Unlocks.Today = key;
        SaveUnlock();

        WindowManager.BlockBackpackWindow?.Reload();
        WindowManager.MainWindow?.ReloadBlock();
    }

    /// <summary>
    /// 获取贴图路径
    /// </summary>
    /// <param name="key">方块ID</param>
    /// <returns>贴图路径</returns>
    public static string? GetTexWithKey(string key)
    {
        if (Blocks.Tex.TryGetValue(key, out var tex))
        {
            return Path.Combine(s_local, tex);
        }

        return null;
    }

    /// <summary>
    /// 获取贴图路径
    /// </summary>
    /// <param name="file">文件名</param>
    /// <returns>贴图路径</returns>
    public static string GetTex(string file)
    {
        return Path.Combine(s_local, file);
    }

    /// <summary>
    /// 加载游戏版本的所有方块贴图
    /// </summary>
    /// <returns>是否成功加载</returns>
    public static async Task<StringRes> LoadNow()
    {
        var versions = await VersionPath.GetVersionsAsync();
        if (versions == null)
        {
            return new StringRes { Data = LanguageUtils.Get("LuckBlockWindow.Text6") };
        }

        //获取最新游戏版本
        var last = versions.Latest.Release;

        if (Blocks.Id == last)
        {
            return new StringRes { State = true };
        }

        var obj = await VersionPath.CheckUpdateAsync(last);
        if (obj == null)
        {
            return new StringRes { Data = LanguageUtils.Get("LuckBlockWindow.Text6") };
        }

        //下载游戏核心
        var ass = GameDownloadHelper.BuildGameItem(last);
        if (!File.Exists(ass.Local))
        {
            var res = await DownloadManager.StartAsync([ass]);
            if (!res)
            {
                return new StringRes { Data = LanguageUtils.Get("LuckBlockWindow.Text7") };
            }
        }
        using var stream = PathHelper.OpenRead(ass.Local);
        if (stream == null)
        {
            return new StringRes { Data = LanguageUtils.Get("LuckBlockWindow.Text8") };
        }

        //提取贴图
        using var reader = ZipArchive.Open(stream);
        foreach (var item in reader.Entries)
        {
            if (!FuntionUtils.IsFile(item))
            {
                continue;
            }
            if (item.Key?.Contains("assets/minecraft/models/block/") == false
                || item.Key?.EndsWith(Names.NameJsonExt) == false)
            {
                continue;
            }

            using var stream1 = item.OpenEntryStream();
            var obj1 = await JsonDocument.ParseAsync(stream1);
            if (!obj1.RootElement.TryGetProperty("parent", out var temp)
                || temp.ValueKind != JsonValueKind.String
                || temp.GetString() != "minecraft:block/cube_all")
            {
                //只要完整方块
                continue;
            }

            var tex = obj1.RootElement.GetProperty("textures").GetProperty("all").GetString();
            if (tex == null)
            {
                continue;
            }

            var id = Path.GetFileName(item.Key!).Replace(Names.NameJsonExt, "");
            var file = tex.Replace("minecraft:block/", "") + ".png";
            var item1 = reader.GetEntry("assets/minecraft/textures/block/" + file);
            if (item1 == null)
            {
                continue;
            }
            using var stream2 = item1.OpenEntryStream();
            var img = SKBitmap.Decode(stream2);

            //渲染方块
            var block = MakeBlockImage(ref img);

            img.Dispose();

            block.SavePng(Path.Combine(s_local, file));
            Blocks.Tex[id] = file;
        }

        Blocks.Id = last;
        SaveState();

        return new StringRes() { State = true };
    }

    /// <summary>
    /// 加载窗口位置文件
    /// </summary>
    private static void LoadState()
    {
        if (File.Exists(s_file))
        {
            try
            {
                using var stream = PathHelper.OpenRead(s_file);
                var state = JsonUtils.ToObj(stream, JsonGuiType.BlocksObj);
                if (state == null)
                {
                    return;
                }

                Blocks = state;
            }
            catch (Exception e)
            {
                Logs.Error(LanguageUtils.Get("App.Error4"), e);
            }
        }
        Blocks ??= new()
        {
            Tex = []
        };
        Blocks.Tex ??= [];

        SaveState();
    }

    /// <summary>
    /// 加载保存的账户
    /// </summary>
    private static void LoadUnlock()
    {
        try
        {
            using var stream = PathHelper.OpenRead(s_unlockFile);
            var list = JsonUtils.ToObj(stream, JsonGuiType.BlockUnlockObj);
            if (list == null)
            {
                Unlocks = new()
                {
                    List = []
                };
                return;
            }

            Unlocks = list;
            Unlocks.List ??= [];
        }
        catch
        {
            SaveUnlock();
        }
    }

    /// <summary>
    /// 保存
    /// </summary>
    public static void SaveUnlock()
    {
        ConfigSave.AddItem(ConfigSaveObj.Build(GuiNames.NameBlockFile, s_unlockFile, Unlocks, JsonGuiType.BlockUnlockObj));
    }

    /// <summary>
    /// 保存窗口状态
    /// </summary>
    private static void SaveState()
    {
        ConfigSave.AddItem(ConfigSaveObj.Build("ColorMC_Block", s_file, Blocks, JsonGuiType.BlocksObj));
    }

    /// <summary>
    /// 创建方块图片
    /// </summary>
    /// <param name="tex">方块贴图</param>
    /// <returns>方块图片</returns>
    public static SKImage MakeBlockImage(ref SKBitmap tex)
    {
        //有动态材质则随便选中一个
        if (tex.Height > 16)
        {
            var size = tex.Height / 16;
            var index = s_random.Next(0, size);

            var sourceBitmap = new SKBitmap(16, 16);
            tex.ExtractSubset(sourceBitmap, new SKRectI(0, index * 16, 16, index * 16 + 16));
            tex.Dispose();
            tex = sourceBitmap;
        }

        // 创建绘图表面
        int width = 200;
        int height = 200;
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;

        // 绘制方块
        DrawBlock3D(canvas, tex);

        // 保存结果到文件
        return surface.Snapshot();
    }

    private static void DrawBlock3D(SKCanvas canvas, SKBitmap texture)
    {
        var transform = CreateTransformMatrix();

        for (int i = 0; i < s_cubeIndices.Length / 4; i++)
        {
            DrawTexturedFace(canvas, texture, transform, i);
        }
    }

    /// <summary>
    /// 创建3D变换矩阵
    /// </summary>
    /// <returns></returns>
    private static SKMatrix44 CreateTransformMatrix()
    {
        var transform = SKMatrix44.CreateIdentity();

        // 平移图像到原点
        var translateToOrigin = SKMatrix44.CreateIdentity();
        translateToOrigin.SetTranslate(-50, -50, 0);

        //// 旋转矩阵
        var rotationX = CreateRotationMatrix(30, 1, 0, 0);
        var rotationY = CreateRotationMatrix(45, 0, 1, 0);

        // 将旋转矩阵相乘
        transform.PreConcat(translateToOrigin);
        transform.PreConcat(rotationX);
        transform.PreConcat(rotationY);

        // 缩放
        var scale = SKMatrix44.CreateIdentity();
        scale.SetScale(55, -55, 55);
        transform.PreConcat(scale);

        // Step 4: 平移图像到画布中心
        var translateToCenter = SKMatrix44.CreateIdentity();
        translateToCenter.SetTranslate(3.83f, -1.63f, 0);
        transform.PreConcat(translateToCenter);

        return transform;
    }

    /// <summary>
    /// 创建旋转矩阵
    /// </summary>
    /// <param name="degrees"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    private static SKMatrix44 CreateRotationMatrix(float degrees, float x, float y, float z)
    {
        var radians = degrees * (float)Math.PI / 180.0f;
        var cos = (float)Math.Cos(radians);
        var sin = (float)Math.Sin(radians);
        var oneMinusCos = 1.0f - cos;

        var rotation = SKMatrix44.CreateIdentity();
        rotation[0, 0] = cos + x * x * oneMinusCos;
        rotation[0, 1] = x * y * oneMinusCos - z * sin;
        rotation[0, 2] = x * z * oneMinusCos + y * sin;
        rotation[1, 0] = y * x * oneMinusCos + z * sin;
        rotation[1, 1] = cos + y * y * oneMinusCos;
        rotation[1, 2] = y * z * oneMinusCos - x * sin;
        rotation[2, 0] = z * x * oneMinusCos - y * sin;
        rotation[2, 1] = z * y * oneMinusCos + x * sin;
        rotation[2, 2] = cos + z * z * oneMinusCos;

        return rotation;
    }


    /// <summary>
    /// 应用3D变换并投影到2D
    /// </summary>
    /// <param name="mat"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    private static SKPoint Project(SKMatrix44 mat, SKPoint3 v)
    {
        // 创建一个4D向量
        float[] vec = [v.X, v.Y, v.Z, 1];
        float[] result = new float[4];

        // 执行矩阵乘法
        mat.MapScalars(vec, result);

        // 进行透视除法
        if (result[3] != 0)
        {
            result[0] /= result[3];
            result[1] /= result[3];
            result[2] /= result[3];
        }

        return new SKPoint(result[0], result[1]);
    }

    /// <summary>
    /// 绘制一个面
    /// </summary>
    /// <param name="canvas">画布</param>
    /// <param name="texture">贴图</param>
    /// <param name="transform">变换</param>
    /// <param name="index">位于第几个面</param>
    private static void DrawTexturedFace(SKCanvas canvas, SKBitmap texture, SKMatrix44 transform, int index)
    {
        var vertices = new SKPoint[4];
        var texCoords = new SKPoint[4];

        index *= 4;
        for (int j = 0; j < 4; ++j)
        {
            vertices[j] = Project(transform, s_cubeVertices[s_cubeIndices[index + j]]);
            texCoords[j] = new SKPoint(s_sourceUV[index + j].X * texture.Width, s_sourceUV[index + j].Y * texture.Height);
        }

        var skVertices = SKVertices.CreateCopy(SKVertexMode.TriangleFan, vertices, texCoords, null);

        using var paint = new SKPaint
        {
            IsAntialias = true,
            Shader = SKShader.CreateBitmap(texture, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp)
        };

        // 绘制带纹理的面
        canvas.DrawVertices(skVertices, SKBlendMode.SrcOver, paint);
    }
}
