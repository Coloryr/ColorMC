using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Controls.HeadControl;
using ColorMC.Gui.UI.Model;

namespace ColorMC.Gui.UI.Controls;

/// <summary>
/// 自定义标题栏
/// </summary>
public partial class TitleControl : UserControl
{
    public TitleControl()
    {
        InitializeComponent();

        DataContextChanged += TitleControl_DataContextChanged;

        PointerPressed += HeadControl_PointerPressed;
        TitleShow.PointerPressed += HeadControl_PointerPressed;
        TitleShow.PointerPressed += HeadControl_PointerPressed;

        DoubleTapped += Border1_DoubleTapped;
        TitleShow.DoubleTapped += Border1_DoubleTapped;
        TitleShow.DoubleTapped += Border1_DoubleTapped;

        var time = DateTime.Now;
        //macos风格
        if ((time.Day == 1 && time.Month == 4) ? SystemInfo.Os != OsType.MacOs : SystemInfo.Os == OsType.MacOs)
        {
            Content = new MacosHeadControl();
        }
        //Windows风格
        else
        {
            Content = new WindowsHeadControl();
        }
    }

    private void TitleControl_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is WindowModel model)
        {
            model.PropertyChanged += Model_PropertyChanged;
        }
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (VisualRoot is not Window window)
        {
            return;
        }

        if (e.PropertyName == nameof(WindowModel.NextState))
        {
            if (DataContext is WindowModel model)
            {
                if (model.NextState is WindowState.Minimized)
                {
                    window.WindowState = WindowState.Minimized;
                }
                else if (model.NextState is WindowState.Maximized)
                {
                    WindowMax();
                }
            }
        }
        else if (e.PropertyName == nameof(WindowModel.CloseClick))
        {
            window.Close();
        }
    }

    private void Border1_DoubleTapped(object? sender, TappedEventArgs e)
    {
        WindowMax();
    }

    private void WindowMax()
    {
        if (VisualRoot is not Window window)
        {
            return;
        }

        if (window.WindowState == WindowState.Maximized)
        {
            window.WindowState = WindowState.Normal;
        }
        else
        {
            window.WindowState = WindowState.Maximized;
        }
    }

    private void HeadControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (VisualRoot as Window)?.BeginMoveDrag(e);
    }
}
