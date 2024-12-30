using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;

namespace ColorMC.Gui.UI.Windows;

public abstract class ABaseWindow : Window
{
    public abstract ITopWindow ICon { get; }

    public abstract int DefaultWidth { get; }
    public abstract int DefaultHeight { get; }

    protected void InitBaseWindow()
    {
        Icon = ImageManager.WindowIcon;

        AddHandler(KeyDownEvent, Window_KeyDown, RoutingStrategies.Tunnel);

        Opened += UserWindow_Opened;
        Closing += ABaseWindow_Closing;
        PropertyChanged += OnPropertyChanged;
    }

    private void ABaseWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }

        WindowManager.SaveWindowState(ICon.UseName, new()
        {
            X = Position.X,
            Y = Position.Y,
            Width = (int)Width,
            Height = (int)Height,
            WindowState = WindowState
        });
    }

    private void UserWindow_Opened(object? sender, EventArgs e)
    {
        ICon.TopOpened();
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
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

    protected bool SetWindowState()
    {
        if (ColorMCGui.RunType != RunType.Program)
        {
            return true;
        }
        var state = WindowManager.GetWindowState(ICon.UseName);
        if (state != null)
        {
            WindowState = state.WindowState;

            if (state.WindowState != WindowState.Maximized)
            {
                int newX = state.X;
                int newY = state.Y;
                int newWidth = state.Width;
                int newHeight = state.Height;

                var sec1 = Screens.ScreenFromWindow(this);
                if (sec1 == null)
                {
                    return false;
                }

                var screenBounds = sec1.Bounds;

                if (newY < 0)
                {
                    newY = 0;
                }
                if (newY > screenBounds.Height - (newHeight / 2))
                {
                    newY = Math.Min(0, screenBounds.Height - newHeight);
                }

                if (newX < screenBounds.X)
                {
                    newX = screenBounds.X;
                }
                if (newX > screenBounds.Width - (newWidth / 2))
                {
                    newX = Math.Min(0, screenBounds.Right - newWidth);
                }

                //if (newY < 20)
                //{
                //    newY = 20;
                //}

                if (newWidth < MinWidth)
                {
                    newWidth = DefaultWidth;
                }
                if (newHeight < MinHeight)
                {
                    newHeight = DefaultHeight;
                }

                Position = new(newX, newY);
                Width = newWidth;
                Height = newHeight;
            }

            return true;
        }

        return false;
    }
}
