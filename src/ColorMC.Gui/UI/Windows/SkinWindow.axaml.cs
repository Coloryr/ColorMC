using Avalonia.Controls;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using static Avalonia.OpenGL.GlConsts;
using System.Runtime.InteropServices;
using System.Numerics;
using System.IO;
using System.Linq;
using System;
using System.Diagnostics;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using Image = SixLabors.ImageSharp.Image;
using ColorMC.Gui.Skin;
using ColorMC.Gui.Skin.Model;
using AvaloniaEdit;
using SixLabors.ImageSharp.Processing;
using OpenTK.Audio.OpenAL;

namespace ColorMC.Gui.UI.Windows;

public partial class SkinWindow : Window
{
    public SkinWindow()
    {
        InitializeComponent();
    }
}

public class OpenGlPageControl : OpenGlControlBase
{
    private float _yaw;

    public static readonly DirectProperty<OpenGlPageControl, float> YawProperty =
        AvaloniaProperty.RegisterDirect<OpenGlPageControl, float>("Yaw", o => o.Yaw, (o, v) => o.Yaw = v);

    public float Yaw
    {
        get => _yaw;
        set => SetAndRaise(YawProperty, ref _yaw, value);
    }

    private float _pitch;

    public static readonly DirectProperty<OpenGlPageControl, float> PitchProperty =
        AvaloniaProperty.RegisterDirect<OpenGlPageControl, float>("Pitch", o => o.Pitch, (o, v) => o.Pitch = v);

    public float Pitch
    {
        get => _pitch;
        set => SetAndRaise(PitchProperty, ref _pitch, value);
    }


    private float _roll;

    public static readonly DirectProperty<OpenGlPageControl, float> RollProperty =
        AvaloniaProperty.RegisterDirect<OpenGlPageControl, float>("Roll", o => o.Roll, (o, v) => o.Roll = v);

    public float Roll
    {
        get => _roll;
        set => SetAndRaise(RollProperty, ref _roll, value);
    }

    private string _info = string.Empty;

    public static readonly DirectProperty<OpenGlPageControl, string> InfoProperty =
        AvaloniaProperty.RegisterDirect<OpenGlPageControl, string>("Info", o => o.Info, (o, v) => o.Info = v);

    public string Info
    {
        get => _info;
        private set => SetAndRaise(InfoProperty, ref _info, value);
    }

    private int _vertexShader;
    private int _fragmentShader;
    private int _shaderProgram;
    private int _vertexBufferObject;
    private int _indexBufferObject;
    private int _vertexArrayObject;

    private string GetShader(bool fragment, string shader)
    {
        var version = (GlVersion.Type == GlProfileType.OpenGL ?
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? 150 : 120 :
            100);
        var data = "#version " + version + "\n";
        if (GlVersion.Type == GlProfileType.OpenGLES)
            data += "precision mediump float;\n";
        if (version >= 150)
        {
            shader = shader.Replace("attribute", "in");
            if (fragment)
                shader = shader
                    .Replace("varying", "in")
                    .Replace("//DECLAREGLFRAG", "out vec4 outFragColor;")
                    .Replace("gl_FragColor", "outFragColor");
            else
                shader = shader.Replace("varying", "out");
        }

        data += shader;

        return data;
    }

    private string VertexShaderSource => GetShader(false, @"
        attribute vec3 a_Position;
        attribute vec2 a_texCoord;
        uniform mat4 uModel;
        uniform mat4 uProjection;
        uniform mat4 uView;
        varying vec2 v_texCoord;
        void main()
        {
            v_texCoord = a_texCoord;
            
            gl_Position = uProjection * uView * uModel * vec4(a_Position, 1.0);
        }
");

    private string FragmentShaderSource => GetShader(true, @"
        varying vec2 v_texCoord;
        uniform sampler2D texture0;
        void main()
        {
            gl_FragColor = texture2D(texture0, v_texCoord);
        }
");

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct Vertex
    {
        public Vector3 Position;
        public Vector2 UV;
    }

    private readonly Vertex[] points;
    private Image<Rgba32> _Image;
    private int texture;

    private ModelSourceTextureType steveModelType;
    private ushort[] steveModelDrawOrder;

    public OpenGlPageControl()
    {
        _Image = Image.Load<Rgba32>("C:\\Users\\40206\\Desktop\\test.png");

        steveModelType = SkinUtil.GetTextType(_Image);
        var steve = SteveC.GetSteve(steveModelType);
        var steveModelCoords = steve.Item1;
        steveModelDrawOrder = steve.Item2;
        var steveTextureCoords = Steve3DTexture.GetSteveTexture(steveModelType);

        points = new Vertex[steveModelCoords.Length / 3];

        for (var primitive = 0; primitive < steveModelCoords.Length / 3; primitive++)
        {
            var srci = primitive * 3;
            var srci1 = primitive * 2;
            points[primitive] = new Vertex
            {
                Position = new Vector3(steveModelCoords[srci], steveModelCoords[srci + 1], steveModelCoords[srci + 2]),
                UV = new Vector2(steveTextureCoords[srci1], steveTextureCoords[srci1 + 1])
            };
        }
    }

    private static void CheckError(GlInterface gl)
    {
        int err;
        while ((err = gl.GetError()) != GL_NO_ERROR)
            Console.WriteLine(err);
    }

    protected override unsafe void OnOpenGlInit(GlInterface GL, int fb)
    {
        CheckError(GL);

        Info = $"Renderer: {GL.GetString(GL_RENDERER)} Version: {GL.GetString(GL_VERSION)}";

        // Load the source of the vertex shader and compile it.
        _vertexShader = GL.CreateShader(GL_VERTEX_SHADER);
        var error = GL.CompileShaderAndGetError(_vertexShader, VertexShaderSource);
        Console.WriteLine(error);

        // Load the source of the fragment shader and compile it.
        _fragmentShader = GL.CreateShader(GL_FRAGMENT_SHADER);
        error = GL.CompileShaderAndGetError(_fragmentShader, FragmentShaderSource);
        Console.WriteLine(error);

        // Create the shader program, attach the vertex and fragment shaders and link the program.
        _shaderProgram = GL.CreateProgram();
        GL.AttachShader(_shaderProgram, _vertexShader);
        GL.AttachShader(_shaderProgram, _fragmentShader);
        error = GL.LinkProgramAndGetError(_shaderProgram);
        Console.WriteLine(error);
        CheckError(GL);

        _vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(GL_ARRAY_BUFFER, _vertexBufferObject);
        var vertexSize = Marshal.SizeOf<Vertex>();
        fixed (void* pdata = points)
            GL.BufferData(GL_ARRAY_BUFFER, new IntPtr(points.Length * vertexSize),
                new IntPtr(pdata), GL_STATIC_DRAW);

        _indexBufferObject = GL.GenBuffer();
        GL.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, _indexBufferObject);
        fixed (void* pdata = steveModelDrawOrder)
            GL.BufferData(GL_ELEMENT_ARRAY_BUFFER, new IntPtr(steveModelDrawOrder.Length * sizeof(ushort)), new IntPtr(pdata), GL_STATIC_DRAW);
        CheckError(GL);

        _vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArrayObject);
        CheckError(GL);

        GL.UseProgram(_shaderProgram);

        int positionLocation =  GL.GetAttribLocationString(_shaderProgram, "a_Position");
        int uv = GL.GetAttribLocationString(_shaderProgram, "a_texCoord");
        GL.VertexAttribPointer(positionLocation, 3, GL_FLOAT,
            0, 5 * sizeof(float), 0);
        GL.VertexAttribPointer(uv, 2, GL_FLOAT,
            0, 5 * sizeof(float), 3 * sizeof(float));

        GL.EnableVertexAttribArray(positionLocation);
        GL.EnableVertexAttribArray(uv);
        CheckError(GL);

        texture = GL.GenTexture();
        GL.ActiveTexture(GL_TEXTURE0);
        GL.BindTexture(GL_TEXTURE_2D, texture);

        GL.TexParameteri(
            GL_TEXTURE_2D,
            GL_TEXTURE_MIN_FILTER, GL_LINEAR
        );
        GL.TexParameteri(
            GL_TEXTURE_2D,
            GL_TEXTURE_MAG_FILTER, GL_NEAREST
        );
        //TextureWrapS ClampToEdge
        GL.TexParameteri(
            GL_TEXTURE_2D,
            10242, 33071
        );
        //TextureWrapT ClampToEdge
        GL.TexParameteri(
            GL_TEXTURE_2D,
            10243, 33071
        );

        var pixels = new byte[4 * _Image.Width * _Image.Height];
        _Image.CopyPixelDataTo(pixels);

        fixed (void* pdata = pixels)
            GL.TexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, _Image.Width, _Image.Height, 0, GL_RGBA, GL_UNSIGNED_BYTE, new IntPtr(pdata));

        CheckError(GL);
    }

    protected override void OnOpenGlDeinit(GlInterface GL, int fb)
    {
        // Unbind everything
        GL.BindBuffer(GL_ARRAY_BUFFER, 0);
        GL.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        GL.BindVertexArray(0);
        GL.UseProgram(0);

        // Delete all resources.
        GL.DeleteBuffer(_vertexBufferObject);
        GL.DeleteBuffer(_indexBufferObject);
        GL.DeleteVertexArray(_vertexArrayObject);
        GL.DeleteProgram(_shaderProgram);
        GL.DeleteShader(_fragmentShader);
        GL.DeleteShader(_vertexShader);
    }
    protected override unsafe void OnOpenGlRender(GlInterface gl, int fb)
    {
        gl.ClearColor(0, 0, 0, 0);
        gl.Clear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
        gl.Enable(GL_DEPTH_TEST);
        gl.Viewport(0, 0, (int)Bounds.Width, (int)Bounds.Height);
        var GL = gl;

        GL.ActiveTexture(GL_TEXTURE0);
        GL.BindTexture(GL_TEXTURE_2D, texture);
        GL.BindBuffer(GL_ARRAY_BUFFER, _vertexBufferObject);
        GL.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, _indexBufferObject);
        GL.BindVertexArray(_vertexArrayObject);
        GL.UseProgram(_shaderProgram);
        CheckError(GL);

        var projection =
            Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), (float)(Bounds.Width / Bounds.Height),
                0.01f, 1000);

        var view = Matrix4x4.CreateLookAt(new Vector3(0, 0, 6), new Vector3(), new Vector3(0, 1, 0));
        var model = Matrix4x4.CreateFromYawPitchRoll(_yaw, _pitch, _roll);
        var modelLoc = GL.GetUniformLocationString(_shaderProgram, "uModel");
        var viewLoc = GL.GetUniformLocationString(_shaderProgram, "uView");
        var projectionLoc = GL.GetUniformLocationString(_shaderProgram, "uProjection");
        GL.UniformMatrix4fv(modelLoc, 1, false, &model);
        GL.UniformMatrix4fv(viewLoc, 1, false, &view);
        GL.UniformMatrix4fv(projectionLoc, 1, false, &projection);

        CheckError(GL);
        GL.DrawElements(GL_TRIANGLES, steveModelDrawOrder.Length, GL_UNSIGNED_SHORT, IntPtr.Zero);

        CheckError(GL);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == YawProperty || change.Property == RollProperty || change.Property == PitchProperty)
            InvalidateVisual();
        base.OnPropertyChanged(change);
    }
}
