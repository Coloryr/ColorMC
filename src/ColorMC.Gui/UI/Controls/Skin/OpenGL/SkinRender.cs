using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Rendering;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Skin;
using MinecraftSkinRender;
using MinecraftSkinRender.OpenGL;

namespace ColorMC.Gui.UI.Controls.Skin.OpenGL;

public class SkinRender : OpenGlControlBase, ICustomHitTest
{
    private SkinRenderOpenGL skin;
    private DateTime time;

    public SkinRender()
    {
        DataContextChanged += SkinRender_DataContextChanged;

        PointerWheelChanged += OpenGlPageControl_PointerWheelChanged;
        PointerPressed += OpenGlPageControl_PointerPressed;
        PointerReleased += OpenGlPageControl_PointerReleased;
        PointerMoved += OpenGlPageControl_PointerMoved;
    }

    private void SkinRender_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is SkinModel model)
        {
            model.PropertyChanged += Model_PropertyChanged;
        }
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var model = (sender as SkinModel)!;
        if (e.PropertyName == nameof(SkinModel.SteveModelType))
        {
            skin.SetSkinType(model.SteveModelType);
        }
        else if (e.PropertyName == nameof(SkinModel.EnableAnimation))
        {
            skin.SetAnimation(model.EnableAnimation);
        }
        else if (e.PropertyName == nameof(SkinModel.EnableCape))
        {
            skin.SetCape(model.EnableCape);
        }
        else if (e.PropertyName == nameof(SkinModel.EnableTop))
        {
            skin.SetTopModel(model.EnableTop);
        }
        else if (e.PropertyName == nameof(SkinModel.EnableMSAA))
        {
            skin.SetMSAA(model.EnableMSAA);
        }
        else if (e.PropertyName == SkinModel.RotateName)
        {
            skin.ArmRotate = model.ArmRotate;
            skin.LegRotate = model.LegRotate;
            skin.HeadRotate = model.HeadRotate;
        }
        else if (e.PropertyName == SkinModel.PosName)
        {
            skin.Pos(model.X, model.Y);
        }
        else if (e.PropertyName == SkinModel.ScollName)
        {
            skin.AddDis(model.X);
        }
        else if (e.PropertyName == SkinModel.RotName)
        {
            skin.Rot(model.X, model.Y);
        }
        else if (e.PropertyName == SkinModel.LoadName)
        {
            model.SteveModelType = skin.SkinType;
        }
    }

    public void ChangeSkin()
    {
        skin.SetSkin(ImageManager.SkinBitmap);
        skin.SetCape(ImageManager.CapeBitmap);

        var model = (DataContext as SkinModel)!;
        model.Type = (int)skin.SkinType;

        RequestNextFrameRendering();
    }

    public void Reset()
    {
        skin.ResetPos();

        RequestNextFrameRendering();
    }

    private void OpenGlPageControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (skin == null || !skin.HaveSkin)
        {
            return;
        }

        var po = e.GetCurrentPoint(this);
        var pos = e.GetPosition(this);

        KeyType type = KeyType.None;
        if (po.Properties.IsLeftButtonPressed)
        {
            type = KeyType.Left;
        }
        else if (po.Properties.IsRightButtonPressed)
        {
            type = KeyType.Right;
        }

        skin.PointerPressed(type, new((float)pos.X, (float)pos.Y));

        RequestNextFrameRendering();
    }

    private void OpenGlPageControl_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (skin == null || !skin.HaveSkin)
        {
            return;
        }

        var po = e.GetCurrentPoint(this);
        var pos = e.GetPosition(this);

        KeyType type = KeyType.None;
        if (po.Properties.IsLeftButtonPressed)
        {
            type = KeyType.Left;
        }
        else if (po.Properties.IsRightButtonPressed)
        {
            type = KeyType.Right;
        }

        skin.PointerReleased(type, new((float)pos.X, (float)pos.Y));

        RequestNextFrameRendering();
    }

    private void OpenGlPageControl_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (skin == null || !skin.HaveSkin)
        {
            return;
        }

        var po = e.GetCurrentPoint(this);
        var pos = e.GetPosition(this);

        KeyType type = KeyType.None;
        if (po.Properties.IsLeftButtonPressed)
        {
            type = KeyType.Left;
        }
        else if (po.Properties.IsRightButtonPressed)
        {
            type = KeyType.Right;
        }

        skin.PointerMoved(type, new((float)pos.X, (float)pos.Y));

        RequestNextFrameRendering();
    }

    private void OpenGlPageControl_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (skin == null || !skin.HaveSkin)
        {
            return;
        }

        skin.PointerWheelChanged(e.Delta.Y > 0);

        RequestNextFrameRendering();
    }

    protected override unsafe void OnOpenGlInit(GlInterface gl)
    {
        var model = (DataContext as SkinModel)!;

        CheckError(gl);

        skin = new(new AvaloniaApi(gl));
        skin.SetBackColor(new(0, 0, 0, 0));
        skin.SetCape(model.EnableCape);
        skin.SetTopModel(model.EnableTop);
        skin.SetMSAA(model.EnableMSAA);
        skin.SetAnimation(model.EnableAnimation);
        skin.SetSkin(ImageManager.SkinBitmap);
        skin.SetCape(ImageManager.CapeBitmap);
        skin.IsGLES = GlVersion.Type == GlProfileType.OpenGLES;
        skin.OpenGlInit();

        model.SteveModelType = skin.SkinType;
        model.Info = skin.Info;
    }

    protected override unsafe void OnOpenGlRender(GlInterface gl, int fb)
    {
        int x = (int)Bounds.Width;
        int y = (int)Bounds.Height;

        if (VisualRoot is TopLevel window)
        {
            var screen = window.RenderScaling;
            x = (int)(Bounds.Width * screen);
            y = (int)(Bounds.Height * screen);
        }

        skin.Width = x;
        skin.Height = y;

        if (time.Year == 1)
        {
            time = DateTime.Now;
        }

        var time1 = DateTime.Now;
        var temp = time1 - time;
        time = time1;

        skin.Tick(temp.TotalSeconds);
        skin.OpenGlRender(fb);

        var model = (DataContext as SkinModel)!;
        model.HaveSkin = skin.HaveSkin;

        CheckError(gl);
    }

    private static void CheckError(GlInterface gl)
    {
        int err;
        while ((err = gl.GetError()) != GlConsts.GL_NO_ERROR)
            Console.WriteLine(err);
    }

    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        skin.OpenGlDeinit();
    }

    public bool HitTest(Point point)
    {
        return Bounds.Contains(point);
    }
}