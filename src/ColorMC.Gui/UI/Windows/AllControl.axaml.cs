using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Controls.Add;
using ColorMC.Gui.UI.Controls.Custom;
using ColorMC.Gui.UI.Controls.Download;
using ColorMC.Gui.UI.Controls.Main;
using ColorMC.Gui.UI.Controls.Setting;
using ColorMC.Gui.UI.Controls.Skin;
using ColorMC.Gui.UI.Controls.User;
using ColorMC.Gui.Utils.LaunchSetting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Windows;

public partial class AllControl : UserControl, IUserControl, IBaseWindow
{
    private UserControl BaseControl;
    private UserControl Now;
    private AllFlyout AllFlyout1;
    private bool IsDialog;

    private readonly Dictionary<UserControl, Grid> Cons = new();
    private readonly Dictionary<Grid, UserControl> Cons1 = new();
    private readonly Dictionary<Grid, Button> Switchs = new();
    private readonly List<Button> List = new();

    public IBaseWindow Window => this;

    Info3Control IBaseWindow.Info3 => Info3;

    Info1Control IBaseWindow.Info1 => Info1;

    Info4Control IBaseWindow.Info => Info;

    Info2Control IBaseWindow.Info2 => Info2;

    Info5Control IBaseWindow.Info5 => Info5;

    Info6Control IBaseWindow.Info6 => Info6;

    HeadControl IBaseWindow.Head => Head;

    public UserControl Con => Now;

    public AllControl()
    {
        InitializeComponent();

        AllFlyout1 = new(List);

        if (App.BackBitmap != null)
        {
            Image_Back.Source = App.BackBitmap;
        }

        if (SystemInfo.Os == OsType.Linux)
        {
            Head.IsVisible = false;
        }

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;

        App.PicUpdate += Update;
    }

    public void Closed()
    {
        MainControl.Children.Clear();
    }

    public void Opened()
    {
        Update();
    }

    public class AllFlyout : PopupFlyoutBase
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
            Grid1.Children.Clear();
            IsDialog = false;
        }
        else
        {
            Close(Now);
        }
    }

    public void Add(UserControl con)
    {
        if (BaseControl == null)
        {
            BaseControl = con;
            MainControl.Children.Add(BaseControl);
            Dispatcher.UIThread.Post(() =>
            {
                (BaseControl as IUserControl)?.Opened();
            });
        }
        else
        {
            var button = new Button
            {
                Content = GetName(con),
                Height = 25,
                Width = 100
            };
            button.Click += (a, e) =>
            {
                AllFlyout1.Hide();
                Active(con);
            };
            var grid = new Grid
            {
                IsVisible = false,
                Background = ColorSel.TopBottomColor
            };
            List.Add(button);
            grid.Children.Add(con);
            Switchs.Add(grid, button);
            Cons.Add(con, grid);
            Cons1.Add(grid, con);
            MainControl.Children.Add(grid);
            App.CrossFade300.Start(null, grid, CancellationToken.None);
            Dispatcher.UIThread.Post(() =>
            {
                (con as IUserControl)?.Opened();
            });
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

    public async void Close(UserControl con)
    {
        if (con is IUserControl con1)
        {
            var res = await con1.Closing();
            if (res)
            {
                return;
            }
        }

        if (Cons.Remove(con, out var item))
        {
            Cons1.Remove(item);
            MainControl.Children.Remove(item);
            if (Switchs.Remove(item, out var item1))
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
        Grid1.Children.Add(con);

        IsDialog = true;
    }

    private void Update()
    {
        App.Update(null, Image_Back);

        Grid3.Background = GuiConfigUtils.Config.WindowTran ?
                ColorSel.BottomTranColor : ColorSel.BottomColor;
    }

    public void SetTitle(string data)
    {
        if (VisualRoot is SingleWindow win)
        {
            win.Title = data;
        }

        Head.Title = data;
    }

    public async Task<bool> Closing(WindowClosingEventArgs e)
    {
        if (Now is MainControl || Now is CustomControl)
            return false;
        if (Now is not IUserControl now)
            return false;

        return await now.Closing();
    }
}
