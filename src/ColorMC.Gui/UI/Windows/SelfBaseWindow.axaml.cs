using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Controls.Error;
using ColorMC.Gui.Utils;
using ColorMC.Gui.Utils.LaunchSetting;
using System;

namespace ColorMC.Gui.UI.Windows;

public partial class SelfBaseWindow : Window, IBaseWindow
{
    Info3Control IBaseWindow.InputInfo => Info3;
    Info1Control IBaseWindow.ProgressInfo => Info1;
    Info4Control IBaseWindow.OkInfo => Info;
    Info2Control IBaseWindow.NotifyInfo => Info2;
    Info6Control IBaseWindow.TextInfo => Info6;
    HeadControl IBaseWindow.Head => Head;
    Info5Control IBaseWindow.ComboInfo => Info5;
    UserControl IBaseWindow.Con => (Main as UserControl)!;

    public IUserControl? Main;

    private bool SetClose;

    public SelfBaseWindow() : this(null)
    {

    }

    public SelfBaseWindow(IUserControl? con)
    {
        Main = con;

        InitializeComponent();

        if (SystemInfo.Os == OsType.Linux)
        {
            SystemDecorations = SystemDecorations.Full;
            Head.IsVisible = false;
        }

        if (SystemInfo.Os == OsType.MacOS)
        {
            KeyDown += Window_KeyDown;
        }

        Icon = App.Icon;

        if (App.BackBitmap != null)
        {
            Image_Back.Source = App.BackBitmap;
        }

        if (Main is UserControl con1)
        {
            MainControl.Children.Add(con1);
        }

        Closed += UserWindow_Closed;
        Opened += UserWindow_Opened;
        Activated += Window_Activated;
        Closing += SelfBaseWindow_Closing;

        App.PicUpdate += Update;

        if (con is ErrorControl)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
        else
        {
            FindGoodPos();
        }

        Update();
    }

    private void SelfBaseWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (Main == null || SetClose == true)
            return;

        e.Cancel = true;

        Dispatcher.UIThread.Post(async () =>
        {
            var res = await Main.Closing();
            if (!res)
            {
                SetClose = true;
                Close();
            }
        });
    }

    public void SetTitle(string temp)
    {
        Head.Title = Title = temp;
    }

    private void FindGoodPos()
    {
        var basewindow = App.LastWindow;

        if (basewindow == null || basewindow.WindowState == WindowState.Minimized)
            return;

        var pos = basewindow.Position;
        var sec = basewindow.Screens.ScreenFromWindow(basewindow);
        if (sec == null)
            return;
        var area = sec.WorkingArea;
        int x, y;
        if (pos.X > area.Width / 2)
        {
            x = pos.X - 100;
        }
        else
        {
            x = pos.X + 100;
        }

        if (pos.Y > area.Height / 2)
        {
            y = pos.Y - 40;
        }
        else
        {
            y = pos.Y + 40;
        }

        Position = new(x, y);
    }

    private void Window_KeyDown(object? sender, KeyEventArgs e)
    {
        var window = (sender as Window)!;
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
                    window.WindowState = WindowState.Minimized;
                    break;
                case Key.W:
                    window.Close();
                    break;
            }
        }
    }

    private void UserWindow_Opened(object? sender, EventArgs e)
    {
        Main?.Opened();
    }

    private void UserWindow_Closed(object? sender, EventArgs e)
    {
        App.PicUpdate -= Update;

        Main?.Closed();

        Main = null;
        MainControl.Children.Clear();

        if (App.LastWindow == this)
        {
            App.LastWindow = null;
        }

        Funtcions.RunGC();
    }
    private void Window_Activated(object? sender, EventArgs e)
    {
        App.LastWindow = this;
    }
    private void Update()
    {
        App.UpdateWindow(this, Image_Back);

        Grid1.Background = GuiConfigUtils.Config.WindowTran ?
                ColorSel.BottomTranColor : ColorSel.BottomColor;

        Main?.Update();
    }
}
