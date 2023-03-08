using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Core;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Controls.Add;
using ColorMC.Gui.UI.Controls.Download;
using ColorMC.Gui.UI.Controls.GameEdit;
using ColorMC.Gui.UI.Controls.Setting;
using ColorMC.Gui.UI.Controls.Skin;
using ColorMC.Gui.UI.Controls.User;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils.LaunchSetting;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
    UserControl IBaseWindow.Con => Now;

    private UserControl BaseControl;
    private UserControl Now;
    private AllFlyout AllFlyout1;
    private bool IsDialog;

    private readonly Dictionary<UserControl, Grid> Cons = new();
    private readonly Dictionary<Grid, UserControl> Cons1 = new();
    private readonly Dictionary<Grid, Button> Switchs = new();
    private readonly List<Button> List = new();

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

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        Closed += UserWindow_Closed;
        Opened += UserWindow_Opened;

        App.PicUpdate += Update;

        Update();
    }

    public class AllFlyout : FlyoutBase
    {
        private readonly List<Button> Obj;
        private StackPanel panel;
        private Grid control;
        public AllFlyout(List<Button> list)
        {
            Obj = list;

            Closing += AllFlyout_Closing;
            Opening += AllFlyout_Opening;

            control = new Grid();
            control.Children.Add(new Rectangle()
            {
                Fill = ColorSel.BackColor,
                Stroke = ColorSel.MainColor,
                StrokeThickness = 3
            });
            panel = new StackPanel()
            {
                Margin = new Thickness(5)
            };
            control.Children.Add(panel);
        }

        private void AllFlyout_Opening(object? sender, EventArgs e)
        {
            foreach (var item in Obj)
            {
                panel.Children.Add(item);
            }
        }

        private void AllFlyout_Closing(object? sender, CancelEventArgs e)
        {
            panel.Children.Clear();
        }

        protected override Control CreatePresenter()
        {
            return control;
        }
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        AllFlyout1.ShowAt(this, true);
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        if (Now == null)
            return;

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
                Content = GetName(con),
                Height = 25,
                Width = 100
            };
            button.Click += (a, e)=> 
            {
                AllFlyout1.Hide();
                Active(con);
            };
            var grid = new Grid
            {
                IsVisible = false,
                Background = ColorSel.AppBackColor2
            };
            List.Add(button);
            grid.Children.Add(con);
            Switchs.Add(grid, button);
            Cons.Add(con, grid);
            Cons1.Add(grid, con);
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
        else if (con is UsersControl)
        {
            return "用户列表";
        }
        else if (con is AddControl)
        {
            return "添加资源";
        }
        else if (con is AddGameControl)
        {
            return "添加实例";
        }
        else if (con is AddJavaControl)
        {
            return "添加Java";
        }
        else if (con is DownloadControl)
        {
            return "下载界面";
        }
        else if (con is SkinControl)
        {
            return "皮肤显示";
        }
        else if (con is AddModPackControl)
        {
            return "添加整合包";
        }

        return "";
    }

    private void Up()
    {
        if (Cons.Count > 0)
        {
            Button2.IsVisible = true;
            if (Cons.Count > 1)
            {
                Button1.IsVisible = true;
            }
            else
            {
                Button1.IsVisible = false;
            }
        }
        else
        {
            Button1.IsVisible = false;
            Button2.IsVisible = false;
        }
    }

    public void Active(UserControl con)
    {
        foreach (Control item1 in MainControl.Children)
        {
            if (item1 == BaseControl)
                continue;

            item1.ZIndex = 0;
        }

        if (Cons.TryGetValue(con, out var item))
        {
            item.ZIndex = 1;
        }

        Now = con;
    }

    public void Close(UserControl con)
    {
        if (Cons.Remove(con, out var item))
        {
            Cons1.Remove(item);
            MainControl.Children.Remove(item);
            if(Switchs.Remove(item, out var item1))
            {
                List.Remove(item1);    
            }
        }

        var item2 = MainControl.Children.Last();
        if (item2 is Grid grid)
        {
            Now = Cons1[grid];
        }
        else
        {
            Now = BaseControl;
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
        AllFlyout1 = new(List);
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
