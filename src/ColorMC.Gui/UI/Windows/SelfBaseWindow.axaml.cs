using Avalonia;
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
    public TopLevel? TopLevel => this;
    public Info3Control InputInfo => Info3;
    public Info1Control ProgressInfo => Info1;
    public Info4Control OkInfo => Info;
    public Info2Control NotifyInfo => Info2;
    public Info6Control TextInfo => Info6;
    public HeadControl Head => WinHead;
    public Info5Control ComboInfo => Info5;
    public IUserControl ICon { get; set; }

    private bool _isClose;

    public SelfBaseWindow() : this(null!)
    {

    }

    public SelfBaseWindow(IUserControl con)
    {
        ICon = con;

        InitializeComponent();

        if (SystemInfo.Os == OsType.Linux)
        {
            SystemDecorations = SystemDecorations.Full;
            Head.IsVisible = false;
        }

        KeyDown += Window_KeyDown;

        Icon = App.Icon;

        if (App.BackBitmap != null)
        {
            Image_Back.Source = App.BackBitmap;
        }

        if (ICon is UserControl con1)
        {
            MainControl.Children.Add(con1);
        }

        Closed += UserWindow_Closed;
        Opened += UserWindow_Opened;
        Activated += Window_Activated;
        Closing += SelfBaseWindow_Closing;
        PropertyChanged += SelfBaseWindow_PropertyChanged;

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

    private void SelfBaseWindow_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == WindowStateProperty)
        {
            ICon.WindowStateChange(WindowState);
        }
    }

    private void SelfBaseWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (ICon == null || _isClose == true)
            return;

        e.Cancel = true;

        Dispatcher.UIThread.Post(async () =>
        {
            var res = await ICon.Closing();
            if (!res)
            {
                _isClose = true;
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

        ICon.OnKeyDown(sender, e);
    }

    private void UserWindow_Opened(object? sender, EventArgs e)
    {
        ICon?.Opened();
    }

    private void UserWindow_Closed(object? sender, EventArgs e)
    {
        App.PicUpdate -= Update;

        ICon?.Closed();

        MainControl.Children.Clear();

        if (App.LastWindow == this)
        {
            App.LastWindow = null;
        }

        FuntionUtils.RunGC();
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

        ICon?.Update();
    }
}
