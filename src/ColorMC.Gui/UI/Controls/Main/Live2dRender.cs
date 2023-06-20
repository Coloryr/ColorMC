using Avalonia.Input;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.Utils;
using Live2DCSharpSDK.App;
using Live2DCSharpSDK.Framework.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.Main;

public class Live2dRender : OpenGlControlBase
{
    private LAppDelegate lapp;

    private DateTime time;
    private bool render;
    private bool change;
    private bool delete;
    private bool init = false;
    private MainModel Model;

    public bool HaveModel => lapp?.Live2dManager.GetModelNum() != 0;

    public Live2dRender()
    {
        new Thread(() =>
        {
            while (true)
            {
                if (render)
                {
                    Dispatcher.UIThread.Invoke(RequestNextFrameRendering);
                }
                Thread.Sleep(20);
            }
        }).Start();

        PointerPressed += Live2dRender_PointerPressed;

        DataContextChanged += Live2dRender_DataContextChanged;
    }

    private void Live2dRender_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MainModel model)
        {
            model.PropertyChanged += Model_PropertyChanged;
            Model = model;
        }
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "ModelChange")
        {
            change = true;
        }
        else if (e.PropertyName == "ModelDelete")
        {
            delete = true;
        }
    }

    private void ChangeModel()
    {
        lapp.Live2dManager.ReleaseAllModel();
        var model = GuiConfigUtils.Config.Live2D.Model;
        if (string.IsNullOrWhiteSpace(model) || !File.Exists(model))
        {
            return;
        }
        var info = new FileInfo(model);
        try
        {
            lapp.Live2dManager.LoadModel(info.DirectoryName! + "/", info.Name.Replace(".model3.json", ""));
        }
        catch (Exception e)
        {
            Logs.Error("model load error", e);
            var window = Model.Con.Window;
            window.OkInfo.Show("Live2D模型加载失败");
        }
    }

    private void Live2dRender_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var pro = e.GetCurrentPoint(this);
        if (pro.Properties.IsLeftButtonPressed)
        {
            Pressed();
            Moved((float)pro.Position.X, (float)pro.Position.Y);
        }
    }

    private static void CheckError(GlInterface gl)
    {
        int err;
        while ((err = gl.GetError()) != GlConsts.GL_NO_ERROR)
            Console.WriteLine(err);
    }

    protected override unsafe void OnOpenGlInit(GlInterface gl)
    {
        if (init)
            return;
        CheckError(gl);

        try
        {
            lapp = new(new AvaloniaApi(this, gl), Logs.Info);
            change = true;
            CheckError(gl);
            init = true;
        }
        catch (Exception e)
        {
            Logs.Error("live2d error", e);
        }
    }

    protected override void OnOpenGlDeinit(GlInterface GL)
    {

    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        if (!init)
            return;
        if (change)
        {
            change = false;
            ChangeModel();
            Model.ShowMessage(App.GetLanguage("Live2D.Text1"));
        }
        if (delete)
        {
            delete = false;
            lapp.Live2dManager.ReleaseAllModel();
        }
        gl.Viewport(0, 0, (int)Bounds.Width, (int)Bounds.Height);
        render = true;
        var now = DateTime.Now;
        float span = 0;
        if (time.Ticks == 0)
        {
            time = now;
        }
        else
        {
            span = (float)(now - time).TotalSeconds;
            time = now;
        }
        lapp.Run(span);
        CheckError(gl);
    }

    public void Pressed()
    {
        lapp.OnMouseCallBack(true);
    }

    public void Release()
    {
        lapp.OnMouseCallBack(false);
    }

    public void Moved(float x, float y)
    {
        lapp.OnMouseCallBack(x, y);
    }

    public List<string> GetMotions()
    {
        return lapp.Live2dManager.GetModel(0).Motions;
    }

    public List<string> GetExpressions()
    {
        return lapp.Live2dManager.GetModel(0).Expressions;
    }

    public void PlayMotion(string name)
    {
        lapp.Live2dManager.GetModel(0).StartMotion(name, MotionPriority.PriorityNormal);
    }

    public void PlayExpression(string name)
    {
        lapp.Live2dManager.GetModel(0).SetExpression(name);
    }
}
