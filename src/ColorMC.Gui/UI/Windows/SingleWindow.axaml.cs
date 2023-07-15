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

    Info3Control IBaseWindow.InputInfo => window1.Info3;

    Info1Control IBaseWindow.ProgressInfo => window1.Info1;

    Info4Control IBaseWindow.OkInfo => window1.Info;

    Info2Control IBaseWindow.NotifyInfo => window1.Info2;

    Info5Control IBaseWindow.ComboInfo => window1.Info5;

    Info6Control IBaseWindow.TextInfo => window1.Info6;

    HeadControl IBaseWindow.Head => window1.Head;

    IUserControl IBaseWindow.Con => window1.Con;

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

        var res = await window1.Closing(e);
        if (res)
        {
            e.Cancel = true;
        }
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
        App.UpdateWindow(this, null);
    }
}
