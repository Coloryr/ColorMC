using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls;
using System;

namespace ColorMC.Gui.UI.Windows;
public partial class SingleWindow : Window, IBaseWindow
{
    public AllControl window1;

    public Info3Control Info3 => window1.Info3;

    public Info1Control Info1 => window1.Info1;

    public Info4Control Info => window1.Info;

    public Info2Control Info2 => window1.Info2;

    public Info5Control Info5 => window1.Info5;

    public Info6Control Info6 => window1.Info6;

    HeadControl IBaseWindow.Head => window1.Head;

    public UserControl Con => window1.Con;

    public SingleWindow(AllControl window) : this()
    {
        window1 = window;

        Main.Children.Add(window);
    }
    public SingleWindow()
    {
        InitializeComponent();

        Icon = App.Icon;

        if (SystemInfo.Os == OsType.MacOS)
        {
            KeyDown += Window_KeyDown;
        }

        if (window1 != null && SystemInfo.Os == OsType.Linux)
        {
            SystemDecorations = SystemDecorations.BorderOnly;
        }

        Closed += UserWindow_Closed;
        Opened += UserWindow_Opened;
        Closing += SingleWindow_Closing;

        App.PicUpdate += Update;

        Update();
    }

    private async void SingleWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (window1 == null)
            return;

        await window1.Closing(e);
    }

    private void Window_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers == KeyModifiers.Control)
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

    public void SetTitle(string temp)
    {
        Title = temp;
        window1.Head.Title = temp;
    }

    private void UserWindow_Opened(object? sender, EventArgs e)
    {
        window1?.Opened();
    }

    private void UserWindow_Closed(object? sender, EventArgs e)
    {
        window1?.Closed();

        App.Close();
    }

    private void Update()
    {
        App.Update(this, null);
    }
}
