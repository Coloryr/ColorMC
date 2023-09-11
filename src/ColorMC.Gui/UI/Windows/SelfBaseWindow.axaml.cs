using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.Error;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using ColorMC.Gui.Utils.LaunchSetting;
using System;

namespace ColorMC.Gui.UI.Windows;

public partial class SelfBaseWindow : Window, IBaseWindow
{
    public IUserControl ICon { get; set; }

    public BaseModel Model => (DataContext as BaseModel)!;

    private bool _isClose;

    public SelfBaseWindow()
    {
        InitializeComponent();
    }

    public SelfBaseWindow(IUserControl con)
    {
        InitializeComponent();

        DataContext = new BaseModel();

        ICon = con;

        if (SystemInfo.Os == OsType.Linux)
        {
            SystemDecorations = SystemDecorations.Full;
            WinHead.IsVisible = false;
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
        {
            return;
        }

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
        Model.Title = temp;
    }

    private void FindGoodPos()
    {
        var basewindow = App.LastWindow;

        if (basewindow == null || basewindow.WindowState == WindowState.Minimized)
            return;

        var pos = basewindow.Position;
        var sec = basewindow.Screens.ScreenFromWindow(basewindow);
        if (sec == null)
        {
            return;
        }
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
        ICon.Opened();
    }

    private void UserWindow_Closed(object? sender, EventArgs e)
    {
        App.PicUpdate -= Update;

        ((ICon as UserControl)?.DataContext as TopModel)?.TopClose();
        DataContext = null;
        ICon.Closed();

        MainControl.Children.Clear();

        if (App.LastWindow == this)
        {
            App.LastWindow = null;
        }
        if (App.TopLevel == this)
        {
            if (ConfigBinding.WindowMode())
            {
                App.TopLevel = GetTopLevel(App.AllWindow);
            }
            else 
            {
                var win = App.GetMainWindow();
                App.TopLevel = win as Window;
            }
        }

        FuntionUtils.RunGC();
    }

    private void Window_Activated(object? sender, EventArgs e)
    {
        App.LastWindow = this;
        App.TopLevel = this;
    }

    private void Update()
    {
        App.UpdateWindow(this, Image_Back);

        Grid1.Background = GuiConfigUtils.Config.WindowTran ?
                ColorSel.BottomTranColor : ColorSel.BottomColor;

        ICon.Update();
    }

    public void SetIcon(Bitmap icon)
    {
        Model.SetIcon(icon);
    }
}
