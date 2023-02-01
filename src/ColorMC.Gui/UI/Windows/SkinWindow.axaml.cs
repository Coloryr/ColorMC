using Avalonia.Controls;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using static Avalonia.OpenGL.GlConsts;
using System.Runtime.InteropServices;
using System.Numerics;
using System;
using ColorMC.Gui.Skin;
using ColorMC.Gui.Skin.Model;
using Avalonia.Input;
using ColorMC.Gui.UIBinding;
using Avalonia.Threading;
using Avalonia.Interactivity;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Avalonia.Media.Imaging;
using ColorMC.Core.Net;
using Avalonia.Utilities;
using Avalonia.Media;
using ColorMC.Gui.Utils.LaunchSetting;
using ColorMC.Core.Game.Auth;
using ColorMC.Core.Utils;

namespace ColorMC.Gui.UI.Windows;

public partial class SkinWindow : Window
{
    public SkinWindow()
    {
        InitializeComponent();

        FontFamily = Program.Font;

        ComboBox1.Items = UserBinding.GetSkinType();

        GL.SetWindow(this);

        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;
        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        Button3.Click += Button3_Click;

        Closed += SkinWindow_Closed;

        App.PicUpdate += Update;
        App.UserEdit += App_UserEdit;

        Check();
        Update();
    }

    private void Button3_Click(object? sender, RoutedEventArgs e)
    {
        UserBinding.EditSkin();
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        GL.Reset();
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

    private void App_UserEdit()
    {
        Check();

        GL.ChangeSkin();
    }

    private void SkinWindow_Closed(object? sender, EventArgs e)
    {
        App.PicUpdate -= Update;

        App.SkinWindow = null;
    }

    private void ComboBox1_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ComboBox1.SelectedIndex == (int)GL.steveModelType)
            return;
        if (ComboBox1.SelectedIndex == (int)SkinType.UNKNOWN)
        {
            Info.Show(Localizer.Instance["SkinWindow.Info1"]);
            ComboBox1.SelectedIndex = (int)GL.steveModelType;
            return;
        }
        GL.ChangeType(ComboBox1.SelectedIndex);
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        GL.ChangeSkin();
    }

    public void Update()
    {
        App.Update(this, Image_Back, Grid1);

        GL.InvalidateVisual();
    }
}

public class OpenGlPageControl : OpenGlControlBase
{
    private SkinWindow Window;
    private string _info = string.Empty;

    public static readonly DirectProperty<OpenGlPageControl, string> InfoProperty =
        AvaloniaProperty.RegisterDirect<OpenGlPageControl, string>("Info", o => o.Info, (o, v) => o.Info = v);

    public string Info
    {
        get => _info;
        private set => SetAndRaise(InfoProperty, ref _info, value);
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

    public delegate void GlFunc1(int v1, int v2);
    public delegate void GlFunc2(int v1);
    public delegate void GlFunc3(float v1);
    public delegate void GlFunc4(bool v1);
    public delegate void GlFunc5(int v1, int v2, nint v3);

    public GlFunc2 glDepthFunc;
    public GlFunc3 glClearDepth;
    public GlFunc1 glBlendFunc;
    public GlFunc4 glDepthMask;
    public GlFunc2 glCullFace;
    public GlFunc2 glDisable;
    public GlFunc2 glDisableVertexAttribArray;
    public GlFunc5 glLightfv;

    public void SetWindow(SkinWindow window)
    {
        Window = window;
    }

    protected override unsafe void OnOpenGlInit(GlInterface gl, int fb)
    {
        IntPtr temp = gl.GetProcAddress("glDepthFunc");
        glDepthFunc = (GlFunc2)Marshal.GetDelegateForFunctionPointer(temp, typeof(GlFunc2));
        temp = gl.GetProcAddress("glClearDepth");
        glClearDepth = (GlFunc3)Marshal.GetDelegateForFunctionPointer(temp, typeof(GlFunc3));
        temp = gl.GetProcAddress("glBlendFunc");
        glBlendFunc = (GlFunc1)Marshal.GetDelegateForFunctionPointer(temp, typeof(GlFunc1));
        temp = gl.GetProcAddress("glDepthMask");
        glDepthMask = (GlFunc4)Marshal.GetDelegateForFunctionPointer(temp, typeof(GlFunc4));
        temp = gl.GetProcAddress("glCullFace");
        glCullFace = (GlFunc2)Marshal.GetDelegateForFunctionPointer(temp, typeof(GlFunc2));
        temp = gl.GetProcAddress("glDisable");
        glDisable = (GlFunc2)Marshal.GetDelegateForFunctionPointer(temp, typeof(GlFunc2));
        temp = gl.GetProcAddress("glDisableVertexAttribArray");
        glDisableVertexAttribArray = (GlFunc2)Marshal.GetDelegateForFunctionPointer(temp, typeof(GlFunc2));
        temp = gl.GetProcAddress("glLightfv");
        glLightfv = (GlFunc5)Marshal.GetDelegateForFunctionPointer(temp, typeof(GlFunc5));

        gl.ClearColor(0, 0, 0, 1);

        //GL_BLEND
        gl.Enable(3042);

        //gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA
        glBlendFunc(770, 771);

        //gl.GL_BACK
        glCullFace(1029);

        //glShadeModel(GL_SMOOTH);

        //int[] AmbientLight = { 1, 1, 1, 1 };

        //fixed (void* pdata = AmbientLight)
        //    glLightfv(16384, 4608, new IntPtr(pdata));
        //gl.Enable(16384);       //开启GL_LIGHT0光源
        //gl.Enable(2896);     //开启光照系统
        //gl.Enable(GL_DEPTH_TEST);

        CheckError(gl);

        Info = $"Renderer: {gl.GetString(GL_RENDERER)} Version: {gl.GetString(GL_VERSION)}";

        _vertexShader = gl.CreateShader(GL_VERTEX_SHADER);
        var error = gl.CompileShaderAndGetError(_vertexShader, VertexShaderSource);
        Console.WriteLine(error);

        _fragmentShader = gl.CreateShader(GL_FRAGMENT_SHADER);
        error = gl.CompileShaderAndGetError(_fragmentShader, FragmentShaderSource);
        Console.WriteLine(error);

        _shaderProgram = gl.CreateProgram();
        gl.AttachShader(_shaderProgram, _vertexShader);
        gl.AttachShader(_shaderProgram, _fragmentShader);
        error = gl.LinkProgramAndGetError(_shaderProgram);
        CheckError(gl);

        int[] temp2 = new int[3];

        fixed (int* pdata = temp2)
            gl.GenVertexArrays(3, pdata);

        _vertexArrayObject = temp2[0];
        _vertexArrayObjectTop = temp2[1];

        _vertexBufferObject = gl.GenBuffer();
        _indexBufferObject = gl.GenBuffer();

        _vertexBufferObjectTop = gl.GenBuffer();
        _indexBufferObjectTop = gl.GenBuffer();

        PointerWheelChanged += OpenGlPageControl_PointerWheelChanged;
        PointerPressed += OpenGlPageControl_PointerPressed;
        PointerReleased += OpenGlPageControl_PointerReleased;
        PointerMoved += OpenGlPageControl_PointerMoved;

        LoadSkin(gl);
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

    private unsafe void LoadSkin(GlInterface gl)
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

        texture = gl.GenTexture();
        gl.ActiveTexture(GL_TEXTURE0);
        gl.BindTexture(GL_TEXTURE_2D, texture);

        gl.TexParameteri(
            GL_TEXTURE_2D,
            GL_TEXTURE_MIN_FILTER, GL_LINEAR
        );
        gl.TexParameteri(
            GL_TEXTURE_2D,
            GL_TEXTURE_MAG_FILTER, GL_NEAREST
        );
        //TextureWrapS ClampToEdge
        gl.TexParameteri(
            GL_TEXTURE_2D,
            10242, 33071
        );
        //TextureWrapT ClampToEdge
        gl.TexParameteri(
            GL_TEXTURE_2D,
            10243, 33071
        );

        var pixels = new byte[4 * UserBinding.SkinImage.Width * UserBinding.SkinImage.Height];
        UserBinding.SkinImage.CopyPixelDataTo(pixels);

        fixed (void* pdata = pixels)
            gl.TexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, UserBinding.SkinImage.Width, UserBinding.SkinImage.Height, 0, GL_RGBA, GL_UNSIGNED_BYTE, new IntPtr(pdata));

        CheckError(gl);

        LoadModel(gl);

        CheckError(gl);

        HaveSkin = true;
        Dispatcher.UIThread.Post(() =>
        {
            Window.ComboBox1.SelectedIndex = (int)steveModelType;
            Window.Label1.IsVisible = false;
        });
    }

    private unsafe void LoadModel(GlInterface gl)
    {
        {
            gl.UseProgram(_shaderProgram);
            gl.BindVertexArray(_vertexArrayObject);

            int a_Position = gl.GetAttribLocationString(_shaderProgram, "a_Position");
            int a_texCoord = gl.GetAttribLocationString(_shaderProgram, "a_texCoord");

            glDisableVertexAttribArray(a_Position);
            glDisableVertexAttribArray(a_texCoord);

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
                    Position = new Vector3(model[srci], model[srci + 1], model[srci + 2]),
                    UV = new Vector2(uv[srci1], uv[srci1 + 1])
                };
            }

            gl.BindBuffer(GL_ARRAY_BUFFER, _vertexBufferObject);
            var vertexSize = Marshal.SizeOf<Vertex>();
            fixed (void* pdata = points)
                gl.BufferData(GL_ARRAY_BUFFER, new IntPtr(points.Length * vertexSize),
                    new IntPtr(pdata), GL_STATIC_DRAW);

            gl.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, _indexBufferObject);
            fixed (void* pdata = draw)
                gl.BufferData(GL_ELEMENT_ARRAY_BUFFER, new IntPtr(draw.Length * sizeof(ushort)), new IntPtr(pdata), GL_STATIC_DRAW);
            
            gl.VertexAttribPointer(a_Position, 3, GL_FLOAT,
                0, 5 * sizeof(float), 0);
            gl.VertexAttribPointer(a_texCoord, 2, GL_FLOAT,
                0, 5 * sizeof(float), 3 * sizeof(float));

            gl.EnableVertexAttribArray(a_Position);
            gl.EnableVertexAttribArray(a_texCoord);

            gl.BindVertexArray(0);

            CheckError(gl);

            steveModelDrawOrder = draw.Length;
        }

        {
            gl.UseProgram(_shaderProgram);
            gl.BindVertexArray(_vertexArrayObjectTop);

            int a_Position = gl.GetAttribLocationString(_shaderProgram, "a_Position");
            int a_texCoord = gl.GetAttribLocationString(_shaderProgram, "a_texCoord");

            glDisableVertexAttribArray(a_Position);
            glDisableVertexAttribArray(a_texCoord);

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
                    Position = new Vector3(model[srci], model[srci + 1], model[srci + 2]),
                    UV = new Vector2(uv[srci1], uv[srci1 + 1])
                };
            }

            gl.BindBuffer(GL_ARRAY_BUFFER, _vertexBufferObjectTop);
            var vertexSize = Marshal.SizeOf<Vertex>();
            fixed (void* pdata = points)
                gl.BufferData(GL_ARRAY_BUFFER, new IntPtr(points.Length * vertexSize),
                    new IntPtr(pdata), GL_STATIC_DRAW);

            gl.BindBuffer(GL_ELEMENT_ARRAY_BUFFER, _indexBufferObjectTop);
            fixed (void* pdata = draw)
                gl.BufferData(GL_ELEMENT_ARRAY_BUFFER, new IntPtr(draw.Length * sizeof(ushort)), new IntPtr(pdata), GL_STATIC_DRAW);

            gl.VertexAttribPointer(a_Position, 3, GL_FLOAT,
                0, 5 * sizeof(float), 0);
            gl.VertexAttribPointer(a_texCoord, 2, GL_FLOAT,
                0, 5 * sizeof(float), 3 * sizeof(float));

            gl.EnableVertexAttribArray(a_Position);
            gl.EnableVertexAttribArray(a_texCoord);

            gl.BindVertexArray(0);

            CheckError(gl);

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

            InvalidateVisual();
        }
        else if (po.Properties.IsRightButtonPressed) 
        {
            var point = e.GetPosition(this);
            XY.X = (-(LastXY.X - (float)point.X) / 100) + SaveXY.X;
            XY.Y = ((LastXY.Y - (float)point.Y) / 100) + SaveXY.Y;

            InvalidateVisual();
        }
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

        GL.DeleteBuffer(_vertexBufferObjectTop);
        GL.DeleteBuffer(_indexBufferObjectTop);
        GL.DeleteVertexArray(_vertexArrayObjectTop);

        GL.DeleteProgram(_shaderProgram);
        GL.DeleteShader(_fragmentShader);
        GL.DeleteShader(_vertexShader);
    }

    protected override unsafe void OnOpenGlRender(GlInterface gl, int fb)
    {
        if (SwitchSkin)
        {
            LoadSkin(gl);
            SwitchSkin = false;
        }
        if (SwitchModel)
        {
            LoadModel(gl);
            SwitchModel = false;
        }

        if (!HaveSkin)
            return;

        if (App.BackBitmap != null)
        {
            gl.ClearColor(0, 0, 0, 0.2f);
        }
        else
        {
            gl.ClearColor(0, 0, 0, 1);
        }

        gl.Clear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

        var screen = Window.PlatformImpl?.Screen.ScreenFromWindow(Window.PlatformImpl);
        if (screen!=null)
        {
            gl.Viewport(0, 0, (int)(Bounds.Width * screen.Scaling), 
                (int)(Bounds.Height * screen.Scaling));
        }
        else
        {
            gl.Viewport(0, 0, (int)Bounds.Width, (int)Bounds.Height);
        }

        CheckError(gl);
        //gl.GL_CULL_FACE
        gl.Enable(2884);
        gl.Enable(GL_DEPTH_TEST);
        gl.ActiveTexture(GL_TEXTURE0);
        gl.BindTexture(GL_TEXTURE_2D, texture);
        gl.UseProgram(_shaderProgram);
        CheckError(gl);

        var modelLoc = gl.GetUniformLocationString(_shaderProgram, "uModel");
        var viewLoc = gl.GetUniformLocationString(_shaderProgram, "uView");
        var projectionLoc = gl.GetUniformLocationString(_shaderProgram, "uProjection");

        CheckError(gl);

        var projection =
        Matrix4x4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), (float)(Bounds.Width / Bounds.Height),
            0.001f, 1000);

        var view = Matrix4x4.CreateLookAt(new Vector3(0, 0, Dis), new Vector3(), new Vector3(0, 1, 0));
        var model = Matrix4x4.CreateRotationX(RotXY.X / 360) * Matrix4x4.CreateRotationY(RotXY.Y / 360)
            * Matrix4x4.CreateTranslation(new Vector3(XY.X, XY.Y, 0));
        gl.UniformMatrix4fv(modelLoc, 1, false, &model);
        gl.UniformMatrix4fv(viewLoc, 1, false, &view);
        gl.UniformMatrix4fv(projectionLoc, 1, false, &projection);

        gl.BindVertexArray(_vertexArrayObject);
        gl.DrawElements(GL_TRIANGLES, steveModelDrawOrder, GL_UNSIGNED_SHORT, IntPtr.Zero);

        gl.Enable(3042); //GL_BLEND
        glBlendFunc(770, 771); //gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA
        glDepthMask(false);
        gl.BindVertexArray(_vertexArrayObjectTop);
        gl.DrawElements(GL_TRIANGLES, steveModelDrawOrderTop, GL_UNSIGNED_SHORT, IntPtr.Zero);
        glDisable(3042);
        glDepthMask(true);

        CheckError(gl);
    }

    private string GetShader(bool fragment, string shader)
    {
        var version = GlVersion.Type == GlProfileType.OpenGL ?
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? 150 : 120 :
            100;
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

    private static void CheckError(GlInterface gl)
    {
        int err;
        while ((err = gl.GetError()) != GL_NO_ERROR)
            Console.WriteLine(err);
    }
}
