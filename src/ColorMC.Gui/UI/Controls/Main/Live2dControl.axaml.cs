using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Gui.UI.Model.Main;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Main;

public partial class Live2dControl : UserControl
{
    private readonly FpsTimer _renderTimer;

    private CancellationTokenSource _cancel = new();

    public Live2dControl()
    {
        InitializeComponent();

        DataContextChanged += Live2dControl_DataContextChanged;

        _renderTimer = new(Live2d)
        {
            FpsTick = FpsTick
        };

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
                Label1.Content = $"{fps}Fps";
            }
        });
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "ModelText")
        {
            Dispatcher.UIThread.Post(() =>
            {
                _cancel.Cancel();
                _cancel = new();

                ShowMessage();
            });
        }
        else if (e.PropertyName == "ModelChangeDone")
        {
            if (Live2d.HaveModel)
            {
                IsVisible = true;
                _renderTimer.Pause = false;
            }
            else if (!Live2d.HaveModel)
            {
                IsVisible = false;
                _renderTimer.Pause = true;
            }
        }
        else if (e.PropertyName == "Render")
        {
            var model = (sender as MainModel)!;
            if (!model.Render)
            {
                _renderTimer.Pause = true;
            }
            else if (Live2d.HaveModel)
            {
                _renderTimer.Pause = false;
            }
        }
    }

    private async void ShowMessage()
    {
        if (!Live2d.HaveModel)
        {
            return;
        }
        await App.CrossFade300.Start(null, Border1, _cancel.Token);
        if (_cancel.Token.IsCancellationRequested)
        {
            return;
        }
        await Task.Delay(TimeSpan.FromSeconds(5));
        if (_cancel.Token.IsCancellationRequested)
        {
            return;
        }
        await App.CrossFade300.Start(Border1, null, _cancel.Token);
    }
}
