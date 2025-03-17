using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Main;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Main;

/// <summary>
/// L2D显示
/// </summary>
public partial class Live2dControl : UserControl
{
    /// <summary>
    /// 渲染定时器
    /// </summary>
    private readonly FpsTimer _renderTimer;
    /// <summary>
    /// 渲染器
    /// </summary>
    private readonly Live2dRender _render;

    /// <summary>
    /// 提示文字动画取消
    /// </summary>
    private CancellationTokenSource _cancel = new();

    public Live2dControl()
    {
        InitializeComponent();
#if Phone
        IsVisible = false;
        return;
#endif

        _render = new();

        Live2D.Child = _render;

        _renderTimer = new(_render)
        {
            FpsTick = FpsTick
        };

        DataContextChanged += Live2dControl_DataContextChanged;
        App.OnClose += App_OnClose;
    }

    private void App_OnClose()
    {
        _renderTimer.Close();
    }

    private void Live2dControl_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is MainModel model)
        {
            model.PropertyChanged += Model_PropertyChanged;
        }
    }

    private void FpsTick(int fps)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (IsVisible)
            {
                Label1.Text = $"{fps}Fps";
            }
        });
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == MainModel.ModelTextName)
        {
            _cancel.Cancel();
            _cancel = new();

            ShowMessage();
        }
        else if (e.PropertyName == MainModel.ModelChangeDoneName)
        {
            if (_render.HaveModel)
            {
                IsVisible = true;
                _renderTimer.Pause = false;
            }
            else if (!_render.HaveModel)
            {
                IsVisible = false;
                _renderTimer.Pause = true;
            }
        }
        else if (e.PropertyName == nameof(MainModel.Render))
        {
            var model = (sender as MainModel)!;
            if (!model.Render)
            {
                _renderTimer.Pause = true;
            }
            else if (_render.HaveModel)
            {
                _renderTimer.Pause = false;
            }
        }
        else if (e.PropertyName == nameof(MainModel.LowFps))
        {
            var model = (sender as MainModel)!;
            _renderTimer.LowFps = model.LowFps;
        }
    }

    private async void ShowMessage()
    {
        if (!_render.HaveModel)
        {
            return;
        }
        await ThemeManager.CrossFade.Start(null, Border1, _cancel.Token);
        if (_cancel.Token.IsCancellationRequested)
        {
            return;
        }
        await Task.Delay(TimeSpan.FromSeconds(5));
        if (_cancel.Token.IsCancellationRequested)
        {
            return;
        }
        await ThemeManager.CrossFade.Start(Border1, null, _cancel.Token);
    }
}
