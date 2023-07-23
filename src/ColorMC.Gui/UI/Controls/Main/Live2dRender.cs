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
    private LAppDelegate _lapp;

    private DateTime _time;
    private bool _render;
    private bool _change;
    private bool _delete;
    private bool _init = false;
    private MainModel _model;
    private bool _first = false;

    public bool HaveModel 
    {
        get 
        {
            if (_lapp == null)
            {
                return false;
            }
            return _lapp.Live2dManager.GetModelNum() != 0;
        }
    }

    public Live2dRender()
    {
        PointerPressed += Live2dRender_PointerPressed;

        DataContextChanged += Live2dRender_DataContextChanged;
    }

    private void Live2dRender_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MainModel model)
        {
            model.PropertyChanged += Model_PropertyChanged;
            _model = model;
        }
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "ModelChange")
        {
            _change = true;
        }
        else if (e.PropertyName == "ModelDelete")
        {
            _delete = true;
        }
    }

    private void ChangeModel()
    {
        var window = _model.Con.Window;
        _lapp.Live2dManager.ReleaseAllModel();
        var model = GuiConfigUtils.Config.Live2D.Model;
        if (string.IsNullOrWhiteSpace(model))
        {
            return;
        }
        if (!File.Exists(model))
        {
            window.OkInfo.Show(App.GetLanguage("MainWindow.Live2d.Error1"));
            return;
        }
        var info = new FileInfo(model);
        try
        {
            _lapp.Live2dManager.LoadModel(info.DirectoryName! + "/", info.Name.Replace(".model3.json", ""));
        }
        catch (Exception e)
        {
            Logs.Error("model load error", e);
            window.OkInfo.Show(App.GetLanguage("MainWindow.Live2d.Error2"));
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
        if (_first)
            return;
        _first = true;
        if (_init)
            return;
        CheckError(gl);

        try
        {
            _lapp = new(new AvaloniaApi(this, gl), Logs.Info);
            _change = true;
            CheckError(gl);
            _init = true;
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
        if (!_init)
            return;
        if (_change)
        {
            _change = false;
            ChangeModel();
            _model.ShowMessage(App.GetLanguage("Live2D.Text1"));
        }
        if (_delete)
        {
            _delete = false;
            _lapp.Live2dManager.ReleaseAllModel();
        }
        gl.Viewport(0, 0, (int)Bounds.Width, (int)Bounds.Height);
        _render = true;
        var now = DateTime.Now;
        float span = 0;
        if (_time.Ticks == 0)
        {
            _time = now;
        }
        else
        {
            span = (float)(now - _time).TotalSeconds;
            _time = now;
        }
        _lapp.Run(span);
        CheckError(gl);
    }

    public void Pressed()
    {
        _lapp.OnMouseCallBack(true);
    }

    public void Release()
    {
        _lapp.OnMouseCallBack(false);
    }

    public void Moved(float x, float y)
    {
        _lapp.OnMouseCallBack(x, y);
    }

    public List<string> GetMotions()
    {
        return _lapp.Live2dManager.GetModel(0).Motions;
    }

    public List<string> GetExpressions()
    {
        return _lapp.Live2dManager.GetModel(0).Expressions;
    }

    public void PlayMotion(string name)
    {
        _lapp.Live2dManager.GetModel(0).StartMotion(name, MotionPriority.PriorityNormal);
    }

    public void PlayExpression(string name)
    {
        _lapp.Live2dManager.GetModel(0).SetExpression(name);
    }
}
