using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;

namespace ColorMC.Gui.UI.Controls;

/// <summary>
/// 自定义标题栏
/// </summary>
public partial class HeadControl : UserControl
{
    /// <summary>
    /// 菜单按钮
    /// </summary>
    private class ButtonBack
    {
        private readonly IBrush _color;
        private readonly Button _button;

        public ButtonBack(Button button, IBrush basecolor)
        {
            _color = basecolor;
            _button = button;

            button.Background = _color;
        }

        public void SetColor(bool hide)
        {
            if (hide)
            {
                _button.Background = ThemeManager.GetColor(nameof(ThemeObj.ProgressBarBG));
            }
            else
            {
                _button.Background = _color;
            }
        }
    }

    private readonly Button _buttonClose;
    private readonly Button _buttonMax;
    private readonly Button _buttonMin;

    private readonly SelfPublisher<string> MaxObservale = new();

    private bool _isLost;

    public HeadControl()
    {
        InitializeComponent();

        Border1.PointerPressed += HeadControl_PointerPressed;
        TitleShow.PointerPressed += HeadControl_PointerPressed;
        TitleShow.PointerPressed += HeadControl_PointerPressed;

        Border1.DoubleTapped += Border1_DoubleTapped;
        TitleShow.DoubleTapped += Border1_DoubleTapped;
        TitleShow.DoubleTapped += Border1_DoubleTapped;

        DataContextChanged += HeadControl_DataContextChanged;

        var time = DateTime.Now;
        //macos风格
        if ((time.Day == 1 && time.Month == 4) ? SystemInfo.Os != OsType.MacOS : SystemInfo.Os == OsType.MacOS)
        {
            StackPanel2.HorizontalAlignment = HorizontalAlignment.Center;
            Icons.IsVisible = false;
            var img1 = new HeadImg() { IsVisible = false, Path = "/Resource/Icon/Head/min.svg" };
            _buttonMin = new Button()
            {
                Width = 13,
                Height = 13,
                Content = img1,
                Margin = new Thickness(20, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                BorderThickness = new Thickness(0),
                BorderBrush = Brushes.Transparent,
                CornerRadius = new CornerRadius(20)
            };
            _buttonMin.Classes.Add("none");
            var select1 = new ButtonBack(_buttonMin, Brush.Parse("#febb2c"));
            _buttonMin.PointerEntered += (a, b) => { img1.IsVisible = true; };
            _buttonMin.PointerExited += (a, b) => { img1.IsVisible = false; };

            var max = new HeadImg()
            {
                IsVisible = false
            };
            max.Bind(HeadImg.PathProperty, MaxObservale.ToBinding());
            MaxObservale.Notify(ImageManager.MaxIcon[0]);
            _buttonMax = new Button()
            {
                Width = 13,
                Height = 13,
                Content = max,
                Margin = new Thickness(40, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                BorderThickness = new Thickness(0),
                BorderBrush = Brushes.Transparent,
                CornerRadius = new CornerRadius(20)
            };
            _buttonMax.Classes.Add("none");
            var select2 = new ButtonBack(_buttonMax, Brush.Parse("#29c73f"));
            _buttonMax.PointerEntered += (a, b) => { max.IsVisible = true; };
            _buttonMax.PointerExited += (a, b) => { max.IsVisible = false; };

            var img2 = new HeadImg() { IsVisible = false, Path = "/Resource/Icon/Head/close.svg" };
            _buttonClose = new Button()
            {
                Width = 13,
                Height = 13,
                Content = img2,
                Margin = new Thickness(0, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                BorderThickness = new Thickness(0),
                BorderBrush = Brushes.Transparent,
                CornerRadius = new CornerRadius(20)
            };
            _buttonClose.Classes.Add("none");
            var select3 = new ButtonBack(_buttonClose, Brush.Parse("#fe5f59"));
            _buttonClose.PointerEntered += (a, b) => { img2.IsVisible = true; };
            _buttonClose.PointerExited += (a, b) => { img2.IsVisible = false; };

            var panel3 = new Panel()
            {
                Background = Brushes.Transparent,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(10, 0, 0, 0)
            };

            panel3.Children.Add(_buttonClose);
            panel3.Children.Add(_buttonMin);
            panel3.Children.Add(_buttonMax);
            Panel1.Children.Add(panel3);
            panel3.PointerEntered += (a, b) =>
            {
                if (_isLost)
                {
                    select1.SetColor(false);
                    select2.SetColor(false);
                    select3.SetColor(false);
                }
            };
            panel3.PointerExited += (a, b) =>
            {
                if (_isLost)
                {
                    select1.SetColor(true);
                    select2.SetColor(true);
                    select3.SetColor(true);
                }
            };

            Pandl2.Children.Remove(StackPanel2);
            Panel1.Children.Add(StackPanel2);

            StackPanel1.HorizontalAlignment = HorizontalAlignment.Right;
            StackPanel2.MaxWidth = 500;
            StackPanel2.HorizontalAlignment = HorizontalAlignment.Center;

            Dispatcher.UIThread.Post(() =>
            {
                if (VisualRoot is Window window)
                {
                    window.Activated += (a, b) =>
                    {
                        _isLost = false;

                        select1.SetColor(false);
                        select2.SetColor(false);
                        select3.SetColor(false);
                    };
                    window.Deactivated += (a, b) =>
                    {
                        _isLost = true;

                        select1.SetColor(true);
                        select2.SetColor(true);
                        select3.SetColor(true);
                    };
                }
            });
        }
        //Windows风格
        else
        {
            _buttonMin = new Button()
            {
                Width = 40,
                Height = 35,
                Content = new HeadImg() { Path = "/Resource/Icon/Head/min.svg" },
                BorderThickness = new Thickness(0),
                BorderBrush = Brushes.Transparent,
                CornerRadius = new CornerRadius(0),
                Background = Brushes.Transparent
            };
            _buttonMax = new Button()
            {
                Width = 40,
                Height = 35,
                BorderThickness = new Thickness(0),
                BorderBrush = Brushes.Transparent,
                CornerRadius = new CornerRadius(0),
                Background = Brushes.Transparent
            };
            var max = new HeadImg();
            max.Bind(HeadImg.PathProperty, MaxObservale.ToBinding());
            MaxObservale.Notify(ImageManager.MaxIcon[0]);
            _buttonMax.Content = max;
            
            _buttonClose = new Button()
            {
                Width = 40,
                Height = 35,
                Content = new HeadImg() { Path = "/Resource/Icon/Head/close.svg" },
                BorderThickness = new Thickness(0),
                BorderBrush = Brushes.Transparent,
                CornerRadius = new CornerRadius(0),
                Background = Brushes.Transparent
            };

            StackPanel1.Children.Add(_buttonMin);
            StackPanel1.Children.Add(_buttonMax);
            StackPanel1.Children.Add(_buttonClose);

            //Win32Properties.SetNonClientHitTestResult(_buttonMax, Win32Properties.Win32HitTestValue.MaxButton);
            //Win32Properties.SetNonClientHitTestResult(_buttonMin, Win32Properties.Win32HitTestValue.MinButton);
            //Win32Properties.SetNonClientHitTestResult(_buttonClose, Win32Properties.Win32HitTestValue.Close);
        }

        _buttonMin.Click += ButtonMin_Click;
        _buttonMax.Click += ButtonMax_Click;
        _buttonClose.Click += ButtonClose_Click;
    }

    private void Border1_DoubleTapped(object? sender, TappedEventArgs e)
    {
        WindowMax();
    }

    private void HeadControl_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is BaseModel model)
        {
            _buttonMin.Bind(IsVisibleProperty, model.HeadDisplayObservale.ToBinding());
            _buttonMax.Bind(IsVisibleProperty, model.HeadDisplayObservale.ToBinding());
            _buttonClose.Bind(IsVisibleProperty, model.HeadDisplayObservale.ToBinding());
            _buttonClose.Bind(IsEnabledProperty, model.HeadCloseObservale.ToBinding());
        }
    }

    private void ButtonClose_Click(object? sender, RoutedEventArgs e)
    {
        (VisualRoot as Window)?.Close();
    }

    private void ButtonMax_Click(object? sender, RoutedEventArgs e)
    {
        WindowMax();
    }

    private void WindowMax()
    {
        if (VisualRoot is not Window window)
        {
            return;
        }

        if (window.WindowState == WindowState.Maximized)
        {
            window.WindowState = WindowState.Normal;
        }
        else
        {
            window.WindowState = WindowState.Maximized;
        }
    }

    private void ButtonMin_Click(object? sender, RoutedEventArgs e)
    {
        if (VisualRoot is not Window window)
        {
            return;
        }

        window.WindowState = WindowState.Minimized;
    }

    private void HeadControl_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (VisualRoot as Window)?.BeginMoveDrag(e);
    }

    public void WindowStateChange(WindowState state)
    {
        if (state == WindowState.Maximized)
        {
            MaxObservale.Notify(ImageManager.MaxIcon[1]);
        }
        else
        {
            MaxObservale.Notify(ImageManager.MaxIcon[0]);
        }
    }
}
