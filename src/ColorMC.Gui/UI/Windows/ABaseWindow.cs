using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls;

namespace ColorMC.Gui.UI.Windows;

public abstract class ABaseWindow : Window
{
    public abstract ITop ICon { get; }

    protected void Init()
    {
        Icon = ImageManager.Icon;

        AddHandler(KeyDownEvent, Window_KeyDown, RoutingStrategies.Tunnel);

        Opened += UserWindow_Opened;
        PropertyChanged += SelfBaseWindow_PropertyChanged;
    }

    private void UserWindow_Opened(object? sender, EventArgs e)
    {
        ICon.Opened();
    }

    private void SelfBaseWindow_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == WindowStateProperty)
        {
            ICon.WindowStateChange(WindowState);
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

    private async void Window_KeyDown(object? sender, KeyEventArgs e)
    {
        if (await ICon.OnKeyDown(sender, e))
        {
            e.Handled = true;
            return;
        }

        if (SystemInfo.Os == OsType.MacOS && e.KeyModifiers == KeyModifiers.Control)
        {
            switch (e.Key)
            {
                case Key.OemComma:
                    WindowManager.ShowSetting(SettingType.Normal);
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
}
