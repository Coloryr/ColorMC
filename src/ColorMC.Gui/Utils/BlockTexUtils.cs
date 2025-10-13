using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ColorMC.Core;
using ColorMC.Core.Config;
using ColorMC.Core.Downloader;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using MinecraftSkinRender.Image;
using SharpCompress.Archives.Zip;
using SkiaSharp;

namespace ColorMC.Gui.Utils;

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

    public static BlocksObj Blocks { get; private set; }
    /// <summary>
    /// 方块背包
    /// </summary>
    public static BlockUnlockObj Unlocks { get; private set; }

    private static readonly Random s_random = new();

    public static void Init()
    {
        s_local = Path.Combine(ColorMCGui.BaseDir, GuiNames.NameBlockDir);

        s_file = Path.Combine(s_local, GuiNames.NameBlockFile);

        var path = SystemInfo.Os == OsType.MacOS ?
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ColorMC") :
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ColorMC");

        Directory.CreateDirectory(path);

        s_unlockFile = Path.Combine(path, GuiNames.NameBlockFile);

        LoadState();
        LoadUnlock();
    }

    public static string GetTex(string file)
    {
        return Path.Combine(s_local, file);
    }

    public static async Task<StringRes> LoadNow()
    {
        var versions = await VersionPath.GetVersionsAsync();
        if (versions == null)
        {
            return new StringRes { Data = App.Lang("LuckBlockWindow.Error1") };
        }

        var last = versions.Latest.Release;

        if (Blocks.Id == last)
        {
            return new StringRes { State = true };
        }

        var obj = await VersionPath.CheckUpdateAsync(last);
        if (obj == null)
        {
            return new StringRes { Data = App.Lang("LuckBlockWindow.Error1") };
        }

        var ass = GameDownloadHelper.BuildGameItem(last);
        if (!File.Exists(ass.Local))
        {
            var res = await DownloadManager.StartAsync([ass]);
            if (!res)
            {
                return new StringRes { Data = App.Lang("LuckBlockWindow.Error2") };
            }
        }
        using var stream = PathHelper.OpenRead(ass.Local);
        if (stream == null)
        {
            return new StringRes { Data = App.Lang("LuckBlockWindow.Error3") };
        }
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
                Blocks.Tex ??= [];
            }
            catch (Exception e)
            {
                Logs.Error(App.Lang("App.Error3"), e);
            }
        }
        else
        {
            Blocks = new()
            {
                Tex = []
            };
            SaveState();
        }
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

    // 定义立方体索引
    private static readonly ushort[] s_cubeIndices =
    [
        0, 4, 7, 3, // Back face
        0, 4, 5, 1, // Bottom face
        0, 1, 2, 3, // Right face
        3, 7, 6, 2, // Top face
        4, 5, 6, 7, // Left face
        1, 5, 6, 2, // Front face
    ];

    // 定义原始图像的四个顶点
    private static readonly SKPoint[] s_sourceVertices =
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

    public static SKImage MakeBlockImage(ref SKBitmap tex)
    {
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

        // 绘制头部
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

    // 创建3D变换矩阵
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

    // 创建旋转矩阵
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


    // 应用3D变换并投影到2D
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
            texCoords[j] = new SKPoint(s_sourceVertices[index + j].X * texture.Width, s_sourceVertices[index + j].Y * texture.Height);
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
