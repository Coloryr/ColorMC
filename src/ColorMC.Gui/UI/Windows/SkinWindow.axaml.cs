using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Avalonia.Threading;
using ColorMC.Gui.SkinModel;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils.LaunchSetting;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace ColorMC.Gui.UI.Windows;

public partial class SkinWindow : Window
{
    public SkinWindow()
    {
        InitializeComponent();

        Head.SetWindow(this);
        this.BindFont();
        Icon = App.Icon;
        Rectangle1.MakeResizeDrag(this);

        ComboBox1.Items = UserBinding.GetSkinType();

        Skin.SetWindow(this);

        Text1.Text = Skin.Info;

        ComboBox2.Items = new List<string>()
        {
            "手臂旋转", "腿部旋转", "头部旋转"
        };

        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;
        ComboBox2.SelectionChanged += ComboBox2_SelectionChanged;
        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        Button3.Click += Button3_Click;
        Button4.Click += Button4_Click;
        Button5.Click += Button5_Click;
        CheckBox1.Click += CheckBox1_Click;

        Opened += SkinWindow_Opened;
        Closed += SkinWindow_Closed;

        App.PicUpdate += Update;
        App.SkinLoad += App_SkinLoad;

        Slider1.PropertyChanged += Slider1_PropertyChanged;
        Slider2.PropertyChanged += Slider2_PropertyChanged;
        Slider3.PropertyChanged += Slider3_PropertyChanged;

        ComboBox2.SelectedIndex = 0;

        Check();
        Update();
    }

    private void Button5_Click(object? sender, RoutedEventArgs e)
    {
        switch (ComboBox2.SelectedIndex)
        {
            case 0:
                Skin.ArmRotate.X = 0;
                Skin.ArmRotate.Y = 0;
                Skin.ArmRotate.Z = 0;
                break;
            case 1:
                Skin.LegRotate.X = 0;
                Skin.LegRotate.Y = 0;
                Skin.LegRotate.Z = 0;
                break;
            case 2:
                Skin.HeadRotate.X = 0;
                Skin.HeadRotate.Y = 0;
                Skin.HeadRotate.Z = 0;
                break;
            default:
                return;
        }
        Slider1.Value = 0;
        Slider2.Value = 0;
        Slider3.Value = 0;
        Skin.InvalidateVisual();
    }

    private void Slider3_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == "Value")
        {
            switch (ComboBox2.SelectedIndex)
            {
                case 0:
                    Skin.ArmRotate.Z = (float)Slider2.Value;
                    break;
                case 1:
                    Skin.LegRotate.Z = (float)Slider3.Value;
                    break;
                case 2:
                    Skin.HeadRotate.Z = (float)Slider3.Value;
                    break;
                default:
                    return;
            }
            Skin.InvalidateVisual();
        }
    }

    private void Slider2_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == "Value")
        {
            switch (ComboBox2.SelectedIndex)
            {
                case 0:
                    Skin.ArmRotate.Y = (float)Slider2.Value;
                    break;
                case 1:
                    Skin.LegRotate.Y = (float)Slider2.Value;
                    break;
                case 2:
                    Skin.HeadRotate.Y = (float)Slider2.Value;
                    break;
                default:
                    return;
            }
            Skin.InvalidateVisual();
        }
    }

    private void Slider1_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == "Value")
        {
            switch (ComboBox2.SelectedIndex)
            {
                case 0:
                    Skin.ArmRotate.X = (float)Slider1.Value;
                    break;
                case 1:
                    Skin.LegRotate.X = (float)Slider1.Value;
                    break;
                case 2:
                    Skin.HeadRotate.X = (float)Slider1.Value;
                    break;
                default:
                    return;
            }
            Skin.InvalidateVisual();
        }
    }

    private void ComboBox2_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Vector3 rotate;
        switch (ComboBox2.SelectedIndex)
        {
            case 0:
                rotate = Skin.ArmRotate;
                Slider1.Minimum = 0;
                Slider2.Minimum = -360;
                Slider3.Minimum = 0;
                Slider3.IsEnabled = false;
                break;
            case 1:
                rotate = Skin.LegRotate;
                Slider1.Minimum = 0;
                Slider2.Minimum = -360;
                Slider3.Minimum = 0;
                Slider3.IsEnabled = false;
                break;
            case 2:
                rotate = Skin.HeadRotate;
                Slider3.IsEnabled = true;
                Slider1.Minimum = -360;
                Slider2.Minimum = -360;
                Slider3.Minimum = -360;
                break;
            default:
                return;
        }

        Slider1.Value = rotate.X;
        Slider2.Value = rotate.Y;
        Slider3.Value = rotate.Z;
    }

    private void CheckBox1_Click(object? sender, RoutedEventArgs e)
    {
        Skin.SetTopDisplay(CheckBox1.IsChecked == true);
    }

    private async void Button4_Click(object? sender, RoutedEventArgs e)
    {
        var res = await BaseBinding.OpSave(this, "保存皮肤", ".png", "skin.png");
        if (!string.IsNullOrWhiteSpace(res))
        {
            await UserBinding.SkinImage.SaveAsPngAsync(res);
            Info2.Show("已保存");
        }
    }

    private void SkinWindow_Opened(object? sender, EventArgs e)
    {
        var temp = Matrix.CreateRotation(Math.PI);
        Skin.RenderTransform = new ImmutableTransform(temp);
    }

    private void App_SkinLoad()
    {
        Skin.ChangeSkin();

        Check();
    }

    private void Button3_Click(object? sender, RoutedEventArgs e)
    {
        UserBinding.EditSkin(this);
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        Skin.Reset();
    }

    private void Check()
    {
        if (Skin.HaveSkin)
        {
            Button4.IsEnabled = true;
        }
        else
        {
            Button4.IsEnabled = false;
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

    private async void Button1_Click(object? sender, RoutedEventArgs e)
    {
        await UserBinding.LoadSkin();
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

    public record VAOItem
    {
        public int VertexBufferObject;
        public int IndexBufferObject;
        public int VertexArrayObject;
    }

    public record ModelVAO
    {
        public VAOItem Head = new();
        public VAOItem Body = new();
        public VAOItem LeftArm = new();
        public VAOItem RightArm = new();
        public VAOItem LeftLeg = new();
        public VAOItem RightLeg = new();
    }

    private string VertexShaderSource => GetShader(false, @"
        attribute vec3 a_Position;
        attribute vec2 a_texCoord;
        uniform mat4 uModel;
        uniform mat4 uProjection;
        uniform mat4 uView;
        uniform mat4 uSelf;
        varying vec2 v_texCoord;
        void main()
        {
            v_texCoord = a_texCoord;
            
            gl_Position = uProjection * uView * uModel * uSelf * vec4(a_Position, 1.0);
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

    public bool HaveSkin { get; private set; } = false;
    private bool SwitchModel = false;
    private bool SwitchSkin = false;
    private bool TopDisplay = true;

    private float Dis = 1;
    private Vector2 RotXY;
    private Vector2 DiffXY;

    private Vector2 XY;
    private Vector2 SaveXY;
    private Vector2 LastXY;

    private int texture;

    public SkinType steveModelType;
    private int steveModelDrawOrder;

    private int _vertexShader;
    private int _fragmentShader;
    private int _shaderProgram;

    public Vector3 ArmRotate;
    public Vector3 LegRotate;
    public Vector3 HeadRotate;

    private int VaoList;

    private Vector2 LastSize;
    private WriteableBitmap bitmap;

    public string Info;

    private readonly ModelVAO NormalVAO = new();
    private readonly ModelVAO TopVAO = new();

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

        InitVAO(NormalVAO);
        InitVAO(TopVAO);

        CheckError();

        PointerWheelChanged += OpenGlPageControl_PointerWheelChanged;
        PointerPressed += OpenGlPageControl_PointerPressed;
        PointerReleased += OpenGlPageControl_PointerReleased;
        PointerMoved += OpenGlPageControl_PointerMoved;

        LoadSkin();
    }

    private static void InitVAOItem(VAOItem item)
    {
        int[] temp1 = new int[2];
        GL.GenBuffers(2, temp1);
        item.VertexBufferObject = temp1[0];
        item.IndexBufferObject = temp1[1];
    }

    private static void InitVAO(ModelVAO vao)
    {
        int[] temp1 = new int[6];

        GL.GenVertexArrays(6, temp1);

        vao.Head.VertexArrayObject = temp1[0];
        vao.Body.VertexArrayObject = temp1[1];
        vao.LeftArm.VertexArrayObject = temp1[2];
        vao.RightArm.VertexArrayObject = temp1[3];
        vao.LeftLeg.VertexArrayObject = temp1[4];
        vao.RightLeg.VertexArrayObject = temp1[5];

        InitVAOItem(vao.Head);
        InitVAOItem(vao.Body);
        InitVAOItem(vao.LeftArm);
        InitVAOItem(vao.RightArm);
        InitVAOItem(vao.LeftLeg);
        InitVAOItem(vao.RightLeg);
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
        Dis = 1;
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

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, UserBinding.SkinImage.Width,
            UserBinding.SkinImage.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

        CheckError();

        LoadModel();

        HaveSkin = true;
        Dispatcher.UIThread.Post(() =>
        {
            Window.ComboBox1.SelectedIndex = (int)steveModelType;
            Window.Label1.IsVisible = false;
        });
    }

    private unsafe void PutVAO(VAOItem vao, ModelItem model, float[] uv)
    {
        GL.UseProgram(_shaderProgram);
        GL.BindVertexArray(vao.VertexArrayObject);

        int a_Position = GL.GetAttribLocation(_shaderProgram, "a_Position");
        int a_texCoord = GL.GetAttribLocation(_shaderProgram, "a_texCoord");

        GL.DisableVertexAttribArray((uint)a_Position);
        GL.DisableVertexAttribArray((uint)a_texCoord);

        int size = model.Model.Length / 3;

        var points = new Vertex[size];

        for (var primitive = 0; primitive < size; primitive++)
        {
            var srci = primitive * 3;
            var srci1 = primitive * 2;
            points[primitive] = new Vertex
            {
                Position = new(model.Model[srci], model.Model[srci + 1], model.Model[srci + 2]),
                UV = new(uv[srci1], uv[srci1 + 1])
            };
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, vao.VertexBufferObject);
        var vertexSize = Marshal.SizeOf<Vertex>();
        fixed (void* pdata = points)
            GL.BufferData(BufferTarget.ArrayBuffer, points.Length * vertexSize,
                new IntPtr(pdata), BufferUsageHint.StaticDraw);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, vao.IndexBufferObject);
        fixed (void* pdata = model.Point)
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                model.Point.Length * sizeof(ushort), new IntPtr(pdata), BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer((uint)a_Position, 3, VertexAttribPointerType.Float,
            false, 5 * sizeof(float), 0);
        GL.VertexAttribPointer((uint)a_texCoord, 2, VertexAttribPointerType.Float,
            false, 5 * sizeof(float), 3 * sizeof(float));

        GL.EnableVertexAttribArray((uint)a_Position);
        GL.EnableVertexAttribArray((uint)a_texCoord);

        GL.BindVertexArray(0);

        CheckError();
    }

    private unsafe void LoadModel()
    {
        var steve = new Steve3DModel();
        var stevetex = new Steve3DTexture();

        var normal = steve.GetSteve(steveModelType);
        var top = steve.GetSteveTop(steveModelType);
        var tex = stevetex.GetSteveTexture(steveModelType);
        var textop = stevetex.GetSteveTextureTop(steveModelType);

        steveModelDrawOrder = normal.Head.Point.Length;

        PutVAO(NormalVAO.Head, normal.Head, tex.Head);
        PutVAO(NormalVAO.Body, normal.Body, tex.Body);
        PutVAO(NormalVAO.LeftArm, normal.LeftArm, tex.LeftArm);
        PutVAO(NormalVAO.RightArm, normal.RightArm, tex.RightArm);
        PutVAO(NormalVAO.LeftLeg, normal.LeftLeg, tex.LeftLeg);
        PutVAO(NormalVAO.RightLeg, normal.RightLeg, tex.RightLeg);

        PutVAO(TopVAO.Head, top.Head, textop.Head);
        if (steveModelType != SkinType.Old)
        {
            PutVAO(TopVAO.Head, top.Head, textop.Head);
            PutVAO(TopVAO.Body, top.Body, textop.Body);
            PutVAO(TopVAO.LeftArm, top.LeftArm, textop.LeftArm);
            PutVAO(TopVAO.RightArm, top.RightArm, textop.RightArm);
            PutVAO(TopVAO.LeftLeg, top.LeftLeg, textop.LeftLeg);
            PutVAO(TopVAO.RightLeg, top.RightLeg, textop.RightLeg);
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
            Dis += 0.1f;
        }
        else if (e.Delta.Y < 0)
        {
            Dis -= 0.1f;
        }

        InvalidateVisual();
    }

    private void DrawNormal()
    {
        var modelLoc = GL.GetUniformLocation(_shaderProgram, "uSelf");
        var model = Matrix4.Identity;
        GL.UniformMatrix4(modelLoc, false, ref model);

        GL.BindVertexArray(NormalVAO.Body.VertexArrayObject);
        GL.DrawElements(PrimitiveType.Triangles, steveModelDrawOrder, DrawElementsType.UnsignedShort, IntPtr.Zero);

        model = Matrix4.CreateTranslation(0, CubeC.Value, 0) *
               Matrix4.CreateRotationZ(HeadRotate.X / 360) *
               Matrix4.CreateRotationX(HeadRotate.Y / 360) *
               Matrix4.CreateRotationY(HeadRotate.Z / 360) *
               Matrix4.CreateTranslation(0, CubeC.Value * 1.5f, 0);
        GL.UniformMatrix4(modelLoc, false, ref model);

        GL.BindVertexArray(NormalVAO.Head.VertexArrayObject);
        GL.DrawElements(PrimitiveType.Triangles, steveModelDrawOrder, DrawElementsType.UnsignedShort, IntPtr.Zero);

        if (steveModelType == SkinType.New_Slim)
        {
            model = Matrix4.CreateTranslation(CubeC.Value / 2, -(1.375f * CubeC.Value), 0) *
                Matrix4.CreateRotationZ(ArmRotate.X / 360) *
                Matrix4.CreateRotationX(ArmRotate.Y / 360) *
                Matrix4.CreateTranslation(
                    (1.375f * CubeC.Value) - (CubeC.Value / 2), 1.375f * CubeC.Value, 0);
        }
        else
        {
            model = Matrix4.CreateTranslation(CubeC.Value / 2, -(1.5f * CubeC.Value), 0) *
                Matrix4.CreateRotationZ(ArmRotate.X / 360) *
                Matrix4.CreateTranslation(
                    (1.5f * CubeC.Value) - (CubeC.Value / 2), 1.5f * CubeC.Value, 0);
        }
        GL.UniformMatrix4(modelLoc, false, ref model);

        GL.BindVertexArray(NormalVAO.LeftArm.VertexArrayObject);
        GL.DrawElements(PrimitiveType.Triangles, steveModelDrawOrder, DrawElementsType.UnsignedShort, IntPtr.Zero);

        if (steveModelType == SkinType.New_Slim)
        {
            model = Matrix4.CreateTranslation(-CubeC.Value / 2, -(1.375f * CubeC.Value), 0) *
                Matrix4.CreateRotationZ(-ArmRotate.X / 360) *
                Matrix4.CreateRotationX(-ArmRotate.Y / 360) *
                Matrix4.CreateTranslation(
                    (-1.375f * CubeC.Value) + (CubeC.Value / 2), 1.375f * CubeC.Value, 0);
        }
        else
        {
            model = Matrix4.CreateTranslation(-CubeC.Value / 2, -(1.5f * CubeC.Value), 0) *
                Matrix4.CreateRotationZ(-ArmRotate.X / 360) *
                Matrix4.CreateRotationX(-ArmRotate.Y / 360) *
                Matrix4.CreateTranslation(
                    (-1.5f * CubeC.Value) + (CubeC.Value / 2), 1.5f * CubeC.Value, 0);
        }
        GL.UniformMatrix4(modelLoc, false, ref model);

        GL.BindVertexArray(NormalVAO.RightArm.VertexArrayObject);
        GL.DrawElements(PrimitiveType.Triangles, steveModelDrawOrder, DrawElementsType.UnsignedShort, IntPtr.Zero);

        model = Matrix4.CreateTranslation(0, -1.5f * CubeC.Value, 0) *
               Matrix4.CreateRotationZ(LegRotate.X / 360) *
               Matrix4.CreateRotationX(LegRotate.Y / 360) *
               Matrix4.CreateTranslation(CubeC.Value * 0.5f, -CubeC.Value * 1.5f, 0);
        GL.UniformMatrix4(modelLoc, false, ref model);

        GL.BindVertexArray(NormalVAO.LeftLeg.VertexArrayObject);
        GL.DrawElements(PrimitiveType.Triangles, steveModelDrawOrder, DrawElementsType.UnsignedShort, IntPtr.Zero);

        model = Matrix4.CreateTranslation(0, -1.5f * CubeC.Value, 0) *
               Matrix4.CreateRotationZ(-LegRotate.X / 360) *
               Matrix4.CreateRotationX(-LegRotate.Y / 360) *
               Matrix4.CreateTranslation(-CubeC.Value * 0.5f, -CubeC.Value * 1.5f, 0);
        GL.UniformMatrix4(modelLoc, false, ref model);

        GL.BindVertexArray(NormalVAO.RightLeg.VertexArrayObject);
        GL.DrawElements(PrimitiveType.Triangles, steveModelDrawOrder, DrawElementsType.UnsignedShort, IntPtr.Zero);
    }

    private void DrawTop()
    {
        var modelLoc = GL.GetUniformLocation(_shaderProgram, "uSelf");
        var model = Matrix4.Identity;
        GL.UniformMatrix4(modelLoc, false, ref model);

        GL.BindVertexArray(TopVAO.Body.VertexArrayObject);
        GL.DrawElements(PrimitiveType.Triangles, steveModelDrawOrder, DrawElementsType.UnsignedShort, IntPtr.Zero);

        model = Matrix4.CreateTranslation(0, CubeC.Value, 0) *
               Matrix4.CreateRotationZ(HeadRotate.X / 360) *
               Matrix4.CreateRotationX(HeadRotate.Y / 360) *
               Matrix4.CreateRotationY(HeadRotate.Z / 360) *
               Matrix4.CreateTranslation(0, CubeC.Value * 1.5f, 0);
        GL.UniformMatrix4(modelLoc, false, ref model);

        GL.BindVertexArray(TopVAO.Head.VertexArrayObject);
        GL.DrawElements(PrimitiveType.Triangles, steveModelDrawOrder, DrawElementsType.UnsignedShort, IntPtr.Zero);

        if (steveModelType == SkinType.New_Slim)
        {
            model = Matrix4.CreateTranslation(CubeC.Value / 2, -(1.375f * CubeC.Value), 0) *
                Matrix4.CreateRotationZ(ArmRotate.X / 360) *
                Matrix4.CreateRotationX(ArmRotate.Y / 360) *
                Matrix4.CreateTranslation(
                    (1.375f * CubeC.Value) - (CubeC.Value / 2), 1.375f * CubeC.Value, 0);
        }
        else
        {
            model = Matrix4.CreateTranslation(CubeC.Value / 2, -(1.5f * CubeC.Value), 0) *
                Matrix4.CreateRotationZ(ArmRotate.X / 360) *
                Matrix4.CreateTranslation(
                    (1.5f * CubeC.Value) - (CubeC.Value / 2), 1.5f * CubeC.Value, 0);
        }
        GL.UniformMatrix4(modelLoc, false, ref model);

        GL.BindVertexArray(TopVAO.LeftArm.VertexArrayObject);
        GL.DrawElements(PrimitiveType.Triangles, steveModelDrawOrder, DrawElementsType.UnsignedShort, IntPtr.Zero);

        if (steveModelType == SkinType.New_Slim)
        {
            model = Matrix4.CreateTranslation(-CubeC.Value / 2, -(1.375f * CubeC.Value), 0) *
                Matrix4.CreateRotationZ(-ArmRotate.X / 360) *
                Matrix4.CreateRotationX(-ArmRotate.Y / 360) *
                Matrix4.CreateTranslation(
                    (-1.375f * CubeC.Value) + (CubeC.Value / 2), 1.375f * CubeC.Value, 0);
        }
        else
        {
            model = Matrix4.CreateTranslation(-CubeC.Value / 2, -(1.5f * CubeC.Value), 0) *
                Matrix4.CreateRotationZ(-ArmRotate.X / 360) *
                Matrix4.CreateRotationX(-ArmRotate.Y / 360) *
                Matrix4.CreateTranslation(
                    (-1.5f * CubeC.Value) + (CubeC.Value / 2), 1.5f * CubeC.Value, 0);
        }
        GL.UniformMatrix4(modelLoc, false, ref model);

        GL.BindVertexArray(TopVAO.RightArm.VertexArrayObject);
        GL.DrawElements(PrimitiveType.Triangles, steveModelDrawOrder, DrawElementsType.UnsignedShort, IntPtr.Zero);

        model = Matrix4.CreateTranslation(0, -1.5f * CubeC.Value, 0) *
               Matrix4.CreateRotationZ(LegRotate.X / 360) *
               Matrix4.CreateRotationX(LegRotate.Y / 360) *
               Matrix4.CreateTranslation(CubeC.Value * 0.5f, -CubeC.Value * 1.5f, 0);
        GL.UniformMatrix4(modelLoc, false, ref model);

        GL.BindVertexArray(TopVAO.LeftLeg.VertexArrayObject);
        GL.DrawElements(PrimitiveType.Triangles, steveModelDrawOrder, DrawElementsType.UnsignedShort, IntPtr.Zero);

        model = Matrix4.CreateTranslation(0, -1.5f * CubeC.Value, 0) *
               Matrix4.CreateRotationZ(-LegRotate.X / 360) *
               Matrix4.CreateRotationX(-LegRotate.Y / 360) *
               Matrix4.CreateTranslation(-CubeC.Value * 0.5f, -CubeC.Value * 1.5f, 0);
        GL.UniformMatrix4(modelLoc, false, ref model);

        GL.BindVertexArray(TopVAO.RightLeg.VertexArrayObject);
        GL.DrawElements(PrimitiveType.Triangles, steveModelDrawOrder, DrawElementsType.UnsignedShort, IntPtr.Zero);
    }

    public override void Render(DrawingContext context)
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


        if (LastSize.X != x || LastSize.Y != y)
        {
            LastSize = new(x, y);
            bitmap?.Dispose();
            bitmap = new WriteableBitmap(new PixelSize(x, y), new Vector(96, 96),
                Avalonia.Platform.PixelFormat.Rgba8888, AlphaFormat.Premul);
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

        var viewLoc = GL.GetUniformLocation(_shaderProgram, "uView");
        var projectionLoc = GL.GetUniformLocation(_shaderProgram, "uProjection");
        var modelLoc = GL.GetUniformLocation(_shaderProgram, "uModel");

        CheckError();

        var projection =
        Matrix4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), (float)(Bounds.Width / Bounds.Height),
            0.001f, 1000) ;

        var view = Matrix4.LookAt(new(0, 0, 7), new(), new(0, 1, 0));

        var model = Matrix4.CreateRotationX(RotXY.X / 360)
                    * Matrix4.CreateRotationY(RotXY.Y / 360)
                    * Matrix4.CreateTranslation(new(XY.X, XY.Y, 0))
                    * Matrix4.CreateScale(Dis);

        GL.UniformMatrix4(viewLoc, false, ref view);
        GL.UniformMatrix4(modelLoc, false, ref model);
        GL.UniformMatrix4(projectionLoc, false, ref projection);

        DrawNormal();

        if (TopDisplay)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.DepthMask(false);

            DrawTop();

            GL.Disable(EnableCap.Blend);
            GL.DepthMask(true);
        }

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

    private static void DeleteVAOItem(VAOItem item)
    {
        GL.DeleteBuffer(item.VertexBufferObject);
        GL.DeleteBuffer(item.IndexBufferObject);
    }

    private static void DeleteVAO(ModelVAO vao)
    {
        GL.DeleteVertexArray(vao.Head.VertexArrayObject);
        GL.DeleteVertexArray(vao.Body.VertexArrayObject);
        GL.DeleteVertexArray(vao.LeftArm.VertexArrayObject);
        GL.DeleteVertexArray(vao.RightArm.VertexArrayObject);
        GL.DeleteVertexArray(vao.LeftLeg.VertexArrayObject);
        GL.DeleteVertexArray(vao.RightLeg.VertexArrayObject);

        DeleteVAOItem(vao.Head);
        DeleteVAOItem(vao.Body);
        DeleteVAOItem(vao.LeftArm);
        DeleteVAOItem(vao.RightArm);
        DeleteVAOItem(vao.LeftLeg);
        DeleteVAOItem(vao.RightLeg);
    }

    public void Close()
    {
        Window1.Close();
        bitmap?.Dispose();

        // Unbind everything
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        GL.BindVertexArray(0);
        GL.UseProgram(0);

        // Delete all resources.
        DeleteVAO(NormalVAO);
        DeleteVAO(TopVAO);

        GL.DeleteProgram(_shaderProgram);
        GL.DeleteShader(_fragmentShader);
        GL.DeleteShader(_vertexShader);

        Window1.Dispose();
    }

    public void SetTopDisplay(bool value)
    {
        TopDisplay = value;

        InvalidateVisual();
    }
}
