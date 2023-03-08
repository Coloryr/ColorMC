using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Core;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Controls.Setting;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils.LaunchSetting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ColorMC.Gui.UI.Windows;

public partial class AllWindow : Window, IBaseWindow
{
    Info3Control IBaseWindow.Info3 => Info3;
    Info1Control IBaseWindow.Info1 => Info1;
    Info4Control IBaseWindow.Info => Info;
    Info2Control IBaseWindow.Info2 => Info2;
    Info6Control IBaseWindow.Info6 => Info6;
    HeadControl IBaseWindow.Head => Head;
    Info5Control IBaseWindow.Info5 => Info5;
    Window IBaseWindow.Window => this;
    UserControl IBaseWindow.Con => null;

    private UserControl BaseControl;
    private UserControl Now;
    private bool IsDialog;

    private Dictionary<UserControl, Grid> Cons = new();
    private Dictionary<Grid, Button> Switchs = new();

    public AllWindow()
    {
        InitializeComponent();

        if (SystemInfo.Os == OsType.Linux)
        {
            SystemDecorations = SystemDecorations.BorderOnly;
            var rectangle = Border1;
            var window = this;
            Border1.PointerPressed += (sender, e) =>
            {
                if (e.GetCurrentPoint(rectangle).Properties.IsLeftButtonPressed)
                {
                    var point = e.GetPosition(rectangle);
                    var arg1 = point.X / rectangle.Bounds.Width;
                    var arg2 = point.Y / rectangle.Bounds.Height;
                    if (arg1 > 0.95)
                    {
                        if (arg2 > 0.95)
                        {
                            window.BeginResizeDrag(WindowEdge.SouthEast, e);
                        }
                        else if (arg2 <= 0.95)
                        {
                            window.BeginResizeDrag(WindowEdge.East, e);
                        }
                    }
                    else if (arg1 < 0.05)
                    {
                        if (arg2 <= 0.95)
                        {
                            window.BeginResizeDrag(WindowEdge.West, e);
                        }
                        else if (arg2 > 0.95)
                        {
                            window.BeginResizeDrag(WindowEdge.SouthWest, e);
                        }
                    }
                    else if (arg2 > 0.95)
                    {
                        window.BeginResizeDrag(WindowEdge.South, e);
                    }
                }
            };
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

        Button1.PointerEntered += Button1_PointerEntered;
        Button2.Click += Button2_Click;
        Expander1.PointerExited += Expander1_PointerExited;
        Closed += UserWindow_Closed;
        Opened += UserWindow_Opened;

        App.PicUpdate += Update;

        Update();
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        if (Now == null)
            return;

        Expander1.IsExpanded = false;
        if (IsDialog)
        {
            MainDialog.Children.Clear();
            IsDialog = false;
        }
        else
        {
            Close(Now);
        }
    }

    private void Expander1_PointerExited(object? sender, PointerEventArgs e)
    {
        Expander1.IsExpanded = false;
    }

    private void Button1_PointerEntered(object? sender, PointerEventArgs e)
    {
        Expander1.IsExpanded = true;
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

    public void Add(UserControl con)
    {
        if (BaseControl == null)
        {
            BaseControl = con;
            MainControl.Children.Add(BaseControl);
            (BaseControl as IUserControl)?.Opened();
        }
        else
        {
            var button = new Button
            {
                Content = GetName(con)
            };
            button.Click += (a, e)=> 
            {
                Expander1.IsExpanded = false;
                Active(con);
            };
            WrapPanel1.Children.Add(button);
            var grid = new Grid
            {
                IsVisible = false,
                Background = ColorSel.AppBackColor2
            };
            grid.Children.Add(con);
            Switchs.Add(grid, button);
            Cons.Add(con, grid);
            MainControl.Children.Add(grid);
            App.CrossFade300.Start(null, grid, CancellationToken.None);
            (con as IUserControl)?.Opened();
        }

        Up();

        Now = con;
    }

    private string GetName(UserControl con)
    {
        if (con is SettingControl)
        {
            return "设置界面";
        }
        else if (con is UserControl)
        {
            return "用户界面";
        }

        return "";
    }

    private void Up()
    {
        if (Cons.Count > 0)
        {
            Button1.IsVisible = true;
            Button2.IsVisible = true;
        }
        else
        {
            Button1.IsVisible = false;
            Button2.IsVisible = false;
        }
    }

    public void Active(UserControl con)
    {
        Now = con;
    }

    public void Close(UserControl con)
    {
        if (Cons.Remove(con, out var item))
        {
            MainControl.Children.Remove(item);
            if (Switchs.Remove(item, out var item1))
            {
                WrapPanel1.Children.Remove(item1);
            }
        }

        Up();

        (con as IUserControl)?.Closed();
    }

    public void ShowDialog(UserControl con)
    {
        MainDialog.Children.Add(con);

        IsDialog = true;
    }

    public void SetTitle(string temp)
    {
        Head.Title = Title = temp;
    }

    private void UserWindow_Opened(object? sender, EventArgs e)
    {
        
    }

    private void UserWindow_Closed(object? sender, EventArgs e)
    { 
        MainControl.Children.Clear();

        App.Close();
    }

    private void Update()
    {
        App.Update(this, Image_Back, Border1, Border2);
    }
}
