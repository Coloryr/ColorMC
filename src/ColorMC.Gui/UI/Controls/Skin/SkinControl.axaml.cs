using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Skin;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.Skin;

/// <summary>
/// 皮肤显示窗口
/// </summary>
public partial class SkinControl : BaseUserControl
{
    /// <summary>
    /// 渲染定时器
    /// </summary>
    private FpsTimer _renderTimer;

    private readonly SkinSideControl _side = new();

    public SkinControl() : base(WindowManager.GetUseName<SkinControl>())
    {
        InitializeComponent();

        Title = LanguageUtils.Get("SkinWindow.Title");

        SidePanel3.PointerPressed += SidePanel3_PointerPressed;

        ImageManager.SkinChange += SkinChange;
    }

    private void SidePanel3_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        SidePanel3.IsVisible = false;
    }

    private void MinModelChange()
    {
        if (DataContext is SkinModel model)
        {
            if (model.MinMode)
            {
                Decorator1.IsVisible = false;
                Decorator1.Child = null;
                Decorator2.Child = _side;
                model.Window.SetChoiseCall(WindowId, DisplaySide);
                model.Window.SetChoiseContent(WindowId, LanguageUtils.Get("SkinWindow.Text10"));
            }
            else
            {
                SidePanel3.IsVisible = false;
                Decorator1.IsVisible = true;
                Decorator2.Child = null;
                Decorator1.Child = _side;
                model.Window.RemoveChoiseData(WindowId);
            }
        }
    }

    private void DisplaySide()
    {
        SidePanel3.IsVisible = !SidePanel3.IsVisible;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is SkinModel model)
        {
            model.PropertyChanged += Model_PropertyChanged1;
        }
    }

    private void Model_PropertyChanged1(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == ControlModel.MinModeName)
        {
            MinModelChange();
        }
        else if (e.PropertyName == SkinModel.ResetName)
        {
            Skin.Reset();
        }
    }

    public override async Task<bool> OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            await (DataContext as SkinModel)!.Load();

            return true;
        }

        return false;
    }

    public override void ControlStateChange(WindowState state)
    {
        _renderTimer.Pause = state != WindowState.Minimized;
    }

    public override void Opened()
    {
        _renderTimer = new(Skin)
        {
            FpsTick = (fps) =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (DataContext is not SkinModel model)
                    {
                        return;
                    }
                    model.Fps = fps;
                });
            }
        };
        MinModelChange();
    }

    public override void Closed()
    {
        ImageManager.SkinChange -= SkinChange;

        _renderTimer.Close();

        WindowManager.SkinWindow = null;
    }

    protected override ControlModel GenModel(WindowModel model)
    {
        var amodel = new SkinModel(model);
        amodel.PropertyChanged += Model_PropertyChanged;
        return amodel;
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SkinModel.HaveSkin))
        {
            if ((DataContext as SkinModel)!.HaveSkin)
            {
                _renderTimer.Pause = false;
            }
            else
            {
                _renderTimer.Pause = true;
            }
        }
    }

    private void SkinChange()
    {
        Skin.ChangeSkin();
    }
}
