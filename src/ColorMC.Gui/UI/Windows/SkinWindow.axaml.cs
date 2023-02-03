using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.Utilities;
using ColorMC.Core.Game.Auth;
using ColorMC.Gui.Skin;
using ColorMC.Gui.Skin.Model;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils.LaunchSetting;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Runtime.InteropServices;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace ColorMC.Gui.UI.Windows;

public partial class SkinWindow : Window
{
    public SkinWindow()
    {
        InitializeComponent();

        Head.SetWindow(this);

        FontFamily = Program.Font;

        ComboBox1.Items = UserBinding.GetSkinType();

        Skin.SetWindow(this);

        Text1.Text = Skin.Info;

        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;
        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        Button3.Click += Button3_Click;

        Opened += SkinWindow_Opened;
        Closed += SkinWindow_Closed;

        App.PicUpdate += Update;
        App.SkinLoad += App_SkinLoad;

        Check();
        Update();
    }

    private void SkinWindow_Opened(object? sender, EventArgs e)
    {
        var pos = Skin.GetXY();
        var temp = Matrix.CreateRotation(Math.PI);//* Matrix.CreateTranslation(new(0, pos.Y / 2));
        Skin.RenderTransform = new ImmutableTransform(temp);
    }

    private void App_SkinLoad()
    {
        Check();

        Skin.ChangeSkin();
    }

    private void Button3_Click(object? sender, RoutedEventArgs e)
    {
        UserBinding.EditSkin();
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        Skin.Reset();
    }

    private void Check()
    {
        if (UserBinding.GetLastUser()?.AuthType != AuthType.Offline)
        {
            Button3.IsEnabled = true;
        }
        else
        {
            Button3.IsEnabled = false;
        }
    }

    private void SkinWindow_Closed(object? sender, EventArgs e)
    {
        App.PicUpdate -= Update;

        App.SkinWindow = null;

        Skin.Close();
    }

    private void ComboBox1_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ComboBox1.SelectedIndex == (int)Skin.steveModelType)
            return;
        if (ComboBox1.SelectedIndex == (int)SkinType.UNKNOWN)
        {
            Info.Show(Localizer.Instance["SkinWindow.Info1"]);
            ComboBox1.SelectedIndex = (int)Skin.steveModelType;
            return;
        }
        Skin.ChangeType(ComboBox1.SelectedIndex);
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        Skin.ChangeSkin();
    }

    public void Update()
    {
        App.Update(this, Image_Back, Grid1);

        Skin.InvalidateVisual();
    }
}

public class SkinRender : Control
{
    private SkinWindow Window;
    private Win Window1;

    class Win : GameWindow
    {
        public Win() : base(new(), new()
        {
            StartVisible = false
        })
        {
            
        }
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

    private bool HaveSkin = false;
    private bool SwitchModel = false;
    private bool SwitchSkin = false;

    private float Dis = 7;
    private Vector2 RotXY;
    private Vector2 DiffXY;

    private Vector2 XY;
    private Vector2 SaveXY;
    private Vector2 LastXY;

    private int texture;
    private int texture1;

    public SkinType steveModelType;
    private int steveModelDrawOrder;
    private int steveModelDrawOrderTop;

    private int _vertexShader;
    private int _fragmentShader;
    private int _shaderProgram;

    private int _vertexBufferObject;
    private int _indexBufferObject;
    private int _vertexArrayObject;

    private int _vertexBufferObjectTop;
    private int _indexBufferObjectTop;
    private int _vertexArrayObjectTop;

    private Vector2 LastSize;
    private WriteableBitmap bitmap;

    public string Info;

    public SkinRender()
    {
        Window1 = new();

        Init();
    }

    public void SetWindow(SkinWindow window)
    {
        Window = window;
    }

    public void Init()
    {
        GL.ClearColor(0, 0, 0, 1);

        //GL_BLEND
        GL.Enable(EnableCap.Blend);

        //GL.SRC_ALPHA, GL.ONE_MINUS_SRC_ALPHA
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        //GL.GL_BACK
        GL.CullFace(CullFaceMode.Back);

        //int[] AmbientLight = { 1, 1, 1, 1 };

        //fixed (void* pdata = AmbientLight)
        //    GLLightfv(16384, 4608, new IntPtr(pdata));
        //GL.Enable(16384);       //开启GL_LIGHT0光源
        //GL.Enable(2896);     //开启光照系统
        //GL.Enable(GL_DEPTH_TEST);

        CheckError();

        Info = $"Renderer: {GL.GetString(StringName.Renderer)} Version: {GL.GetString(StringName.Version)}";

        _vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(_vertexShader, VertexShaderSource);
        CompileShader(_vertexShader);

        _fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(_fragmentShader, FragmentShaderSource);
        CompileShader(_vertexShader);

        _shaderProgram = GL.CreateProgram();
        GL.AttachShader(_shaderProgram, _vertexShader);
        GL.AttachShader(_shaderProgram, _fragmentShader);
        LinkProgram(_shaderProgram);

        int[] temp2 = new int[3];

        GL.GenVertexArrays(3, temp2);

        _vertexArrayObject = temp2[0];
        _vertexArrayObjectTop = temp2[1];

        _vertexBufferObject = GL.GenBuffer();
        _indexBufferObject = GL.GenBuffer();

        _vertexBufferObjectTop = GL.GenBuffer();
        _indexBufferObjectTop = GL.GenBuffer();

        CheckError();

        PointerWheelChanged += OpenGlPageControl_PointerWheelChanged;
        PointerPressed += OpenGlPageControl_PointerPressed;
        PointerReleased += OpenGlPageControl_PointerReleased;
        PointerMoved += OpenGlPageControl_PointerMoved;

        LoadSkin();
    }

    public void ChangeType(int index)
    {
        steveModelType = (SkinType)index;

        SwitchModel = true;

        InvalidateVisual();
    }

    public void ChangeSkin()
    {
        SwitchSkin = true;

        InvalidateVisual();
    }

    public void Reset()
    {
        Dis = 7;
        RotXY.X = 0;
        RotXY.Y = 0;
        DiffXY.X = 0;
        DiffXY.Y = 0;
        XY.X = 0;
        XY.Y = 0;
        SaveXY.X = 0;
        SaveXY.Y = 0;
        LastXY.X = 0;
        LastXY.Y = 0;

        InvalidateVisual();
    }

    private unsafe void LoadSkin()
    {
        if (UserBinding.SkinImage == null)
        {
            HaveSkin = false;
            Dispatcher.UIThread.Post(() =>
            {
                Window.Label1.IsVisible = true;
                Window.Label1.Content = Localizer.Instance["SkinWindow.Info2"];
            });
            return;
        }

        steveModelType = SkinUtil.GetTextType(UserBinding.SkinImage);
        if (steveModelType == SkinType.UNKNOWN)
        {
            HaveSkin = false;
            Dispatcher.UIThread.Post(() =>
            {
                Window.Label1.IsVisible = true;
                Window.Label1.Content = Localizer.Instance["SkinWindow.Info3"];
            });
            return;
        }

        texture = GL.GenTexture();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, texture);

        GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear
        );
        GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest
        );
        GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge
        );
        GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge
        );

        var pixels = new byte[4 * UserBinding.SkinImage.Width * UserBinding.SkinImage.Height];
        UserBinding.SkinImage.CopyPixelDataTo(pixels);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, UserBinding.SkinImage.Width, UserBinding.SkinImage.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

        CheckError();

        LoadModel();

        HaveSkin = true;
        Dispatcher.UIThread.Post(() =>
        {
            Window.ComboBox1.SelectedIndex = (int)steveModelType;
            Window.Label1.IsVisible = false;
        });
    }

    private unsafe void LoadModel()
    {
        {
            GL.UseProgram(_shaderProgram);
            GL.BindVertexArray(_vertexArrayObject);

            int a_Position = GL.GetAttribLocation(_shaderProgram, "a_Position");
            int a_texCoord = GL.GetAttribLocation(_shaderProgram, "a_texCoord");

            GL.DisableVertexAttribArray((uint)a_Position);
            GL.DisableVertexAttribArray((uint)a_texCoord);

            var steve = SteveC.GetSteve(steveModelType);
            var model = steve.Item1;
            var draw = steve.Item2;
            var uv = Steve3DTexture.GetSteveTexture(steveModelType);

            var points = new Vertex[model.Length / 3];

            for (var primitive = 0; primitive < model.Length / 3; primitive++)
            {
                var srci = primitive * 3;
                var srci1 = primitive * 2;
                points[primitive] = new Vertex
                {
                    Position = new(model[srci], model[srci + 1], model[srci + 2]),
                    UV = new(uv[srci1], uv[srci1 + 1])
                };
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            var vertexSize = Marshal.SizeOf<Vertex>();
            fixed (void* pdata = points)
                GL.BufferData(BufferTarget.ArrayBuffer, points.Length * vertexSize,
                    new IntPtr(pdata), BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBufferObject);
            fixed (void* pdata = draw)
                GL.BufferData(BufferTarget.ElementArrayBuffer, draw.Length * sizeof(ushort), new IntPtr(pdata), BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer((uint)a_Position, 3, VertexAttribPointerType.Float,
                false, 5 * sizeof(float), 0);
            GL.VertexAttribPointer((uint)a_texCoord, 2, VertexAttribPointerType.Float,
                false, 5 * sizeof(float), 3 * sizeof(float));

            GL.EnableVertexAttribArray((uint)a_Position);
            GL.EnableVertexAttribArray((uint)a_texCoord);

            GL.BindVertexArray(0);

            CheckError();

            steveModelDrawOrder = draw.Length;
        }

        {
            GL.UseProgram(_shaderProgram);
            GL.BindVertexArray(_vertexArrayObjectTop);

            int a_Position = GL.GetAttribLocation(_shaderProgram, "a_Position");
            int a_texCoord = GL.GetAttribLocation(_shaderProgram, "a_texCoord");

            GL.DisableVertexAttribArray((uint)a_Position);
            GL.DisableVertexAttribArray((uint)a_texCoord);

            var steve = SteveC.GetSteveTop(steveModelType);
            var model = steve.Item1;
            var draw = steve.Item2;
            var uv = Steve3DTexture.GetSteveTextureTop(steveModelType);

            var points = new Vertex[model.Length / 3];

            for (var primitive = 0; primitive < model.Length / 3; primitive++)
            {
                var srci = primitive * 3;
                var srci1 = primitive * 2;
                points[primitive] = new Vertex
                {
                    Position = new(model[srci], model[srci + 1], model[srci + 2]),
                    UV = new(uv[srci1], uv[srci1 + 1])
                };
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObjectTop);
            var vertexSize = Marshal.SizeOf<Vertex>();
            fixed (void* pdata = points)
                GL.BufferData(BufferTarget.ArrayBuffer, points.Length * vertexSize,
                    new IntPtr(pdata), BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBufferObjectTop);
            fixed (void* pdata = draw)
                GL.BufferData(BufferTarget.ElementArrayBuffer, draw.Length * sizeof(ushort), new IntPtr(pdata), BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer((uint)a_Position, 3, VertexAttribPointerType.Float,
                false, 5 * sizeof(float), 0);
            GL.VertexAttribPointer((uint)a_texCoord, 2, VertexAttribPointerType.Float,
                false, 5 * sizeof(float), 3 * sizeof(float));

            GL.EnableVertexAttribArray((uint)a_Position);
            GL.EnableVertexAttribArray((uint)a_texCoord);

            GL.BindVertexArray(0);

            CheckError();

            steveModelDrawOrderTop = draw.Length;
        }
    }

    private void OpenGlPageControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!HaveSkin)
            return;

        var po = e.GetCurrentPoint(this);
        var pos = e.GetPosition(this);

        if (po.Properties.IsLeftButtonPressed)
        {
            DiffXY.X = (float)pos.X - RotXY.Y;
            DiffXY.Y = -(float)pos.Y + RotXY.X;
        }
        else if (po.Properties.IsRightButtonPressed)
        {
            LastXY.X = (float)pos.X;
            LastXY.Y = (float)pos.Y;
        }

        InvalidateVisual();
    }

    private void OpenGlPageControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!HaveSkin)
            return;

        if (e.InitialPressMouseButton == MouseButton.Right)
        {
            SaveXY.X = XY.X;
            SaveXY.Y = XY.Y;
        }

        InvalidateVisual();
    }

    private void OpenGlPageControl_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (!HaveSkin)
            return;

        var po = e.GetCurrentPoint(this);

        if (po.Properties.IsLeftButtonPressed)
        {
            var point = e.GetPosition(this);
            RotXY.Y = (float)point.X - DiffXY.X;
            RotXY.X = (float)point.Y + DiffXY.Y;
        }
        else if (po.Properties.IsRightButtonPressed)
        {
            var point = e.GetPosition(this);
            XY.X = (-(LastXY.X - (float)point.X) / 100) + SaveXY.X;
            XY.Y = ((LastXY.Y - (float)point.Y) / 100) + SaveXY.Y;
        }

        InvalidateVisual();
    }

    private void OpenGlPageControl_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (!HaveSkin)
            return;

        if (e.Delta.Y > 0)
        {
            Dis -= 0.1f;
        }
        else if (e.Delta.Y < 0)
        {
            Dis += 0.1f;
        }

        InvalidateVisual();
    }

    public unsafe override void Render(DrawingContext context)
    {
        base.Render(context);

        if (SwitchSkin)
        {
            LoadSkin();
            SwitchSkin = false;
        }
        if (SwitchModel)
        {
            LoadModel();
            SwitchModel = false;
        }

        if (!HaveSkin)
            return;

        int x = (int)Bounds.Width;
        int y = (int)Bounds.Height;

        var screen = Window.PlatformImpl?.Screen.ScreenFromWindow(Window.PlatformImpl);
        if (screen != null)
        {
            x = (int)(Bounds.Width * screen.Scaling);
            y = (int)(Bounds.Height * screen.Scaling);
        }

        var pos = this.GetXY();
        //y += (int)pos.Y;

        if (LastSize.X != x || LastSize.Y != y)
        {
            LastSize = new(x, y);
            bitmap?.Dispose();
            bitmap = new WriteableBitmap(new PixelSize(x, y), new Vector(96, 96), Avalonia.Platform.PixelFormat.Rgba8888, AlphaFormat.Premul);
        }

        if (Window1.Size.X != x || Window1.Size.Y != y)
        {
            Window1.Size = new(x, y);
        }

        GL.Viewport(0, 0, x, y);

        if (App.BackBitmap != null)
        {
            GL.ClearColor(0, 0, 0, 0.2f);
        }
        else
        {
            GL.ClearColor(0, 0, 0, 1);
        }

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        CheckError();
        //gl.GL_CULL_FACE
        GL.Enable(EnableCap.CullFace);
        GL.Enable(EnableCap.DepthTest);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, texture);
        GL.UseProgram(_shaderProgram);
        CheckError();

        var modelLoc = GL.GetUniformLocation(_shaderProgram, "uModel");
        var viewLoc = GL.GetUniformLocation(_shaderProgram, "uView");
        var projectionLoc = GL.GetUniformLocation(_shaderProgram, "uProjection");

        CheckError();

        var projection =
        Matrix4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), (float)(Bounds.Width / Bounds.Height),
            0.001f, 1000);

        var view = Matrix4.LookAt(new(0, 0, Dis), new(), new(0, 1, 0));
        var model = Matrix4.CreateRotationX(RotXY.X / 360) * Matrix4.CreateRotationY(RotXY.Y / 360)
            * Matrix4.CreateTranslation(new(XY.X, XY.Y, 0));
        GL.UniformMatrix4(modelLoc, false, ref model);
        GL.UniformMatrix4(viewLoc, false, ref view);
        GL.UniformMatrix4(projectionLoc, false, ref projection);

        GL.BindVertexArray(_vertexArrayObject);
        GL.DrawElements(PrimitiveType.Triangles, steveModelDrawOrder, DrawElementsType.UnsignedShort, IntPtr.Zero);

        GL.Enable(EnableCap.Blend); //GL_BLEND
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha); //gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA
        GL.DepthMask(false);
        GL.BindVertexArray(_vertexArrayObjectTop);
        GL.DrawElements(PrimitiveType.Triangles, steveModelDrawOrderTop, DrawElementsType.UnsignedShort, IntPtr.Zero);
        GL.Disable(EnableCap.Blend);
        GL.DepthMask(true);
        
        using (var fb = bitmap.Lock())
        {
            GL.ReadPixels(0, 0, x, y, PixelFormat.Rgba, PixelType.UnsignedByte, fb.Address);
        }

        context.DrawImage(bitmap, new(0, 0, x, y), Bounds);

        CheckError();
    }

    private static void CompileShader(int shader)
    {
        // Try to compile the shader
        GL.CompileShader(shader);

        // Check for compilation errors
        GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
        if (code != (int)All.True)
        {
            // We can use `GL.GetShaderInfoLog(shader)` to get information about the error.
            var infoLog = GL.GetShaderInfoLog(shader);
            Console.WriteLine($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
        }
    }

    private static void LinkProgram(int program)
    {
        // We link the program
        GL.LinkProgram(program);

        // Check for linking errors
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
        if (code != (int)All.True)
        {
            // We can use `GL.GetProgramInfoLog(program)` to get information about the error.
            GL.GetProgramInfoLog(program, out var error);
            Console.WriteLine($"Error occurred whilst linking Program({program})\n\n{error}");
        }
    }

    private static string GetShader(bool fragment, string shader)
    {
        var version = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? 150 : 120;
        var data = "#version " + version + "\n";
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

    private static void CheckError()
    {
        ErrorCode err;
        while ((err = GL.GetError()) != ErrorCode.NoError)
            Console.WriteLine(err);
    }

    internal void Close()
    {
        Window1.Close();
        
        // Unbind everything
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        GL.BindVertexArray(0);
        GL.UseProgram(0);

        // Delete all resources.
        GL.DeleteBuffer(_vertexBufferObject);
        GL.DeleteBuffer(_indexBufferObject);
        GL.DeleteVertexArray(_vertexArrayObject);

        GL.DeleteBuffer(_vertexBufferObjectTop);
        GL.DeleteBuffer(_indexBufferObjectTop);
        GL.DeleteVertexArray(_vertexArrayObjectTop);

        GL.DeleteProgram(_shaderProgram);
        GL.DeleteShader(_fragmentShader);
        GL.DeleteShader(_vertexShader);

        Window1.Dispose();
    }
}
