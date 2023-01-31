using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using ColorMC.Gui.Skin.Render;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ColorMC.Gui;

internal class OpenglTest : GameWindow
{
    private SkinView3DRenderer render;
    public OpenglTest(int width, int height, string title) 
        : base(GameWindowSettings.Default, new NativeWindowSettings() 
        { Size = (width, height), Title = title }) 
    {
        
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        Image<Rgba32> image = Image.Load<Rgba32>("C:\\Users\\40206\\Desktop\\color_yr.png");
        render = new SkinView3DRenderer(image);
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, e.Width, e.Height);
        render.Change(e.Width, e.Height);
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        render.Draw();

        SwapBuffers();
    }
}
