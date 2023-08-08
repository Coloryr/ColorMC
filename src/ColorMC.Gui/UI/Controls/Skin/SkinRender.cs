using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using ColorMC.Gui.Objs;
using ColorMC.Gui.PlayerModel;
using ColorMC.Gui.UI.Animations;
using ColorMC.Gui.UI.Model.Skin;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ColorMC.Gui.UI.Controls.Skin;

public class SkinRender : OpenGlControlBase
{
    private bool _haveCape = false;
    private bool _switchModel = false;
    private bool _switchSkin = false;

    private float _dis = 1;
    private Vector2 _rotXY;
    private Vector2 _diffXY;

    private Vector2 _xy;
    private Vector2 _saveXY;
    private Vector2 _lastXY;

    private Matrix4x4 _last;

    private int _texture;
    private int _texture1;
    private int _steveModelDrawOrder;

    private int _vertexShader;
    private int _fragmentShader;
    private int _shaderProgram;

    private readonly ModelVAO _normalVAO = new();
    private readonly ModelVAO _topVAO = new();

    private readonly SkinAnimation _skina;

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

    public SkinModel _model;

    public SkinRender()
    {
        _skina = new(this);
        _last = Matrix4x4.Identity;
    }

    public void SetModel(SkinModel model)
    {
        _model = model;
        model.PropertyChanged += Model_PropertyChanged;
        _skina.Run = true;
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "SteveModelType")
        {
            Dispatcher.UIThread.Post(ChangeType);
        }
        else if (e.PropertyName == "EnableAnimation")
        {
            Dispatcher.UIThread.Post(SetAnimation);
        }
        else if (e.PropertyName == "EnableCape")
        {
            Dispatcher.UIThread.Post(RequestNextFrameRendering);
        }
        else if (e.PropertyName == "EnableTop")
        {
            Dispatcher.UIThread.Post(RequestNextFrameRendering);
        }
        else if (e.PropertyName == "Rotate")
        {
            Dispatcher.UIThread.Post(RequestNextFrameRendering);
        }
        else if (e.PropertyName == "Pos")
        {
            _xy.X += _model.X;
            _xy.Y += _model.Y;
        }
        else if (e.PropertyName == "Dis")
        {
            _dis += _model.X;
        }
        else if (e.PropertyName == "Rot")
        {
            _rotXY.X += _model.X;
            _rotXY.Y += _model.Y;
        }
    }

    public void Rot(float x, float y)
    {
        _rotXY.X += x;
        _rotXY.Y += y;
    }

    public void Pos(float x, float y)
    {
        _xy.X += x;
        _xy.Y += y;
    }

    public void AddDis(float x)
    {
        _dis += x;
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

        _model.Info = $"Renderer: {gl.GetString(GlConsts.GL_RENDERER)} Version: {gl.GetString(GlConsts.GL_VERSION)}";

        _vertexShader = gl.CreateShader(GlConsts.GL_VERTEX_SHADER);
        var smg = gl.CompileShaderAndGetError(_vertexShader, SkinShader.VertexShader(GlVersion, false));
        if (smg != null)
        {
            App.ShowError(App.GetLanguage("SkinWindow.Error2"),
                    new Exception($"GlConsts.GL_VERTEX_SHADER.\n{smg}"));
        }

        _fragmentShader = gl.CreateShader(GlConsts.GL_FRAGMENT_SHADER);
        smg = gl.CompileShaderAndGetError(_fragmentShader, SkinShader.VertexShader(GlVersion, true));
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

        InitVAO(gl, _normalVAO);
        InitVAO(gl, _topVAO);

        _texture = gl.GenTexture();
        _texture1 = gl.GenTexture();

        CheckError(gl);

        PointerWheelChanged += OpenGlPageControl_PointerWheelChanged;
        PointerPressed += OpenGlPageControl_PointerPressed;
        PointerReleased += OpenGlPageControl_PointerReleased;
        PointerMoved += OpenGlPageControl_PointerMoved;

        LoadSkin(gl);

        _model.IsLoad = true;
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

    public void ChangeType()
    {
        _switchModel = true;

        RequestNextFrameRendering();
    }

    public void ChangeSkin()
    {
        _switchSkin = true;

        RequestNextFrameRendering();
    }

    public void Reset()
    {
        _dis = 1;
        _diffXY.X = 0;
        _diffXY.Y = 0;
        _xy.X = 0;
        _xy.Y = 0;
        _saveXY.X = 0;
        _saveXY.Y = 0;
        _lastXY.X = 0;
        _lastXY.Y = 0;
        _last = Matrix4x4.Identity;

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
        _model.IsLoading = true;
        if (UserBinding.SkinImage == null)
        {
            _model.HaveSkin = false;
            _model.Text = App.GetLanguage("SkinWindow.Info2");
            return;
        }

        _model.SteveModelType = SkinUtil.GetTextType(UserBinding.SkinImage);
        if (_model.SteveModelType == SkinType.Unkonw)
        {
            _model.HaveSkin = false;
            _model.Text = App.GetLanguage("SkinWindow.Info3");
            return;
        }
        LoadTex(gl, UserBinding.SkinImage, _texture);
        if (UserBinding.CapeIamge != null)
        {
            LoadTex(gl, UserBinding.CapeIamge, _texture1);
            _haveCape = true;
        }
        else
        {
            _haveCape = false;
        }

        CheckError(gl);

        LoadModel(gl);

        _model.HaveSkin = true;

        _model.Type = (int)_model.SteveModelType;
        _model.IsLoading = false;
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

        gl.VertexAttribPointer(a_Position, 3, GlConsts.GL_FLOAT, 0, 8 * sizeof(float), 0);
        gl.VertexAttribPointer(a_texCoord, 2, GlConsts.GL_FLOAT, 0, 8 * sizeof(float), 3 * sizeof(float));
        gl.VertexAttribPointer(a_normal, 3, GlConsts.GL_FLOAT, 0, 8 * sizeof(float), 5 * sizeof(float));

        gl.EnableVertexAttribArray(a_Position);
        gl.EnableVertexAttribArray(a_texCoord);
        gl.EnableVertexAttribArray(a_normal);

        gl.BindVertexArray(0);

        CheckError(gl);
    }

    private unsafe void LoadModel(GlInterface gl)
    {
        _model.IsLoading = true;
        var steve = new Steve3DModel();
        var stevetex = new Steve3DTexture();

        var normal = steve.GetSteve(_model.SteveModelType);
        var top = steve.GetSteveTop(_model.SteveModelType);
        var tex = stevetex.GetSteveTexture(_model.SteveModelType);
        var textop = stevetex.GetSteveTextureTop(_model.SteveModelType);

        _steveModelDrawOrder = normal.Head.Point.Length;

        PutVAO(gl, _normalVAO.Head, normal.Head, tex.Head);
        PutVAO(gl, _normalVAO.Body, normal.Body, tex.Body);
        PutVAO(gl, _normalVAO.LeftArm, normal.LeftArm, tex.LeftArm);
        PutVAO(gl, _normalVAO.RightArm, normal.RightArm, tex.RightArm);
        PutVAO(gl, _normalVAO.LeftLeg, normal.LeftLeg, tex.LeftLeg);
        PutVAO(gl, _normalVAO.RightLeg, normal.RightLeg, tex.RightLeg);
        PutVAO(gl, _normalVAO.Cape, normal.Cape, tex.Cape);

        PutVAO(gl, _topVAO.Head, top.Head, textop.Head);
        if (_model.SteveModelType != SkinType.Old)
        {
            PutVAO(gl, _topVAO.Head, top.Head, textop.Head);
            PutVAO(gl, _topVAO.Body, top.Body, textop.Body);
            PutVAO(gl, _topVAO.LeftArm, top.LeftArm, textop.LeftArm);
            PutVAO(gl, _topVAO.RightArm, top.RightArm, textop.RightArm);
            PutVAO(gl, _topVAO.LeftLeg, top.LeftLeg, textop.LeftLeg);
            PutVAO(gl, _topVAO.RightLeg, top.RightLeg, textop.RightLeg);
        }
        _model.IsLoading = false;
    }

    private void OpenGlPageControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!_model.HaveSkin)
            return;

        var po = e.GetCurrentPoint(this);
        var pos = e.GetPosition(this);

        if (po.Properties.IsLeftButtonPressed)
        {
            _diffXY.X = (float)pos.X - _rotXY.Y;
            _diffXY.Y = -(float)pos.Y + _rotXY.X;
        }
        else if (po.Properties.IsRightButtonPressed)
        {
            _lastXY.X = (float)pos.X;
            _lastXY.Y = (float)pos.Y;
        }

        RequestNextFrameRendering();
    }

    private void OpenGlPageControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (!_model.HaveSkin)
            return;

        if (e.InitialPressMouseButton == MouseButton.Right)
        {
            _saveXY.X = _xy.X;
            _saveXY.Y = _xy.Y;
        }

        RequestNextFrameRendering();
    }

    private void OpenGlPageControl_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_model.HaveSkin)
            return;

        var po = e.GetCurrentPoint(this);

        if (po.Properties.IsLeftButtonPressed)
        {
            var point = e.GetPosition(this);
            _rotXY.Y = (float)point.X - _diffXY.X;
            _rotXY.X = (float)point.Y + _diffXY.Y;
        }
        else if (po.Properties.IsRightButtonPressed)
        {
            var point = e.GetPosition(this);
            _xy.X = (-(_lastXY.X - (float)point.X) / 100) + _saveXY.X;
            _xy.Y = ((_lastXY.Y - (float)point.Y) / 100) + _saveXY.Y;
        }

        RequestNextFrameRendering();
    }

    private void OpenGlPageControl_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (!_model.HaveSkin)
            return;

        if (e.Delta.Y > 0)
        {
            _dis += 0.1f;
        }
        else if (e.Delta.Y < 0)
        {
            _dis -= 0.1f;
        }

        RequestNextFrameRendering();
    }

    private unsafe void DrawCape(GlInterface gl)
    {
        if (_haveCape && _model.EnableCape)
        {
            gl.BindTexture(GlConsts.GL_TEXTURE_2D, _texture1);

            var modelLoc = gl.GetUniformLocationString(_shaderProgram, "self");

            var model = Matrix4x4.CreateTranslation(0, -2f * CubeC.Value, -CubeC.Value * 0.1f) *
               Matrix4x4.CreateRotationX((float)(10.8 * Math.PI / 180)) *
               Matrix4x4.CreateTranslation(0, 1.6f * CubeC.Value,
               -CubeC.Value * 0.5f);
            gl.UniformMatrix4fv(modelLoc, 1, false, &model);

            gl.BindVertexArray(_normalVAO.Cape.VertexArrayObject);
            gl.DrawElements(GlConsts.GL_TRIANGLES, _steveModelDrawOrder,
                GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);

            gl.BindTexture(GlConsts.GL_TEXTURE_2D, 0);
        }
    }

    private unsafe void DrawNormal(GlInterface GL)
    {
        GL.BindTexture(GlConsts.GL_TEXTURE_2D, _texture);

        var modelLoc = GL.GetUniformLocationString(_shaderProgram, "self");
        var modelMat = Matrix4x4.Identity;
        GL.UniformMatrix4fv(modelLoc, 1, false, &modelMat);

        GL.BindVertexArray(_normalVAO.Body.VertexArrayObject);
        GL.DrawElements(GlConsts.GL_TRIANGLES, _steveModelDrawOrder,
            GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);

        bool enable = _model.EnableAnimation;

        modelMat = Matrix4x4.CreateTranslation(0, CubeC.Value, 0) *
               Matrix4x4.CreateRotationZ((enable ? _skina.Head.X : _model.HeadRotate.X) / 360) *
               Matrix4x4.CreateRotationX((enable ? _skina.Head.Y : _model.HeadRotate.Y) / 360) *
               Matrix4x4.CreateRotationY((enable ? _skina.Head.Z : _model.HeadRotate.Z) / 360) *
               Matrix4x4.CreateTranslation(0, CubeC.Value * 1.5f, 0);
        GL.UniformMatrix4fv(modelLoc, 1, false, &modelMat);

        GL.BindVertexArray(_normalVAO.Head.VertexArrayObject);
        GL.DrawElements(GlConsts.GL_TRIANGLES, _steveModelDrawOrder,
                 GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);

        if (_model.SteveModelType == SkinType.NewSlim)
        {
            modelMat = Matrix4x4.CreateTranslation(CubeC.Value / 2, -(1.375f * CubeC.Value), 0) *
                Matrix4x4.CreateRotationZ((enable ? _skina.Arm.X : _model.ArmRotate.X) / 360) *
                Matrix4x4.CreateRotationX((enable ? _skina.Arm.Y : _model.ArmRotate.Y) / 360) *
                Matrix4x4.CreateTranslation(
                    (1.375f * CubeC.Value) - (CubeC.Value / 2), 1.375f * CubeC.Value, 0);
        }
        else
        {
            modelMat = Matrix4x4.CreateTranslation(CubeC.Value / 2, -(1.5f * CubeC.Value), 0) *
                Matrix4x4.CreateRotationZ((enable ? _skina.Arm.X : _model.ArmRotate.X) / 360) *
                Matrix4x4.CreateRotationX((enable ? _skina.Arm.Y : _model.ArmRotate.Y) / 360) *
                Matrix4x4.CreateTranslation(
                    (1.5f * CubeC.Value) - (CubeC.Value / 2), 1.5f * CubeC.Value, 0);
        }
        GL.UniformMatrix4fv(modelLoc, 1, false, &modelMat);

        GL.BindVertexArray(_normalVAO.LeftArm.VertexArrayObject);
        GL.DrawElements(GlConsts.GL_TRIANGLES, _steveModelDrawOrder,
                GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);

        if (_model.SteveModelType == SkinType.NewSlim)
        {
            modelMat = Matrix4x4.CreateTranslation(-CubeC.Value / 2, -(1.375f * CubeC.Value), 0) *
                Matrix4x4.CreateRotationZ((enable ? -_skina.Arm.X : -_model.ArmRotate.X) / 360) *
                Matrix4x4.CreateRotationX((enable ? -_skina.Arm.Y : -_model.ArmRotate.Y) / 360) *
                Matrix4x4.CreateTranslation(
                    (-1.375f * CubeC.Value) + (CubeC.Value / 2), 1.375f * CubeC.Value, 0);
        }
        else
        {
            modelMat = Matrix4x4.CreateTranslation(-CubeC.Value / 2, -(1.5f * CubeC.Value), 0) *
                Matrix4x4.CreateRotationZ((enable ? -_skina.Arm.X : -_model.ArmRotate.X) / 360) *
                Matrix4x4.CreateRotationX((enable ? -_skina.Arm.Y : -_model.ArmRotate.Y) / 360) *
                Matrix4x4.CreateTranslation(
                    (-1.5f * CubeC.Value) + (CubeC.Value / 2), 1.5f * CubeC.Value, 0);
        }
        GL.UniformMatrix4fv(modelLoc, 1, false, &modelMat);

        GL.BindVertexArray(_normalVAO.RightArm.VertexArrayObject);
        GL.DrawElements(GlConsts.GL_TRIANGLES, _steveModelDrawOrder,
                 GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);

        modelMat = Matrix4x4.CreateTranslation(0, -1.5f * CubeC.Value, 0) *
               Matrix4x4.CreateRotationZ((enable ? _skina.Leg.X : _model.LegRotate.X) / 360) *
               Matrix4x4.CreateRotationX((enable ? _skina.Leg.Y : _model.LegRotate.Y) / 360) *
               Matrix4x4.CreateTranslation(CubeC.Value * 0.5f, -CubeC.Value * 1.5f, 0);
        GL.UniformMatrix4fv(modelLoc, 1, false, &modelMat);

        GL.BindVertexArray(_normalVAO.LeftLeg.VertexArrayObject);
        GL.DrawElements(GlConsts.GL_TRIANGLES, _steveModelDrawOrder,
                GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);

        modelMat = Matrix4x4.CreateTranslation(0, -1.5f * CubeC.Value, 0) *
               Matrix4x4.CreateRotationZ((enable ? -_skina.Leg.X : -_model.LegRotate.X) / 360) *
               Matrix4x4.CreateRotationX((enable ? -_skina.Leg.Y : -_model.LegRotate.Y) / 360) *
               Matrix4x4.CreateTranslation(-CubeC.Value * 0.5f, -CubeC.Value * 1.5f, 0);
        GL.UniformMatrix4fv(modelLoc, 1, false, &modelMat);

        GL.BindVertexArray(_normalVAO.RightLeg.VertexArrayObject);
        GL.DrawElements(GlConsts.GL_TRIANGLES, _steveModelDrawOrder,
                GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);

        GL.BindTexture(GlConsts.GL_TEXTURE_2D, 0);
    }

    private unsafe void DrawTop(GlInterface gl)
    {
        gl.BindTexture(GlConsts.GL_TEXTURE_2D, _texture);

        var modelLoc = gl.GetUniformLocationString(_shaderProgram, "self");
        var modelMat = Matrix4x4.Identity;
        gl.UniformMatrix4fv(modelLoc, 1, false, &modelMat);

        gl.BindVertexArray(_topVAO.Body.VertexArrayObject);
        gl.DrawElements(GlConsts.GL_TRIANGLES, _steveModelDrawOrder,
                 GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);

        bool enable = _model.EnableAnimation;

        modelMat = Matrix4x4.CreateTranslation(0, CubeC.Value, 0) *
               Matrix4x4.CreateRotationZ((enable ? _skina.Head.X : _model.HeadRotate.X) / 360) *
               Matrix4x4.CreateRotationX((enable ? _skina.Head.Y : _model.HeadRotate.Y) / 360) *
               Matrix4x4.CreateRotationY((enable ? _skina.Head.Z : _model.HeadRotate.Z) / 360) *
               Matrix4x4.CreateTranslation(0, CubeC.Value * 1.5f, 0);
        gl.UniformMatrix4fv(modelLoc, 1, false, &modelMat);

        gl.BindVertexArray(_topVAO.Head.VertexArrayObject);
        gl.DrawElements(GlConsts.GL_TRIANGLES, _steveModelDrawOrder,
                 GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);

        if (_model.SteveModelType == SkinType.NewSlim)
        {
            modelMat = Matrix4x4.CreateTranslation(CubeC.Value / 2, -(1.375f * CubeC.Value), 0) *
                Matrix4x4.CreateRotationZ((enable ? _skina.Arm.X : _model.ArmRotate.X) / 360) *
                Matrix4x4.CreateRotationX((enable ? _skina.Arm.Y : _model.ArmRotate.Y) / 360) *
                Matrix4x4.CreateTranslation(
                    (1.375f * CubeC.Value) - (CubeC.Value / 2), 1.375f * CubeC.Value, 0);
        }
        else
        {
            modelMat = Matrix4x4.CreateTranslation(CubeC.Value / 2, -(1.5f * CubeC.Value), 0) *
                Matrix4x4.CreateRotationZ((enable ? _skina.Arm.X : _model.ArmRotate.X) / 360) *
                Matrix4x4.CreateRotationX((enable ? _skina.Arm.Y : _model.ArmRotate.Y) / 360) *
                Matrix4x4.CreateTranslation(
                    (1.5f * CubeC.Value) - (CubeC.Value / 2), 1.5f * CubeC.Value, 0);
        }
        gl.UniformMatrix4fv(modelLoc, 1, false, &modelMat);

        gl.BindVertexArray(_topVAO.LeftArm.VertexArrayObject);
        gl.DrawElements(GlConsts.GL_TRIANGLES, _steveModelDrawOrder,
                GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);

        if (_model.SteveModelType == SkinType.NewSlim)
        {
            modelMat = Matrix4x4.CreateTranslation(-CubeC.Value / 2, -(1.375f * CubeC.Value), 0) *
                Matrix4x4.CreateRotationZ((enable ? -_skina.Arm.X : -_model.ArmRotate.X) / 360) *
                Matrix4x4.CreateRotationX((enable ? -_skina.Arm.Y : -_model.ArmRotate.Y) / 360) *
                Matrix4x4.CreateTranslation(
                    (-1.375f * CubeC.Value) + (CubeC.Value / 2), 1.375f * CubeC.Value, 0);
        }
        else
        {
            modelMat = Matrix4x4.CreateTranslation(-CubeC.Value / 2, -(1.5f * CubeC.Value), 0) *
                Matrix4x4.CreateRotationZ((enable ? -_skina.Arm.X : -_model.ArmRotate.X) / 360) *
                Matrix4x4.CreateRotationX((enable ? -_skina.Arm.Y : -_model.ArmRotate.Y) / 360) *
                Matrix4x4.CreateTranslation(
                    (-1.5f * CubeC.Value) + (CubeC.Value / 2), 1.5f * CubeC.Value, 0);
        }
        gl.UniformMatrix4fv(modelLoc, 1, false, &modelMat);

        gl.BindVertexArray(_topVAO.RightArm.VertexArrayObject);
        gl.DrawElements(GlConsts.GL_TRIANGLES, _steveModelDrawOrder,
                GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);

        modelMat = Matrix4x4.CreateTranslation(0, -1.5f * CubeC.Value, 0) *
               Matrix4x4.CreateRotationZ((enable ? _skina.Leg.X : _model.LegRotate.X) / 360) *
               Matrix4x4.CreateRotationX((enable ? _skina.Leg.Y : _model.LegRotate.Y) / 360) *
               Matrix4x4.CreateTranslation(CubeC.Value * 0.5f, -CubeC.Value * 1.5f, 0);
        gl.UniformMatrix4fv(modelLoc, 1, false, &modelMat);

        gl.BindVertexArray(_topVAO.LeftLeg.VertexArrayObject);
        gl.DrawElements(GlConsts.GL_TRIANGLES, _steveModelDrawOrder,
                 GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);

        modelMat = Matrix4x4.CreateTranslation(0, -1.5f * CubeC.Value, 0) *
               Matrix4x4.CreateRotationZ((enable ? -_skina.Leg.X : -_model.LegRotate.X) / 360) *
               Matrix4x4.CreateRotationX((enable ? -_skina.Leg.Y : -_model.LegRotate.Y) / 360) *
               Matrix4x4.CreateTranslation(-CubeC.Value * 0.5f, -CubeC.Value * 1.5f, 0);
        gl.UniformMatrix4fv(modelLoc, 1, false, &modelMat);

        gl.BindVertexArray(_topVAO.RightLeg.VertexArrayObject);
        gl.DrawElements(GlConsts.GL_TRIANGLES, _steveModelDrawOrder,
                GlConsts.GL_UNSIGNED_SHORT, IntPtr.Zero);

        gl.BindTexture(GlConsts.GL_TEXTURE_2D, 0);
    }

    protected override unsafe void OnOpenGlRender(GlInterface gl, int fb)
    {
        _skina.Tick();

        if (_switchSkin)
        {
            LoadSkin(gl);
            _switchSkin = false;
        }
        if (_switchModel)
        {
            LoadModel(gl);
            _switchModel = false;
        }

        if (!_model.HaveSkin)
            return;

        int x = (int)Bounds.Width;
        int y = (int)Bounds.Height;

        if (App.FindRoot(VisualRoot) is Window window)
        {
            var screen = window.RenderScaling;
            x = (int)(Bounds.Width * screen);
            y = (int)(Bounds.Height * screen);
        }
        gl.Viewport(0, 0, x, y);

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

        if (_rotXY.X != 0 || _rotXY.Y != 0)
        {
            _last *= Matrix4x4.CreateRotationX(_rotXY.X / 360)
                    * Matrix4x4.CreateRotationY(_rotXY.Y / 360);
            _rotXY.X = 0;
            _rotXY.Y = 0;
        }


        var modelMat = _last
                    * Matrix4x4.CreateTranslation(new(_xy.X, _xy.Y, 0))
                    * Matrix4x4.CreateScale(_dis);

        gl.UniformMatrix4fv(viewLoc, 1, false, &view);
        gl.UniformMatrix4fv(modelLoc, 1, false, &modelMat);
        gl.UniformMatrix4fv(projectionLoc, 1, false, &projection);

        CheckError(gl);

        DrawNormal(gl);

        DrawCape(gl);

        if (_model.EnableTop)
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
        _skina.Run = false;

        // Unbind everything
        gl.BindBuffer(GlConsts.GL_ARRAY_BUFFER, 0);
        gl.BindBuffer(GlConsts.GL_ELEMENT_ARRAY_BUFFER, 0);
        gl.BindVertexArray(0);
        gl.UseProgram(0);

        // Delete all resources.
        DeleteVAO(gl, _normalVAO);
        DeleteVAO(gl, _topVAO);

        gl.DeleteProgram(_shaderProgram);
        gl.DeleteShader(_fragmentShader);
        gl.DeleteShader(_vertexShader);
    }

    public void SetAnimation()
    {
        if (_model.EnableAnimation)
        {
            _skina.Run = true;
        }
        else
        {
            _skina.Run = false;
        }

        RequestNextFrameRendering();
    }
}