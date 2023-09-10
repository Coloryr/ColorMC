using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Controls.Custom;
using ColorMC.Gui.UI.Controls.Main;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.Utils;
using ColorMC.Gui.Utils.LaunchSetting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Windows;

public partial class AllControl : UserControl, IBaseWindow
{
    private IUserControl _baseControl;
    private IUserControl _nowControl;
    private readonly AllFlyout _allFlyout;
    private bool _isDialog;

    //mode true
    private readonly Dictionary<IUserControl, Control> _cons = new();
    private readonly Dictionary<Control, IUserControl> _cons1 = new();
    private readonly Dictionary<Control, Button> _switchs = new();
    private readonly List<Button> _buttonList = new();

    //mode false
    private readonly List<Control> controls = new();

    public IBaseWindow Window => this;

    public IUserControl ICon => _nowControl;

    public BaseModel Model => (DataContext as BaseModel)!;

    public AllControl()
    {
        InitializeComponent();

        DataContext = new BaseModel();

        _allFlyout = new(_buttonList);

        if (App.BackBitmap != null)
        {
            Image_Back.Source = App.BackBitmap;
        }

        if (SystemInfo.Os == OsType.Linux)
        {
            WinHead.IsVisible = false;
        }

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;

        App.PicUpdate += Update;
    }

    public void Closed()
    {
        Controls.Children.Clear();
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
            var con1 = (_baseControl as Control)!;
            Controls.Children.Add(con1);
            Dispatcher.UIThread.Post(() =>
            {
                _baseControl.Opened();
            });
        }
        else
        {
            if (GuiConfigUtils.Config.ControlMode)
            {
                var button = new Button
                {
                    Content = con.Title,
                    Height = 25,
                    Width = 150
                };
                button.Click += (a, e) =>
                {
                    _allFlyout.Hide();
                    Active(con);
                };
                var grid = new Border
                {
                    IsVisible = false,
                    Background = ColorSel.TopBottomColor
                };
                _buttonList.Add(button);
                grid.Child = (con as Control)!;
                _switchs.Add(grid, button);
                _cons.Add(con, grid);
                _cons1.Add(grid, con);

                Controls.Children.Add(grid);
                App.CrossFade300.Start(null, grid);
            }
            else
            {
                var con1 = Controls.Children[0];
                var con2 = (con as Control)!;
                Controls.Children.Remove(con1);
                if (con1 is not (MainControl or CustomControl))
                {
                    controls.Add(con1);
                }
                Controls.Children.Add(con2);
                App.CrossFade300.Start(null, con2);
            }
            
            con.Opened();
        }

        _nowControl = con;
        SetTitle(_nowControl.Title);

        ButtonUp();
    }

    private void ButtonUp()
    {
        if (GuiConfigUtils.Config.ControlMode)
        {
            if (_switchs.Count > 0)
            {
                Button2.IsVisible = true;
                if (_switchs.Count > 1)
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
        else
        {
            Button1.IsVisible = false;
            if (controls.Count > 0 || _nowControl is not (MainControl or CustomControl))
            {
                Button2.IsVisible = true;
            }
            else
            {
                Button2.IsVisible = false;
            }
        }
    }

    public void Active(IUserControl con)
    {
        if (GuiConfigUtils.Config.ControlMode)
        {
            foreach (Control item1 in Controls.Children)
            {
                if (item1 == _baseControl)
                    continue;

                item1.ZIndex = 0;
            }

            if (_cons.TryGetValue(con, out var item))
            {
                item.ZIndex = 1;
            }
        }
        else
        {
            var con1 = (con as Control)!;
            if (Controls.Children.Contains(con1))
                return;
            
            controls.Remove(con1);
            var con2 = Controls.Children[0];
            Controls.Children.Clear();
            controls.Add(con2);
            Controls.Children.Add(con1);
        }

        _nowControl = con;
        SetTitle(_nowControl.Title);

        ButtonUp();
    }

    public async void Close(IUserControl con)
    {
        var res = await con.Closing();
        if (res)
        {
            return;
        }

        if (GuiConfigUtils.Config.ControlMode)
        {
            if (_cons.Remove(con, out var item))
            {
                _cons1.Remove(item);
                Controls.Children.Remove(item);
                if (_switchs.Remove(item, out var item1))
                {
                    _buttonList.Remove(item1);
                }
            }

            var item2 = Controls.Children.Last();
            if (item2 is Border grid)
            {
                _nowControl = _cons1[grid];
            }
            else
            {
                _nowControl = _baseControl;
            }
        }
        else
        {
            var con1 = Controls.Children[0];
            var con2 = (con as Control)!;
            if (con1 == con2)
            {
                Controls.Children.Remove(con1);
            }
            controls.Remove(con2);
            if (Controls.Children.Count == 0)
            {
                if (controls.Count > 0)
                {
                    con1 = controls.Last();
                    controls.Remove(con1);
                    _nowControl = (con1 as IUserControl)!;
                }
                else
                {
                    con1 = (_baseControl as Control)!;
                    _nowControl = _baseControl;
                }
                Controls.Children.Add(con1);
            }
        }

        ((con as UserControl)?.DataContext as TopModel)?.TopClose();
        con.Closed();

        SetTitle(_nowControl.Title);

        ButtonUp();
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

        Model.Title = data;
    }

    public async Task<bool> Closing()
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

    public void SetIcon(Bitmap icon)
    {
        Model.SetIcon(icon);
    }

    public void Hide()
    {
        (VisualRoot as Window)?.Hide();
    }
}
