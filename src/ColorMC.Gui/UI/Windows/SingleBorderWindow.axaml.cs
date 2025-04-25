using System;
using Avalonia;
using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Controls;

namespace ColorMC.Gui.UI.Windows;

/// <summary>
/// 带边框的单窗口
/// 某些系统需要用这个
/// </summary>
public partial class SingleBorderWindow : ABaseWindow
{
    public override IBaseControl ICon => Win;

    public override int DefaultWidth => 770;
    public override int DefaultHeight => 460;

    public SingleBorderWindow()
    {
        InitializeComponent();

        Closed += UserWindow_Closed;
        Closing += SingleWindow_Closing;
        PropertyChanged += OnPropertyChanged;

        DataContext = Win.DataContext;

        if (SystemInfo.Os == OsType.Linux)
        {
            SystemDecorations = SystemDecorations.BorderOnly;
        }

        InitBaseWindow();
        SetWindowState();
    }

    /// <summary>
    /// 窗口关闭
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void SingleWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        var res = await Win.Closing();
        if (res)
        {
            e.Cancel = true;
        }
    }

    private void UserWindow_Closed(object? sender, EventArgs e)
    {
        ColorMCGui.Exit();
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == WindowStateProperty)
        {
            if (WindowState == WindowState.Maximized)
            {
                Win.Margin = new Thickness(0);
            }
            else
            {
                Win.Margin = new Thickness(5);
            }
        }
    }
}
