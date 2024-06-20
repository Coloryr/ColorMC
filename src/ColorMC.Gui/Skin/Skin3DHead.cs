using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace ColorMC.Gui.Skin;

public static class Skin3DHead
{
    private const int Size = 200;
    private const int CubeSize = 60;
    // Define cube vertices
    private static readonly SKPoint3[] cubeVertices =
    [
        new SKPoint3(-CubeSize, -CubeSize, -CubeSize),
        new SKPoint3(CubeSize, -CubeSize, -CubeSize),
        new SKPoint3(CubeSize, CubeSize, -CubeSize),
        new SKPoint3(-CubeSize, CubeSize, -CubeSize),
        new SKPoint3(-CubeSize, -CubeSize, CubeSize),
        new SKPoint3(CubeSize, -CubeSize, CubeSize),
        new SKPoint3(CubeSize, CubeSize, CubeSize),
        new SKPoint3(-CubeSize, CubeSize, CubeSize),
    ];

    // Define the faces of the cube (each face is a list of vertex indices)
    private static readonly int[][] faces =
    [
        [0, 1, 2, 3], // Front face
        [4, 5, 6, 7], // Back face
        [0, 1, 5, 4], // Bottom face
        [2, 3, 7, 6], // Top face
        [0, 3, 7, 4], // Left face
        [1, 2, 6, 5]  // Right face
    ];

    // Define the colors for each face
    private static readonly SKRectI[] facePos =
    [
        new SKRectI(8, 8, 16, 16),
        new SKRectI(24, 8, 32, 16),
        new SKRectI(16, 0, 24, 8),
        new SKRectI(8, 0, 16, 8),
        new SKRectI(0, 8, 8, 16),
        new SKRectI(16, 8, 26, 16),
    ];

    public static SKImage? Draw(string pic)
    {
        // 加载皮肤纹理
        using var skin = LoadSkin(pic); // 替换为皮肤文件路径
        if (skin == null)
        {
            Console.WriteLine("Failed to load skin file.");
            return null;
        }

        // 创建画布
        var info = new SKImageInfo(Size, Size);
        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);

        // 绘制头部
        DrawHead3D(canvas, skin);

        // 保存结果到文件
        var image = surface.Snapshot();
        return image;
    }

    // Translate to center
    private static SKPoint TranslateToCenter(SKPoint3 point3)
    {
        return new SKPoint(point3.X + Size / 2, point3.Y + Size / 2);
    }

    private static SKBitmap LoadSkin(string filePath)
    {
        return SKBitmap.Decode(filePath);
    }

    private static void DrawHead3D(SKCanvas canvas, SKBitmap skin)
    {
        canvas.Clear(SKColors.White);

        // Define transformation matrices
        var rotationX = SKMatrix44.CreateRotationDegrees(1, 0, 0, 30);
        var rotationY = SKMatrix44.CreateRotationDegrees(0, 1, 0, 50);
        var rotation = SKMatrix44.CreateIdentity();
        rotation.PreConcat(rotationX);
        rotation.PreConcat(rotationY);

        // Apply transformations
        for (int i = 0; i < cubeVertices.Length; i++)
        {
            float[] vector = [cubeVertices[i].X, cubeVertices[i].Y, cubeVertices[i].Z, 1];
            var res = rotation.MapScalars(vector);
            cubeVertices[i] = new SKPoint3(res[0], res[1], res[2]);
        }

        // Apply perspective manually
        float perspectiveZ = 0.001f;
        for (int i = 0; i < cubeVertices.Length; i++)
        {
            float z = cubeVertices[i].Z * perspectiveZ + 1;
            cubeVertices[i] = new SKPoint3(cubeVertices[i].X / z, cubeVertices[i].Y / z, cubeVertices[i].Z);
        }

        //for (int index = 0; index < faces.Length; index++)
        //{
        int index = 0;
        var face = faces[index];
        var path = new SKPath();
        path.MoveTo(TranslateToCenter(cubeVertices[face[0]]));
        path.LineTo(TranslateToCenter(cubeVertices[face[1]]));
        path.LineTo(TranslateToCenter(cubeVertices[face[2]]));
        path.LineTo(TranslateToCenter(cubeVertices[face[3]]));
        path.Close();

        using var croppedBitmap = new SKBitmap(8, 8);
        skin.ExtractSubset(croppedBitmap, facePos[index]);
        using (var output = File.OpenWrite("face.png"))
        using (var image = SKImage.FromBitmap(croppedBitmap))
        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
        {
            data.SaveTo(output);
        }
        using var paint = new SKPaint
        {
            Shader = SKShader.CreateBitmap(croppedBitmap, SKShaderTileMode.Mirror, SKShaderTileMode.Mirror),
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        canvas.DrawPath(path, paint);
        //}
    }
}
