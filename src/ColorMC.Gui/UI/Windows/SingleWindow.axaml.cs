using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;

namespace ColorMC.Gui.UI.Windows;

public partial class SingleWindow : Window
{
    public SingleWindow()
    {
        InitializeComponent();

        Icon = App.Icon;

        if (SystemInfo.Os == OsType.Linux)
        {
            SystemDecorations = SystemDecorations.BorderOnly;
        }

        Closed += UserWindow_Closed;
        Opened += UserWindow_Opened;
        Closing += SingleWindow_Closing;

        AddHandler(KeyDownEvent, Window_KeyDown, RoutingStrategies.Tunnel);

        PropertyChanged += SelfBaseWindow_PropertyChanged;

        DataContext = Win.DataContext;
    }

    private void SelfBaseWindow_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == WindowStateProperty)
        {
            Win.WindowStateChange(WindowState);
            if (SystemInfo.Os == OsType.Windows)
            {
                if (WindowState == WindowState.Maximized)
                {
                    Padding = new Thickness(8);
                }
                else
                {
                    Padding = new Thickness(0);
                }
            }
        }
    }

    private async void SingleWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        var res = await Win.Closing();
        if (res)
        {
            e.Cancel = true;
        }
    }

    private async void Window_KeyDown(object? sender, KeyEventArgs e)
    {
        if (await Win.IKeyDown(sender, e))
        {
            e.Handled = true;
            return;
        }

        if (SystemInfo.Os == OsType.MacOS && e.KeyModifiers == KeyModifiers.Control)
        {
            switch (e.Key)
            {
                case Key.OemComma:
                    App.ShowSetting(SettingType.Normal);
                    break;
                case Key.Q:
                    App.Close();
                    break;
                case Key.M:
                    WindowState = WindowState.Minimized;
                    break;
                case Key.W:
                    Close();
                    break;
            }
        }
    }

    private void UserWindow_Opened(object? sender, EventArgs e)
    {
        Win.Opened();
    }

    private void UserWindow_Closed(object? sender, EventArgs e)
    {
        Win.Closed();
        App.Close();
    }
}
