using System;
using System.IO;
using SkiaSharp;

namespace ColorMC.Gui.Skin;

public static class Skin3DHead
{
    private const int Size = 220;
    private const int CubeSize = 60;
    private const int CubeSizeOut = CubeSize + (int)(CubeSize * 0.125);
    // Define cube vertices
    private static readonly SKPoint3[] cubeVertices =
    [
        //In
        new SKPoint3(-CubeSize, -CubeSize, -CubeSize),
        new SKPoint3(CubeSize, -CubeSize, -CubeSize),
        new SKPoint3(CubeSize, CubeSize, -CubeSize),
        new SKPoint3(-CubeSize, CubeSize, -CubeSize),
        new SKPoint3(-CubeSize, -CubeSize, CubeSize),
        new SKPoint3(CubeSize, -CubeSize, CubeSize),
        new SKPoint3(CubeSize, CubeSize, CubeSize),
        new SKPoint3(-CubeSize, CubeSize, CubeSize),
        //Out
        new SKPoint3(-CubeSizeOut, -CubeSizeOut, -CubeSizeOut),
        new SKPoint3(CubeSizeOut, -CubeSizeOut, -CubeSizeOut),
        new SKPoint3(CubeSizeOut, CubeSizeOut, -CubeSizeOut),
        new SKPoint3(-CubeSizeOut, CubeSizeOut, -CubeSizeOut),
        new SKPoint3(-CubeSizeOut, -CubeSizeOut, CubeSizeOut),
        new SKPoint3(CubeSizeOut, -CubeSizeOut, CubeSizeOut),
        new SKPoint3(CubeSizeOut, CubeSizeOut, CubeSizeOut),
        new SKPoint3(-CubeSizeOut, CubeSizeOut, CubeSizeOut),
    ];

    // Define the faces of the cube (each face is a list of vertex indices)
    private static readonly int[][] faces =
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
    private static readonly SKRectI[] facePos =
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
    private static readonly SKPoint[][] sourceVertices =
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
        var info = new SKImageInfo(Size, Size);
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
        return new SKPoint(point3.X + Size / 2, point3.Y + Size / 2);
    }

    private static void DrawHead3D(SKCanvas canvas, SKBitmap skin)
    {
        // Define transformation matrices
        using var rotationX = SKMatrix44.CreateRotationDegrees(1, 0, 0, 20);
        using var rotationY = SKMatrix44.CreateRotationDegrees(0, 1, 0, 50);
        using var pos = SKMatrix44.CreateTranslation(0, -6f, 5f);
        using var rotation = SKMatrix44.CreateIdentity();
        rotation.PreConcat(rotationX);
        rotation.PreConcat(rotationY);
        rotation.PostConcat(pos);

        var sKPoints = new SKPoint3[cubeVertices.Length];

        // Apply transformations
        for (int i = 0; i < cubeVertices.Length; i++)
        {
            float[] vector = [cubeVertices[i].X, cubeVertices[i].Y, cubeVertices[i].Z, 1];
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

        for (int index = 0; index < faces.Length; index++)
        {
            var face = faces[index];

            // 将3D顶点投影到2D平面上
            var projectedVertices = new SKPoint[]
            {
                TranslateToCenter(sKPoints[face[0]]),
                TranslateToCenter(sKPoints[face[1]]),
                TranslateToCenter(sKPoints[face[2]]),
                TranslateToCenter(sKPoints[face[3]])
            };

            using var sourceBitmap = new SKBitmap(8, 8);
            skin.ExtractSubset(sourceBitmap, facePos[index]);

            // 计算仿射变换矩阵
            using var matrix = CalculateAffineTransform(sourceVertices[index], projectedVertices);

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
