using ColorMC.Gui.Utils;
using SkiaSharp;
using System;
using System.IO;

namespace ColorMC.Gui.Skin;

/// <summary>
/// 3D头像生成
/// </summary>
public static class Skin3DHeadA
{
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
        // Front face (Top)
        new SKPoint3(-1 * 1.125f, -1 * 1.125f,  1 * 1.125f),
        new SKPoint3( 1 * 1.125f, -1 * 1.125f,  1 * 1.125f),
        new SKPoint3( 1 * 1.125f,  1 * 1.125f,  1 * 1.125f),
        new SKPoint3(-1 * 1.125f,  1 * 1.125f,  1 * 1.125f),
        // Back face (Top)                          
        new SKPoint3(-1 * 1.125f, -1 * 1.125f, -1 * 1.125f),
        new SKPoint3( 1 * 1.125f, -1 * 1.125f, -1 * 1.125f),
        new SKPoint3( 1 * 1.125f,  1 * 1.125f, -1 * 1.125f),
        new SKPoint3(-1 * 1.125f,  1 * 1.125f, -1 * 1.125f)
    ];

    // 定义立方体索引
    private static readonly ushort[] s_cubeIndices =
    [
        8, 12, 15, 11, // Back face (Top)
        8, 12, 13, 9, // Bottom face (Top)
        8, 9, 10, 11, // Right face (Top)
        0, 4, 7, 3, // Back face
        0, 4, 5, 1, // Bottom face
        0, 1, 2, 3, // Right face
        3, 7, 6, 2, // Top face
        4, 5, 6, 7, // Left face
        1, 5, 6, 2, // Front face
        11, 15, 14, 10, // Top face (Top)
        12, 13, 14, 15, // Left face (Top)
        9, 13, 14, 10, // Front face (Top)
    ];

    // Define the colors for each face
    private static readonly SKRectI[] s_facePos =
    [
        new SKRectI(56, 8, 64, 16), // Back face (Top)
        new SKRectI(48, 0, 56, 8),  // Bottom face (Top)
        new SKRectI(48, 8, 56, 16), // Right face (Top)
        new SKRectI(24, 8, 32, 16), // Back face
        new SKRectI(16, 0, 24, 8),  // Bottom face
        new SKRectI(16, 8, 24, 16), // Right face
        new SKRectI(8, 0, 16, 8),   // Top face
        new SKRectI(0, 8, 8, 16),   // Left face
        new SKRectI(8, 8, 16, 16),  // Front face
        new SKRectI(40, 0, 48, 8),  // Top face (Top)
        new SKRectI(32, 8, 40, 16), // Left face (Top)
        new SKRectI(40, 8, 48, 16), // Front face (Top)
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
        new SKPoint(1, 0),
        new SKPoint(0, 0),
        new SKPoint(0, 1),
        new SKPoint(1, 1),
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
        // Top face
        new SKPoint(1, 0),
        new SKPoint(0, 0),
        new SKPoint(0, 1),
        new SKPoint(1, 1),
        // Left face
        new SKPoint(0, 1),
        new SKPoint(1, 1),
        new SKPoint(1, 0),
        new SKPoint(0, 0),
        // Front face
        new SKPoint(1, 1),
        new SKPoint(0, 1),
        new SKPoint(0, 0),
        new SKPoint(1, 0)
    ];

    public static Stream MakeHeadImage(SKBitmap skin)
    {
        // 创建绘图表面
        int width = 400;
        int height = 400;
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;

        // 绘制头部
        DrawHead3D(canvas, skin);

        // 保存结果到文件
        return surface.Snapshot().Encode(SKEncodedImageFormat.Png, 100).AsStream();
    }

    private static void DrawHead3D(SKCanvas canvas, SKBitmap texture)
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
        translateToOrigin.SetTranslate(-100, -100, 0);

        //// 旋转矩阵
        var rotationX = CreateRotationMatrix(30, 1, 0, 0);
        var rotationY = CreateRotationMatrix(45, 0, 1, 0);

        // 将旋转矩阵相乘
        transform.PreConcat(translateToOrigin);
        transform.PreConcat(rotationX);
        transform.PreConcat(rotationY);

        // 缩放
        var scale = SKMatrix44.CreateIdentity();
        scale.SetScale(110, -110, 110);
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

    private static void DrawTexturedFace(SKCanvas canvas, SKBitmap texture, SKMatrix44 transform, int index)
    {
        var vertices = new SKPoint[4];
        var texCoords = new SKPoint[4];
        using var sourceBitmap = new SKBitmap(8, 8);
        texture.ExtractSubset(sourceBitmap, s_facePos[index]);

        index *= 4;
        for (int j = 0; j < 4; ++j)
        {
            vertices[j] = Project(transform, s_cubeVertices[s_cubeIndices[index + j]]);
            texCoords[j] = new SKPoint(s_sourceVertices[index + j].X * sourceBitmap.Width, s_sourceVertices[index + j].Y * sourceBitmap.Height);
        }

        var skVertices = SKVertices.CreateCopy(SKVertexMode.TriangleFan, vertices, texCoords, null);

        using var paint = new SKPaint
        {
            IsAntialias = true,
            Shader = SKShader.CreateBitmap(sourceBitmap, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp)
        };

        // 绘制带纹理的面
        canvas.DrawVertices(skVertices, SKBlendMode.SrcOver, paint);
    }
}

/// <summary>
/// 3D头像生成
/// </summary>
public static class Skin3DHeadB
{
    private const int s_size = 220;
    private const int s_cubeSize = 60;
    private const int s_cubeSizeOut = s_cubeSize + (int)(s_cubeSize * 0.125);
    // Define cube vertices
    private static readonly SKPoint3[] s_cubeVertices =
    [
        //In
        new SKPoint3(-s_cubeSize, -s_cubeSize, -s_cubeSize),
        new SKPoint3(s_cubeSize, -s_cubeSize, -s_cubeSize),
        new SKPoint3(s_cubeSize, s_cubeSize, -s_cubeSize),
        new SKPoint3(-s_cubeSize, s_cubeSize, -s_cubeSize),
        new SKPoint3(-s_cubeSize, -s_cubeSize, s_cubeSize),
        new SKPoint3(s_cubeSize, -s_cubeSize, s_cubeSize),
        new SKPoint3(s_cubeSize, s_cubeSize, s_cubeSize),
        new SKPoint3(-s_cubeSize, s_cubeSize, s_cubeSize),
        //Out
        new SKPoint3(-s_cubeSizeOut, -s_cubeSizeOut, -s_cubeSizeOut),
        new SKPoint3(s_cubeSizeOut, -s_cubeSizeOut, -s_cubeSizeOut),
        new SKPoint3(s_cubeSizeOut, s_cubeSizeOut, -s_cubeSizeOut),
        new SKPoint3(-s_cubeSizeOut, s_cubeSizeOut, -s_cubeSizeOut),
        new SKPoint3(-s_cubeSizeOut, -s_cubeSizeOut, s_cubeSizeOut),
        new SKPoint3(s_cubeSizeOut, -s_cubeSizeOut, s_cubeSizeOut),
        new SKPoint3(s_cubeSizeOut, s_cubeSizeOut, s_cubeSizeOut),
        new SKPoint3(-s_cubeSizeOut, s_cubeSizeOut, s_cubeSizeOut),
    ];

    // Define the faces of the cube (each face is a list of vertex indices)
    private static readonly int[][] s_faces =
    [
        [8, 11, 15, 12],  // Back face (Top)
        [10, 11, 15, 6],  // Bottom face (Top)
        [12, 13, 14, 15], // Right face (Top)
        [0, 3, 7, 4],     // Back face
        [2, 3, 7, 6],     // Bottom face
        [4, 5, 6, 7],     // Right face
        [0, 1, 5, 4],     // Top face
        [0, 1, 2, 3],     // Left face
        [1, 2, 6, 5],     // Front face
        [8, 9, 13, 12],   // Top face (Top)
        [8, 9, 10, 11],   // Left face (Top)
        [9, 10, 14, 13],  // Front face (Top)
    ];

    // Define the colors for each face
    private static readonly SKRectI[] s_facePos =
    [
        new SKRectI(56, 8, 64, 16), // Back face (Top)
        new SKRectI(48, 0, 56, 8),  // Bottom face (Top)
        new SKRectI(48, 8, 56, 16), // Right face (Top)
        new SKRectI(24, 8, 32, 16), // Back face
        new SKRectI(16, 0, 24, 8),  // Bottom face
        new SKRectI(16, 8, 24, 16), // Right face
        new SKRectI(8, 0, 16, 8),   // Top face
        new SKRectI(0, 8, 8, 16),   // Left face
        new SKRectI(8, 8, 16, 16),  // Front face
        new SKRectI(40, 0, 48, 8),  // Top face (Top)
        new SKRectI(32, 8, 40, 16), // Left face (Top)
        new SKRectI(40, 8, 48, 16), // Front face (Top)
    ];

    // 定义原始图像的四个顶点
    private static readonly SKPoint[][] s_sourceVertices =
    [
        [
            // Back face
            new SKPoint(8, 0),
            new SKPoint(8, 8),
            new SKPoint(0, 8),
            new SKPoint(0, 0)
        ],
        [
            // Bottom face
            new SKPoint(0, 8),
            new SKPoint(0, 0),
            new SKPoint(8, 0),
            new SKPoint(8, 8)
        ],
        [
            // Right face
            new SKPoint(8, 0),
            new SKPoint(0, 0),
            new SKPoint(0, 8),
            new SKPoint(8, 8)
        ],
        [
            // Back face
            new SKPoint(8, 0),
            new SKPoint(8, 8),
            new SKPoint(0, 8),
            new SKPoint(0, 0)
        ],
        [
            // Bottom face
            new SKPoint(0, 8),
            new SKPoint(0, 0),
            new SKPoint(8, 0),
            new SKPoint(8, 8)
        ],
        [
            // Right face
            new SKPoint(8, 0),
            new SKPoint(0, 0),
            new SKPoint(0, 8),
            new SKPoint(8, 8)
        ],
        [
            // Top face
            new SKPoint(0, 0),
            new SKPoint(0, 8),
            new SKPoint(8, 8),
            new SKPoint(8, 0)
        ],
        [
            // Left face
            new SKPoint(0, 0),
            new SKPoint(8, 0),
            new SKPoint(8, 8),
            new SKPoint(0, 8)
        ],
        [
            // Front face
            new SKPoint(0, 0),
            new SKPoint(0, 8),
            new SKPoint(8, 8),
            new SKPoint(8, 0)
        ],
        [
            // Top face
            new SKPoint(0, 0),
            new SKPoint(0, 8),
            new SKPoint(8, 8),
            new SKPoint(8, 0)
        ],
        [
            // Left face
            new SKPoint(0, 0),
            new SKPoint(8, 0),
            new SKPoint(8, 8),
            new SKPoint(0, 8)
        ],
        [
            // Front face
            new SKPoint(0, 0),
            new SKPoint(0, 8),
            new SKPoint(8, 8),
            new SKPoint(8, 0)
        ]
    ];

    public static Stream MakeHeadImage(SKBitmap skin)
    {
        // 创建画布
        var info = new SKImageInfo(s_size, s_size);
        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        // 绘制头部
        DrawHead3D(canvas, skin);

        // 保存结果到文件
        return surface.Snapshot()
            .Encode(SKEncodedImageFormat.Png, 100).AsStream();
    }

    // Translate to center
    private static SKPoint TranslateToCenter(SKPoint3 point3)
    {
        return new SKPoint(point3.X + s_size / 2, point3.Y + s_size / 2);
    }

    private static void DrawHead3D(SKCanvas canvas, SKBitmap skin)
    {
        var config = GuiConfigUtils.Config.Head;

        // Define transformation matrices
        using var rotationX = SKMatrix44.CreateRotationDegrees(1, 0, 0, config.X);
        using var rotationY = SKMatrix44.CreateRotationDegrees(0, 1, 0, config.Y);
        using var pos = SKMatrix44.CreateTranslation(0, -6f, 5f);
        using var rotation = SKMatrix44.CreateIdentity();
        rotation.PreConcat(rotationX);
        rotation.PreConcat(rotationY);
        rotation.PostConcat(pos);

        var sKPoints = new SKPoint3[s_cubeVertices.Length];

        // Apply transformations
        for (int i = 0; i < s_cubeVertices.Length; i++)
        {
            float[] vector = [s_cubeVertices[i].X, s_cubeVertices[i].Y, s_cubeVertices[i].Z, 1];
            var res = rotation.MapScalars(vector);
            sKPoints[i] = new SKPoint3(res[0], res[1], res[2]);
        }

        // Apply perspective manually
        float perspectiveZ = 0.001f;
        for (int i = 0; i < sKPoints.Length; i++)
        {
            float z = sKPoints[i].Z * perspectiveZ + 1;
            sKPoints[i].X = sKPoints[i].X / z;
            sKPoints[i].Y = sKPoints[i].Y / z;
        }

        {
            // Define the shadow offset and blur radius
            float offsetX = 0;
            float offsetY = 0;
            float blurRadius = 20;

            var face = s_faces[1];

            // 将3D顶点投影到2D平面上
            var projectedVertices = new SKPoint[]
            {
                TranslateToCenter(sKPoints[face[0]]),
                TranslateToCenter(sKPoints[face[1]]),
                TranslateToCenter(sKPoints[face[2]]),
                TranslateToCenter(sKPoints[face[3]])
            };

            // Define the ellipse
            var ellipseRect = new SKRect(20, 140, 200, 180);

            // Create the paint for the shadow
            using var shadowPaint = new SKPaint();
            shadowPaint.IsAntialias = true;
            shadowPaint.Color = new SKColor(0, 0, 0, 128); // Semi-transparent black for shadow
            shadowPaint.ImageFilter = SKImageFilter.CreateBlur(blurRadius, blurRadius);

            // Create a shadow ellipse rect
            SKRect shadowRect = ellipseRect;
            shadowRect.Offset(offsetX, offsetY);

            // Draw the shadow (an ellipse)
            canvas.DrawOval(shadowRect, shadowPaint);
        }

        for (int index = 0; index < s_faces.Length; index++)
        {
            var face = s_faces[index];

            // 将3D顶点投影到2D平面上
            var projectedVertices = new SKPoint[]
            {
                TranslateToCenter(sKPoints[face[0]]),
                TranslateToCenter(sKPoints[face[1]]),
                TranslateToCenter(sKPoints[face[2]]),
                TranslateToCenter(sKPoints[face[3]])
            };

            using var sourceBitmap = new SKBitmap(8, 8);
            skin.ExtractSubset(sourceBitmap, s_facePos[index]);

            // 计算仿射变换矩阵
            using var matrix = CalculateAffineTransform(s_sourceVertices[index], projectedVertices);

            // 设置变换矩阵
            canvas.SetMatrix(matrix.Matrix);

            using var paint = new SKPaint
            {
                IsAntialias = false
            };

            // 绘制变形的图像
            canvas.DrawBitmap(sourceBitmap, 0, 0, paint);
        }
    }

    private static SKMatrix44 CalculateAffineTransform(SKPoint[] src, SKPoint[] dst)
    {
        // 计算透视变换矩阵
        float[,] matrix = new float[8, 9];

        for (int i = 0; i < 4; i++)
        {
            int row = i * 2;
            matrix[row, 0] = src[i].X;
            matrix[row, 1] = src[i].Y;
            matrix[row, 2] = 1;
            matrix[row, 3] = 0;
            matrix[row, 4] = 0;
            matrix[row, 5] = 0;
            matrix[row, 6] = -src[i].X * dst[i].X;
            matrix[row, 7] = -src[i].Y * dst[i].X;
            matrix[row, 8] = dst[i].X;

            row++;
            matrix[row, 0] = 0;
            matrix[row, 1] = 0;
            matrix[row, 2] = 0;
            matrix[row, 3] = src[i].X;
            matrix[row, 4] = src[i].Y;
            matrix[row, 5] = 1;
            matrix[row, 6] = -src[i].X * dst[i].Y;
            matrix[row, 7] = -src[i].Y * dst[i].Y;
            matrix[row, 8] = dst[i].Y;
        }

        // 高斯消元法求解
        for (int i = 0; i < 8; i++)
        {
            // 选主元
            int maxRow = i;
            for (int k = i + 1; k < 8; k++)
            {
                if (Math.Abs(matrix[k, i]) > Math.Abs(matrix[maxRow, i]))
                {
                    maxRow = k;
                }
            }

            // 交换行
            for (int k = i; k < 9; k++)
            {
                (matrix[i, k], matrix[maxRow, k]) = (matrix[maxRow, k], matrix[i, k]);
            }

            // 归一化主元
            float pivot = matrix[i, i];
            for (int k = i; k < 9; k++)
            {
                matrix[i, k] /= pivot;
            }

            // 消去列
            for (int k = 0; k < 8; k++)
            {
                if (k != i)
                {
                    float factor = matrix[k, i];
                    for (int j = i; j < 9; j++)
                    {
                        matrix[k, j] -= factor * matrix[i, j];
                    }
                }
            }
        }

        // 提取结果
        float[] h = new float[9];
        for (int i = 0; i < 8; i++)
        {
            h[i] = matrix[i, 8];
        }
        h[8] = 1;

        var m44 = new SKMatrix44();
        m44[0, 0] = h[0];
        m44[0, 1] = h[1];
        m44[0, 2] = 0;
        m44[0, 3] = h[2];
        m44[1, 0] = h[3];
        m44[1, 1] = h[4];
        m44[1, 2] = 0;
        m44[1, 3] = h[5];
        m44[2, 0] = 0;
        m44[2, 1] = 0;
        m44[2, 2] = 1;
        m44[2, 3] = 0;
        m44[3, 0] = h[6];
        m44[3, 1] = h[7];
        m44[3, 2] = 0;
        m44[3, 3] = h[8];

        return m44;
    }
}
