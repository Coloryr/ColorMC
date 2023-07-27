using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Controls.Custom;
using ColorMC.Gui.UI.Controls.Main;
using ColorMC.Gui.Utils;
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
    public IBaseWindow TopWindow => App.FindRoot(this);

    private IUserControl _baseControl;
    private IUserControl _nowControl;
    private readonly AllFlyout _allFlyout;
    private bool _isDialog;

    private readonly Dictionary<IUserControl, Grid> _cons = new();
    private readonly Dictionary<Grid, IUserControl> _cons1 = new();
    private readonly Dictionary<Grid, Button> _switchs = new();
    private readonly List<Button> _buttonList = new();

    public IBaseWindow Window => this;
    public IUserControl Con => _nowControl;
    UserControl IUserControl.Con => _nowControl.Con;

    Info3Control IBaseWindow.InputInfo => Info3;
    Info1Control IBaseWindow.ProgressInfo => Info1;
    Info4Control IBaseWindow.OkInfo => Info;
    Info2Control IBaseWindow.NotifyInfo => Info2;
    Info5Control IBaseWindow.ComboInfo => Info5;
    Info6Control IBaseWindow.TextInfo => Info6;
    HeadControl IBaseWindow.Head => Head;

    public string Title => "";

    public AllControl()
    {
        InitializeComponent();

        _allFlyout = new(_buttonList);

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
        private readonly StackPanel panel;
        private readonly Grid control;
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
        _allFlyout.ShowAt(this, true);
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        if (_nowControl == null)
            return;

        if (_isDialog)
        {
            Grid1.Children.Clear();
            _isDialog = false;
        }
        else
        {
            Close(_nowControl);
        }
    }

    public void Add(IUserControl con)
    {
        if (_baseControl == null)
        {
            _baseControl = con;
            MainControl.Children.Add(_baseControl.Con);
            Dispatcher.UIThread.Post(() =>
            {
                (_baseControl as IUserControl)?.Opened();
            });
        }
        else
        {
            var button = new Button
            {
                Content = GetName(con),
                Height = 25,
                Width = 150
            };
            button.Click += (a, e) =>
            {
                _allFlyout.Hide();
                Active(con);
            };
            var grid = new Grid
            {
                IsVisible = false,
                Background = ColorSel.TopBottomColor
            };
            _buttonList.Add(button);
            grid.Children.Add(con.Con);
            _switchs.Add(grid, button);
            _cons.Add(con, grid);
            _cons1.Add(grid, con);
            MainControl.Children.Add(grid);
            App.CrossFade300.Start(null, grid, CancellationToken.None);
            Dispatcher.UIThread.Post(() =>
            {
                (con as IUserControl)?.Opened();
            });
        }

        Up();

        _nowControl = con;
    }

    private static string GetName(IUserControl con)
    {
        return con.Title;
    }

    private void Up()
    {
        if (_cons.Count > 0)
        {
            Button2.IsVisible = true;
            if (_cons.Count > 1)
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

    public void Active(IUserControl con)
    {
        foreach (Control item1 in MainControl.Children)
        {
            if (item1 == _baseControl)
                continue;

            item1.ZIndex = 0;
        }

        if (_cons.TryGetValue(con, out var item))
        {
            item.ZIndex = 1;
        }

        _nowControl = con;
    }

    public async void Close(IUserControl con)
    {
        if (con is IUserControl con1)
        {
            var res = await con1.Closing();
            if (res)
            {
                return;
            }
        }

        if (_cons.Remove(con, out var item))
        {
            _cons1.Remove(item);
            MainControl.Children.Remove(item);
            if (_switchs.Remove(item, out var item1))
            {
                _buttonList.Remove(item1);
            }
        }

        var item2 = MainControl.Children.Last();
        if (item2 is Grid grid)
        {
            _nowControl = _cons1[grid];
        }
        else
        {
            _nowControl = _baseControl;
        }

        Up();

        con.Closed();
    }

    public void ShowDialog(UserControl con)
    {
        Grid1.Children.Add(con);

        _isDialog = true;
    }

    private void Update()
    {
        App.UpdateWindow(null, Image_Back);

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

    public async Task<bool> Closing(WindowClosingEventArgs _)
    {
        if (_nowControl is MainControl || _nowControl is CustomControl)
            return false;
        if (_nowControl is not IUserControl now)
            return false;

        return await now.Closing();
    }

    public void HideAll()
    {
        var list = new List<IUserControl>(_cons.Keys);
        list.ForEach(Close);
    }
}
