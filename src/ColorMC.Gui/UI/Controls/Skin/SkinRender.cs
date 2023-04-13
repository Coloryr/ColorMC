using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.SkinModel;
using ColorMC.Gui.UI.Animations;
using ColorMC.Gui.UIBinding;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ColorMC.Gui.UI.Controls.Skin;

internal record VAOItem
{
    public int VertexBufferObject;
    public int IndexBufferObject;
    public int VertexArrayObject;
}

internal record ModelVAO
{
    public VAOItem Head = new();
    public VAOItem Body = new();
    public VAOItem LeftArm = new();
    public VAOItem RightArm = new();
    public VAOItem LeftLeg = new();
    public VAOItem RightLeg = new();
    public VAOItem Cape = new();
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct Vertex
{
    public Vector3 Position;
    public Vector2 UV;
    public Vector3 Normal;
}

public class SkinRender : OpenGlControlBase
{
    private string VertexShaderSource =>
@"attribute vec3 a_position;
    attribute vec2 a_texCoord;
    attribute vec3 a_normal;

    uniform mat4 model;
    uniform mat4 projection;
    uniform mat4 view;
    uniform mat4 self;

    varying vec3 normalIn;
    varying vec2 texIn;
    varying vec3 fragPosIn;

    void main()
    {
        texIn = a_texCoord;

        mat4 temp = view * model * self;

        fragPosIn = vec3(model * vec4(a_position, 1.0));
        normalIn = normalize(vec3(model * vec4(a_normal, 1.0)));
    	gl_Position = projection * temp * vec4(a_position, 1.0);
    }
    ";

    private string FragmentShaderSource =>
@"uniform sampler2D texture0;

    varying vec3 fragPosIn;
    varying vec3 normalIn;
    varying vec2 texIn;
    //DECLAREGLFRAG
    void main()
    {
        vec3 lightColor = vec3(1.0, 1.0, 1.0);
        float ambientStrength = 0.1;
        vec3 lightPos = vec3(0, 1, 5);
        
        vec3 ambient = ambientStrength * lightColor;

        vec3 norm = normalize(normalIn);
        vec3 lightDir = normalize(lightPos - fragPosIn);
        float diff = max(dot(norm, lightDir), 0.0);
        vec3 diffuse = diff * lightColor;

        vec3 result = (ambient + diffuse);
        gl_FragColor = texture2D(texture0, texIn) * vec4(result, 1.0);
        //gl_FragColor = texture2D(texture0, texIn);
    }
    ";

    private bool HaveCape = false;
    private bool SwitchModel = false;
    private bool SwitchSkin = false;
    private bool TopDisplay = true;
    private bool CapeDisplay = true;
    private bool Animation = true;

    private float Dis = 1;
    private Vector2 RotXY;
    private Vector2 DiffXY;

    private Vector2 XY;
    private Vector2 SaveXY;
    private Vector2 LastXY;

    private int texture;
    private int texture1;
    private int steveModelDrawOrder;

    private int _vertexShader;
    private int _fragmentShader;
    private int _shaderProgram;

    public SkinType SteveModelType { get; private set; }
    public bool HaveSkin { get; private set; } = false;

    public Vector3 ArmRotate;
    public Vector3 LegRotate;
    public Vector3 HeadRotate;

    public string Info = "";

    private readonly ModelVAO NormalVAO = new();
    private readonly ModelVAO TopVAO = new();

    private readonly SkinAnimation skina;

    private delegate void GlFunc1(int v1, int v2);
    private delegate void GlFunc2(int v1);
    private delegate void GlFunc3(float v1);
    private delegate void GlFunc4(bool v1);
    private delegate void GlFunc6(int v1, float v2, float v3, float v4);

    private GlFunc2 glDepthFunc;
    private GlFunc1 glBlendFunc;
    private GlFunc4 glDepthMask;
    private GlFunc2 glDisable;
    private GlFunc2 glDisableVertexAttribArray;
    private GlFunc6 glUniform3f;
    private GlFunc2 glCullFace;

    public SkinRender()
    {
        skina = new(this);
        skina.Start();
    }

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

    public void Rot(float x, float y)
    {
        RotXY.X += x;
        RotXY.Y += y;
    }

    public void Pos(float x, float y)
    {
        XY.X += x;
        XY.Y += y;
    }

    public void AddDis(float x)
    {
        Dis += x;
    }

    protected override unsafe void OnOpenGlInit(GlInterface gl)
    {
        CheckError(gl);

        IntPtr temp = gl.GetProcAddress("glDepthFunc");
        glDepthFunc = (GlFunc2)Marshal.GetDelegateForFunctionPointer(temp, typeof(GlFunc2));
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
        temp = gl.GetProcAddress("glUniform3f");
        glUniform3f = (GlFunc6)Marshal.GetDelegateForFunctionPointer(temp, typeof(GlFunc6));

        gl.ClearColor(0, 0, 0, 1);
        //GL_BLEND
        gl.Enable(0x0BE2); 
        //GL_SRC_ALPHA GL_ONE_MINUS_SRC_ALPHA
        glBlendFunc(0x0302, 0x0303);
        //GL_BACK
        glCullFace(0x0405);

        CheckError(gl);

        Info = $"Renderer: {gl.GetString(GlConsts.GL_RENDERER)} Version: {gl.GetString(GlConsts.GL_VERSION)}";

        _vertexShader = gl.CreateShader(GlConsts.GL_VERTEX_SHADER);
        var smg = gl.CompileShaderAndGetError(_vertexShader, GetShader(false, VertexShaderSource));
        if (smg != null)
        {
            App.ShowError(App.GetLanguage("SkinWindow.Error2"),
                    new Exception($"GlConsts.GL_VERTEX_SHADER.\n{smg}"));
        }

        _fragmentShader = gl.CreateShader(GlConsts.GL_FRAGMENT_SHADER);
        smg = gl.CompileShaderAndGetError(_fragmentShader, GetShader(true, FragmentShaderSource));
        if (smg != null)
        {
            App.ShowError(App.GetLanguage("SkinWindow.Error2"),
                    new Exception($"GlConsts.GL_FRAGMENT_SHADER.\n{smg}"));
        }

        _shaderProgram = gl.CreateProgram();
        gl.AttachShader(_shaderProgram, _vertexShader);
        gl.AttachShader(_shaderProgram, _fragmentShader);

        smg = gl.LinkProgramAndGetError(_shaderProgram);
        if (smg != null)
        {
            App.ShowError(App.GetLanguage("SkinWindow.Error1"), new Exception(smg));
        }

        InitVAO(gl, NormalVAO);
        InitVAO(gl, TopVAO);

        texture = gl.GenTexture();
        texture1 = gl.GenTexture();

        CheckError(gl);

        PointerWheelChanged += OpenGlPageControl_PointerWheelChanged;
        PointerPressed += OpenGlPageControl_PointerPressed;
        PointerReleased += OpenGlPageControl_PointerReleased;
        PointerMoved += OpenGlPageControl_PointerMoved;

        LoadSkin(gl);

        var window = this.FindTop<SkinControl>();
        if (window != null)
        {
            Dispatcher.UIThread.Post(() =>
            {
                window.Skin_Loaded();
            });
        }
    }

    private static void InitVAOItem(GlInterface gl, VAOItem item)
    {
        item.VertexBufferObject = gl.GenBuffer();
        item.IndexBufferObject = gl.GenBuffer();
    }

    private static void InitVAO(GlInterface gl, ModelVAO vao)
    {
        vao.Head.VertexArrayObject = gl.GenVertexArray();
        vao.Body.VertexArrayObject = gl.GenVertexArray();
        vao.LeftArm.VertexArrayObject = gl.GenVertexArray();
        vao.RightArm.VertexArrayObject = gl.GenVertexArray();
        vao.LeftLeg.VertexArrayObject = gl.GenVertexArray();
        vao.RightLeg.VertexArrayObject = gl.GenVertexArray();
        vao.Cape.VertexArrayObject = gl.GenVertexArray();

        InitVAOItem(gl, vao.Head);
        InitVAOItem(gl, vao.Body);
        InitVAOItem(gl, vao.LeftArm);
        InitVAOItem(gl, vao.RightArm);
        InitVAOItem(gl, vao.LeftLeg);
        InitVAOItem(gl, vao.RightLeg);
        InitVAOItem(gl, vao.Cape);
    }

    public void ChangeType(int index)
    {
        SteveModelType = (SkinType)index;

        SwitchModel = true;

        RequestNextFrameRendering();
    }

    public void ChangeSkin()
    {
        SwitchSkin = true;

        RequestNextFrameRendering();
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

        RequestNextFrameRendering();
    }

    private static unsafe void LoadTex(GlInterface gl, Image<Rgba32> image, int tex)
    {
        gl.ActiveTexture(GlConsts.GL_TEXTURE0);
        gl.BindTexture(GlConsts.GL_TEXTURE_2D, tex);

        gl.TexParameteri(
            GlConsts.GL_TEXTURE_2D,
            GlConsts.GL_TEXTURE_MIN_FILTER, GlConsts.GL_LINEAR
        );
        gl.TexParameteri(
            GlConsts.GL_TEXTURE_2D,
            GlConsts.GL_TEXTURE_MAG_FILTER, GlConsts.GL_NEAREST
        );
        gl.TexParameteri(
            GlConsts.GL_TEXTURE_2D,
            //GlConsts.GL_TEXTURE_WRAP_S GL_CLAMP_TO_EDGE
            0x2802, 0x812F
        );
        gl.TexParameteri(
            GlConsts.GL_TEXTURE_2D,
            //GlConsts.GL_TEXTURE_WRAP_T, GlConsts.GL_CLAMP_TO_EDGE
            0x2803, 0x812F
        );

        var pixels = new byte[4 * image.Width * image.Height];
        image.CopyPixelDataTo(pixels);

        fixed (byte* prt = pixels)
        {
            gl.TexImage2D(GlConsts.GL_TEXTURE_2D, 0, GlConsts.GL_RGBA, image.Width,
               image.Height, 0, GlConsts.GL_RGBA, GlConsts.GL_UNSIGNED_BYTE, (IntPtr)prt);
        }

        gl.BindTexture(GlConsts.GL_TEXTURE_2D, 0);
    }

    private void LoadSkin(GlInterface gl)
    {
        var window = App.FindRoot(VisualRoot);
        if (UserBinding.SkinImage == null)
        {
            HaveSkin = false;
            Dispatcher.UIThread.Post(() =>
            {
                (window.Con as SkinControl)!.Label1.IsVisible = true;
                (window.Con as SkinControl)!.Label1.Content = App.GetLanguage("SkinWindow.Info2");
            });
            return;
        }

        SteveModelType = SkinUtil.GetTextType(UserBinding.SkinImage);
        if (SteveModelType == SkinType.Unkonw)
        {
            HaveSkin = false;
            Dispatcher.UIThread.Post(() =>
            {
                (window.Con as SkinControl)!.Label1.IsVisible = true;
                (window.Con as SkinControl)!.Label1.Content = App.GetLanguage("SkinWindow.Info3");
            });
            return;
        }
        LoadTex(gl, UserBinding.SkinImage, texture);
        if (UserBinding.CapeIamge != null)
        {
            LoadTex(gl, UserBinding.CapeIamge, texture1);
            HaveCape = true;
        }
        else
        {
            HaveCape = false;
        }

        CheckError(gl);

        LoadModel(gl);

        HaveSkin = true;
        Dispatcher.UIThread.Post(() =>
        {
            (window.Con as SkinControl)!.ComboBox1.SelectedIndex = (int)SteveModelType;
            (window.Con as SkinControl)!.Label1.IsVisible = false;
        });
    }

    private unsafe void PutVAO(GlInterface gl, VAOItem vao, ModelItem model, float[] uv)
    {
        float[] vertices =
        {
            0.0f,  0.0f, -1.0f,
            0.0f,  0.0f, -1.0f,
            0.0f,  0.0f, -1.0f,
            0.0f,  0.0f, -1.0f,

            0.0f,  0.0f,  1.0f,
            0.0f,  0.0f,  1.0f,
            0.0f,  0.0f,  1.0f,
            0.0f,  0.0f,  1.0f,

            -1.0f,  0.0f,  0.0f,
            -1.0f,  0.0f,  0.0f,
            -1.0f,  0.0f,  0.0f,
            -1.0f,  0.0f,  0.0f,

            1.0f,  0.0f,  0.0f,
            1.0f,  0.0f,  0.0f,
            1.0f,  0.0f,  0.0f,
            1.0f,  0.0f,  0.0f,

            0.0f,  1.0f,  0.0f,
            0.0f,  1.0f,  0.0f,
            0.0f,  1.0f,  0.0f,
            0.0f,  1.0f,  0.0f,

            0.0f, -1.0f,  0.0f,
            0.0f, -1.0f,  0.0f,
            0.0f, -1.0f,  0.0f,
            0.0f, -1.0f,  0.0f,
        };

        gl.UseProgram(_shaderProgram);
        gl.BindVertexArray(vao.VertexArrayObject);

        int a_Position = gl.GetAttribLocationString(_shaderProgram, "a_position");
        int a_texCoord = gl.GetAttribLocationString(_shaderProgram, "a_texCoord");
        int a_normal = gl.GetAttribLocationString(_shaderProgram, "a_normal");

        glDisableVertexAttribArray(a_Position);
        glDisableVertexAttribArray(a_texCoord);
        glDisableVertexAttribArray(a_normal);

        int size = model.Model.Length / 3;

        var points = new Vertex[size];

        for (var primitive = 0; primitive < size; primitive++)
        {
            var srci = primitive * 3;
            var srci1 = primitive * 2;
            points[primitive] = new Vertex
            {
                Position = new(model.Model[srci], model.Model[srci + 1], model.Model[srci + 2]),
                UV = new(uv[srci1], uv[srci1 + 1]),
                Normal = new(vertices[srci], vertices[srci + 1], vertices[srci + 2])
            };
        }

        gl.BindBuffer(GlConsts.GL_ARRAY_BUFFER, vao.VertexBufferObject);
        var vertexSize = Marshal.SizeOf<Vertex>();
        fixed (void* pdata = points)
        {
            gl.BufferData(GlConsts.GL_ARRAY_BUFFER, points.Length * vertexSize,
                    new IntPtr(pdata), GlConsts.GL_STATIC_DRAW);
        }

        gl.BindBuffer(GlConsts.GL_ELEMENT_ARRAY_BUFFER, vao.IndexBufferObject);
        fixed (void* pdata = model.Point)
        {
            gl.BufferData(GlConsts.GL_ELEMENT_ARRAY_BUFFER,
                model.Point.Length * sizeof(ushort), new IntPtr(pdata), GlConsts.GL_STATIC_DRAW);
        }

        gl.VertexAttribPointer(a_Position, 3, GlConsts.GL_FLOAT,
            0, 8 * sizeof(float), 0);
        gl.VertexAttribPointer(a_texCoord, 2, GlConsts.GL_FLOAT,
            0, 8 * sizeof(float), 3 * sizeof(float));
        gl.VertexAttribPointer(a_normal, 3, GlConsts.GL_FLOAT,
            0, 8 * sizeof(float), 5 * sizeof(float));

        gl.EnableVertexAttribArray(a_Position);
        gl.EnableVertexAttribArray(a_texCoord);
        gl.EnableVertexAttribArray(a_normal);

        gl.BindVertexArray(0);

        CheckError(gl);
    }

    private unsafe void LoadModel(GlInterface gl)
    {
        var steve = new Steve3DModel();
        var stevetex = new Steve3DTexture();

        var normal = steve.GetSteve(SteveModelType);
        var top = steve.GetSteveTop(SteveModelType);
        var tex = stevetex.GetSteveTexture(SteveModelType);
        var textop = stevetex.GetSteveTextureTop(SteveModelType);

        steveModelDrawOrder = normal.Head.Point.Length;

        PutVAO(gl, NormalVAO.Head, normal.Head, tex.Head);
        PutVAO(gl, NormalVAO.Body, normal.Body, tex.Body);
        PutVAO(gl, NormalVAO.LeftArm, normal.LeftArm, tex.LeftArm);
        PutVAO(gl, NormalVAO.RightArm, normal.RightArm, tex.RightArm);
        PutVAO(gl, NormalVAO.LeftLeg, normal.LeftLeg, tex.LeftLeg);
        PutVAO(gl, NormalVAO.RightLeg, normal.RightLeg, tex.RightLeg);
        PutVAO(gl, NormalVAO.Cape, normal.Cape, tex.Cape);

        PutVAO(gl, TopVAO.Head, top.Head, textop.Head);
        if (SteveModelType != SkinType.Old)
        {
            PutVAO(gl, TopVAO.Head, top.Head, textop.Head);
            PutVAO(gl, TopVAO.Body, top.Body, textop.Body);
            PutVAO(gl, TopVAO.LeftArm, top.LeftArm, textop.LeftArm);
            PutVAO(gl, TopVAO.RightArm, top.RightArm, textop.RightArm);
            PutVAO(gl, TopVAO.LeftLeg, top.LeftLeg, textop.LeftLeg);
            PutVAO(gl, TopVAO.RightLeg, top.RightLeg, textop.RightLeg);
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

        RequestNextFrameRendering();
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

        RequestNextFrameRendering();
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

        RequestNextFrameRendering();
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

        RequestNextFrameRendering();
    }

    private unsafe void DrawCape(GlInterface gl)
    {
        if (HaveCape && CapeDisplay)
        {
            gl.BindTexture(GlConsts.GL_TEXTURE_2D, texture1);

            var modelLoc = gl.GetUniformLocationString(_shaderProgram, "self");

            var model = Matrix4x4.CreateTranslation(0, -2f * CubeC.Value, -CubeC.Value * 0.1f) *
               Matrix4x4.CreateRotationX((float)(10.8 * Math.PI / 180)) *
               Matrix4x4.CreateTranslation(0, 1.6f * CubeC.Value,
               -CubeC.Value * 0.5f);
            gl.UniformMatrix4fv(modelLoc, 1, false, &model);

            gl.BindVertexArray(NormalVAO.Cape.VertexArrayObject);
            gl.DrawElements(GlConsts.GL_TRIANGLES, steveModelDrawOrder,
                GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);

            gl.BindTexture(GlConsts.GL_TEXTURE_2D, 0);
        }
    }

    private unsafe void DrawNormal(GlInterface GL)
    {
        GL.BindTexture(GlConsts.GL_TEXTURE_2D, texture);

        var modelLoc = GL.GetUniformLocationString(_shaderProgram, "self");
        var model = Matrix4x4.Identity;
        GL.UniformMatrix4fv(modelLoc,1, false, &model);

        GL.BindVertexArray(NormalVAO.Body.VertexArrayObject);
        GL.DrawElements(GlConsts.GL_TRIANGLES, steveModelDrawOrder,
            GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);

        model = Matrix4x4.CreateTranslation(0, CubeC.Value, 0) *
               Matrix4x4.CreateRotationZ((Animation ? skina.Head.X : HeadRotate.X) / 360) *
               Matrix4x4.CreateRotationX((Animation ? skina.Head.Y : HeadRotate.Y) / 360) *
               Matrix4x4.CreateRotationY((Animation ? skina.Head.Z : HeadRotate.Z) / 360) *
               Matrix4x4.CreateTranslation(0, CubeC.Value * 1.5f, 0);
        GL.UniformMatrix4fv(modelLoc, 1, false, &model);

        GL.BindVertexArray(NormalVAO.Head.VertexArrayObject);
        GL.DrawElements(GlConsts.GL_TRIANGLES, steveModelDrawOrder,
                 GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);

        if (SteveModelType == SkinType.NewSlim)
        {
            model = Matrix4x4.CreateTranslation(CubeC.Value / 2, -(1.375f * CubeC.Value), 0) *
                Matrix4x4.CreateRotationZ((Animation ? skina.Arm.X : ArmRotate.X) / 360) *
                Matrix4x4.CreateRotationX((Animation ? skina.Arm.Y : ArmRotate.Y) / 360) *
                Matrix4x4.CreateTranslation(
                    (1.375f * CubeC.Value) - (CubeC.Value / 2), 1.375f * CubeC.Value, 0);
        }
        else
        {
            model = Matrix4x4.CreateTranslation(CubeC.Value / 2, -(1.5f * CubeC.Value), 0) *
                Matrix4x4.CreateRotationZ((Animation ? skina.Arm.X : ArmRotate.X) / 360) *
                Matrix4x4.CreateRotationX((Animation ? skina.Arm.Y : ArmRotate.Y) / 360) *
                Matrix4x4.CreateTranslation(
                    (1.5f * CubeC.Value) - (CubeC.Value / 2), 1.5f * CubeC.Value, 0);
        }
        GL.UniformMatrix4fv(modelLoc, 1, false, &model);

        GL.BindVertexArray(NormalVAO.LeftArm.VertexArrayObject);
        GL.DrawElements(GlConsts.GL_TRIANGLES, steveModelDrawOrder,
                GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);

        if (SteveModelType == SkinType.NewSlim)
        {
            model = Matrix4x4.CreateTranslation(-CubeC.Value / 2, -(1.375f * CubeC.Value), 0) *
                Matrix4x4.CreateRotationZ((Animation ? -skina.Arm.X : -ArmRotate.X) / 360) *
                Matrix4x4.CreateRotationX((Animation ? -skina.Arm.Y : -ArmRotate.Y) / 360) *
                Matrix4x4.CreateTranslation(
                    (-1.375f * CubeC.Value) + (CubeC.Value / 2), 1.375f * CubeC.Value, 0);
        }
        else
        {
            model = Matrix4x4.CreateTranslation(-CubeC.Value / 2, -(1.5f * CubeC.Value), 0) *
                Matrix4x4.CreateRotationZ((Animation ? -skina.Arm.X : -ArmRotate.X) / 360) *
                Matrix4x4.CreateRotationX((Animation ? -skina.Arm.Y : -ArmRotate.Y) / 360) *
                Matrix4x4.CreateTranslation(
                    (-1.5f * CubeC.Value) + (CubeC.Value / 2), 1.5f * CubeC.Value, 0);
        }
        GL.UniformMatrix4fv(modelLoc, 1, false, &model);

        GL.BindVertexArray(NormalVAO.RightArm.VertexArrayObject);
        GL.DrawElements(GlConsts.GL_TRIANGLES, steveModelDrawOrder,
                 GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);

        model = Matrix4x4.CreateTranslation(0, -1.5f * CubeC.Value, 0) *
               Matrix4x4.CreateRotationZ((Animation ? skina.Leg.X : LegRotate.X) / 360) *
               Matrix4x4.CreateRotationX((Animation ? skina.Leg.Y : LegRotate.Y) / 360) *
               Matrix4x4.CreateTranslation(CubeC.Value * 0.5f, -CubeC.Value * 1.5f, 0);
        GL.UniformMatrix4fv(modelLoc, 1, false, &model);

        GL.BindVertexArray(NormalVAO.LeftLeg.VertexArrayObject);
        GL.DrawElements(GlConsts.GL_TRIANGLES, steveModelDrawOrder,
                GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);

        model = Matrix4x4.CreateTranslation(0, -1.5f * CubeC.Value, 0) *
               Matrix4x4.CreateRotationZ((Animation ? -skina.Leg.X : -LegRotate.X) / 360) *
               Matrix4x4.CreateRotationX((Animation ? -skina.Leg.Y : -LegRotate.Y) / 360) *
               Matrix4x4.CreateTranslation(-CubeC.Value * 0.5f, -CubeC.Value * 1.5f, 0);
        GL.UniformMatrix4fv(modelLoc, 1, false, &model);

        GL.BindVertexArray(NormalVAO.RightLeg.VertexArrayObject);
        GL.DrawElements(GlConsts.GL_TRIANGLES, steveModelDrawOrder,
                GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);

        GL.BindTexture(GlConsts.GL_TEXTURE_2D, 0);
    }

    private unsafe void DrawTop(GlInterface gl)
    {
        gl.BindTexture(GlConsts.GL_TEXTURE_2D, texture);

        var modelLoc = gl.GetUniformLocationString(_shaderProgram, "self");
        var model = Matrix4x4.Identity;
        gl.UniformMatrix4fv(modelLoc, 1, false, &model);

        gl.BindVertexArray(TopVAO.Body.VertexArrayObject);
        gl.DrawElements(GlConsts.GL_TRIANGLES, steveModelDrawOrder,
                 GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);

        model = Matrix4x4.CreateTranslation(0, CubeC.Value, 0) *
               Matrix4x4.CreateRotationZ((Animation ? skina.Head.X : HeadRotate.X) / 360) *
               Matrix4x4.CreateRotationX((Animation ? skina.Head.Y : HeadRotate.Y) / 360) *
               Matrix4x4.CreateRotationY((Animation ? skina.Head.Z : HeadRotate.Z) / 360) *
               Matrix4x4.CreateTranslation(0, CubeC.Value * 1.5f, 0);
        gl.UniformMatrix4fv(modelLoc, 1, false, &model);

        gl.BindVertexArray(TopVAO.Head.VertexArrayObject);
        gl.DrawElements(GlConsts.GL_TRIANGLES, steveModelDrawOrder,
                 GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);

        if (SteveModelType == SkinType.NewSlim)
        {
            model = Matrix4x4.CreateTranslation(CubeC.Value / 2, -(1.375f * CubeC.Value), 0) *
                Matrix4x4.CreateRotationZ((Animation ? skina.Arm.X : ArmRotate.X) / 360) *
                Matrix4x4.CreateRotationX((Animation ? skina.Arm.Y : ArmRotate.Y) / 360) *
                Matrix4x4.CreateTranslation(
                    (1.375f * CubeC.Value) - (CubeC.Value / 2), 1.375f * CubeC.Value, 0);
        }
        else
        {
            model = Matrix4x4.CreateTranslation(CubeC.Value / 2, -(1.5f * CubeC.Value), 0) *
                Matrix4x4.CreateRotationZ((Animation ? skina.Arm.X : ArmRotate.X) / 360) *
                Matrix4x4.CreateRotationX((Animation ? skina.Arm.Y : ArmRotate.Y) / 360) *
                Matrix4x4.CreateTranslation(
                    (1.5f * CubeC.Value) - (CubeC.Value / 2), 1.5f * CubeC.Value, 0);
        }
        gl.UniformMatrix4fv(modelLoc, 1, false, &model);

        gl.BindVertexArray(TopVAO.LeftArm.VertexArrayObject);
        gl.DrawElements(GlConsts.GL_TRIANGLES, steveModelDrawOrder,
                GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);

        if (SteveModelType == SkinType.NewSlim)
        {
            model = Matrix4x4.CreateTranslation(-CubeC.Value / 2, -(1.375f * CubeC.Value), 0) *
                Matrix4x4.CreateRotationZ((Animation ? -skina.Arm.X : -ArmRotate.X) / 360) *
                Matrix4x4.CreateRotationX((Animation ? -skina.Arm.Y : -ArmRotate.Y) / 360) *
                Matrix4x4.CreateTranslation(
                    (-1.375f * CubeC.Value) + (CubeC.Value / 2), 1.375f * CubeC.Value, 0);
        }
        else
        {
            model = Matrix4x4.CreateTranslation(-CubeC.Value / 2, -(1.5f * CubeC.Value), 0) *
                Matrix4x4.CreateRotationZ((Animation ? -skina.Arm.X : -ArmRotate.X) / 360) *
                Matrix4x4.CreateRotationX((Animation ? -skina.Arm.Y : -ArmRotate.Y) / 360) *
                Matrix4x4.CreateTranslation(
                    (-1.5f * CubeC.Value) + (CubeC.Value / 2), 1.5f * CubeC.Value, 0);
        }
        gl.UniformMatrix4fv(modelLoc, 1, false, &model);

        gl.BindVertexArray(TopVAO.RightArm.VertexArrayObject);
        gl.DrawElements(GlConsts.GL_TRIANGLES, steveModelDrawOrder,
                GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);

        model = Matrix4x4.CreateTranslation(0, -1.5f * CubeC.Value, 0) *
               Matrix4x4.CreateRotationZ((Animation ? skina.Leg.X : LegRotate.X) / 360) *
               Matrix4x4.CreateRotationX((Animation ? skina.Leg.Y : LegRotate.Y) / 360) *
               Matrix4x4.CreateTranslation(CubeC.Value * 0.5f, -CubeC.Value * 1.5f, 0);
        gl.UniformMatrix4fv(modelLoc, 1, false, &model);

        gl.BindVertexArray(TopVAO.LeftLeg.VertexArrayObject);
        gl.DrawElements(GlConsts.GL_TRIANGLES, steveModelDrawOrder,
                 GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);

        model = Matrix4x4.CreateTranslation(0, -1.5f * CubeC.Value, 0) *
               Matrix4x4.CreateRotationZ((Animation ? -skina.Leg.X : -LegRotate.X) / 360) *
               Matrix4x4.CreateRotationX((Animation ? -skina.Leg.Y : -LegRotate.Y) / 360) *
               Matrix4x4.CreateTranslation(-CubeC.Value * 0.5f, -CubeC.Value * 1.5f, 0);
        gl.UniformMatrix4fv(modelLoc, 1, false, &model);

        gl.BindVertexArray(TopVAO.RightLeg.VertexArrayObject);
        gl.DrawElements(GlConsts.GL_TRIANGLES, steveModelDrawOrder,
                GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);

        gl.BindTexture(GlConsts.GL_TEXTURE_2D, 0);
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

        gl.Viewport(0, 0, (int)Bounds.Width, (int)Bounds.Height);

        if (App.BackBitmap != null)
        {
            gl.ClearColor(0, 0, 0, 0.2f);
        }
        else
        {
            gl.ClearColor(0, 0, 0, 1);
        }

        gl.Clear(GlConsts.GL_COLOR_BUFFER_BIT | GlConsts.GL_DEPTH_BUFFER_BIT);

        CheckError(gl);
        //GL_CULL_FACE
        gl.Enable(0x0B44);
        gl.Enable(GlConsts.GL_DEPTH_TEST);
        glDepthMask(true);
        gl.ActiveTexture(GlConsts.GL_TEXTURE0);
        gl.UseProgram(_shaderProgram);
        CheckError(gl);

        var viewLoc = gl.GetUniformLocationString(_shaderProgram, "view");
        var projectionLoc = gl.GetUniformLocationString(_shaderProgram, "projection");
        var modelLoc = gl.GetUniformLocationString(_shaderProgram, "model");

        var projection = Matrix4x4.CreatePerspectiveFieldOfView(
            (float)(Math.PI / 4), (float)(Bounds.Width / Bounds.Height),
            0.001f, 1000);

        var view = Matrix4x4.CreateLookAt(new(0, 0, 7), new(), new(0, 1, 0));

        var model = Matrix4x4.CreateRotationX(RotXY.X / 360)
                    * Matrix4x4.CreateRotationY(RotXY.Y / 360)
                    * Matrix4x4.CreateTranslation(new(XY.X, XY.Y, 0))
                    * Matrix4x4.CreateScale(Dis);

        gl.UniformMatrix4fv(viewLoc, 1, false, &view);
        gl.UniformMatrix4fv(modelLoc, 1, false, &model);
        gl.UniformMatrix4fv(projectionLoc, 1, false, &projection);

        CheckError(gl);

        DrawNormal(gl);

        DrawCape(gl);

        if (TopDisplay)
        {
            //GL_BLEND
            gl.Enable(0x0BE2);
            //GL_SRC_ALPHA GL_ONE_MINUS_SRC_ALPHA
            glBlendFunc(0x0302, 0x0303);
            glDepthMask(false);

            DrawTop(gl);

            //GL_BLEND
            glDisable(0x0BE2);
            glDepthMask(true);
        }

        CheckError(gl);
    }

    private static void CheckError(GlInterface gl)
    {
        int err;
        while ((err = gl.GetError()) != GlConsts.GL_NO_ERROR)
            Console.WriteLine(err);
    }

    private static void DeleteVAOItem(GlInterface gl, VAOItem item)
    {
        gl.DeleteBuffer(item.VertexBufferObject);
        gl.DeleteBuffer(item.IndexBufferObject);
    }

    private static void DeleteVAO(GlInterface gl, ModelVAO vao)
    {
        gl.DeleteVertexArray(vao.Head.VertexArrayObject);
        gl.DeleteVertexArray(vao.Body.VertexArrayObject);
        gl.DeleteVertexArray(vao.LeftArm.VertexArrayObject);
        gl.DeleteVertexArray(vao.RightArm.VertexArrayObject);
        gl.DeleteVertexArray(vao.LeftLeg.VertexArrayObject);
        gl.DeleteVertexArray(vao.RightLeg.VertexArrayObject);

        DeleteVAOItem(gl, vao.Head);
        DeleteVAOItem(gl, vao.Body);
        DeleteVAOItem(gl, vao.LeftArm);
        DeleteVAOItem(gl, vao.RightArm);
        DeleteVAOItem(gl, vao.LeftLeg);
        DeleteVAOItem(gl, vao.RightLeg);
    }

    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        // Unbind everything
        gl.BindBuffer(GlConsts.GL_ARRAY_BUFFER, 0);
        gl.BindBuffer(GlConsts.GL_ELEMENT_ARRAY_BUFFER, 0);
        gl.BindVertexArray(0);
        gl.UseProgram(0);

        // Delete all resources.
        DeleteVAO(gl, NormalVAO);
        DeleteVAO(gl, TopVAO);

        gl.DeleteProgram(_shaderProgram);
        gl.DeleteShader(_fragmentShader);
        gl.DeleteShader(_vertexShader);
    }

    public void SetTopDisplay(bool value)
    {
        TopDisplay = value;

        RequestNextFrameRendering();
    }

    public void SetCapeDisplay(bool value)
    {
        CapeDisplay = value;

        RequestNextFrameRendering();
    }

    public void SetAnimation(bool value)
    {
        Animation = value;
        if (value)
        {
            skina.Start();
        }
        else
        {
            skina.Pause();
        }

        RequestNextFrameRendering();
    }
}