using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Gui.UI.Model.Skin;
using ColorMC.Gui.UI.Windows;
using System;
using System.Timers;

namespace ColorMC.Gui.UI.Controls.Skin;

public enum MoveType
{
    LeftUp, Up, RightUp,
    Left, Right,
    LeftDown, Down, RightDown
}

public partial class SkinControl : UserControl, IUserControl
{
    private readonly SkinModel _model;

    private readonly Timer _timer;
    private readonly Timer _timer1;
    private readonly Timer _timer2;
    private MoveType _type;

    private float _xdiff = 0;
    private float _ydiff = 0;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public UserControl Con => this;

    public string Title => App.GetLanguage("SkinWindow.Title");

    public SkinControl()
    {
        InitializeComponent();

        _model = new(this);
        DataContext = _model;

        Skin.SetModel(_model);

        Button2.Click += Button2_Click;

        Button_1_1.PropertyChanged += Button_1_1_PropertyChanged;
        Button_1_2.PropertyChanged += Button_1_1_PropertyChanged;
        Button_1_3.PropertyChanged += Button_1_1_PropertyChanged;
        Button_1_4.PropertyChanged += Button_1_1_PropertyChanged;
        Button_1_5.PropertyChanged += Button_1_1_PropertyChanged;
        Button_1_6.PropertyChanged += Button_1_1_PropertyChanged;
        Button_1_7.PropertyChanged += Button_1_1_PropertyChanged;
        Button_1_8.PropertyChanged += Button_1_1_PropertyChanged;

        Button_2_1.PropertyChanged += Button_2_1_PropertyChanged;
        Button_2_2.PropertyChanged += Button_2_1_PropertyChanged;
        Button_2_3.PropertyChanged += Button_2_1_PropertyChanged;
        Button_2_4.PropertyChanged += Button_2_1_PropertyChanged;
        Button_2_5.PropertyChanged += Button_2_1_PropertyChanged;
        Button_2_6.PropertyChanged += Button_2_1_PropertyChanged;
        Button_2_7.PropertyChanged += Button_2_1_PropertyChanged;
        Button_2_8.PropertyChanged += Button_2_1_PropertyChanged;

        Button_3_1.PropertyChanged += Button_3_1_PropertyChanged;
        Button_3_2.PropertyChanged += Button_3_1_PropertyChanged;

        _timer = new(TimeSpan.FromMilliseconds(20))
        {
            AutoReset = true
        };
        _timer.BeginInit();
        _timer.Elapsed += Timer_Elapsed;
        _timer.EndInit();

        _timer1 = new(TimeSpan.FromMilliseconds(20))
        {
            AutoReset = true
        };
        _timer1.BeginInit();
        _timer1.Elapsed += Timer1_Elapsed;
        _timer1.EndInit();

        _timer2 = new(TimeSpan.FromMilliseconds(20))
        {
            AutoReset = true
        };
        _timer2.BeginInit();
        _timer2.Elapsed += Timer2_Elapsed; ;
        _timer2.EndInit();

        App.SkinLoad += App_SkinLoad;

        SkinTop.PointerMoved += SkinTop_PointerMoved;
        SkinTop.PointerPressed += SkinTop_PointerPressed;
        SkinTop.PointerWheelChanged += SkinTop_PointerWheelChanged;
    }

    private void SkinTop_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (e.Delta.Y > 0)
        {
            Skin.AddDis(0.05f);
        }
        else
        {
            Skin.AddDis(-0.05f);
        }
    }

    private void SkinTop_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var pro = e.GetCurrentPoint(this);
        _xdiff = (float)pro.Position.X;
        _ydiff = -(float)pro.Position.Y;
    }

    private void SkinTop_PointerMoved(object? sender, PointerEventArgs e)
    {
        var pro = e.GetCurrentPoint(this);
        if (pro.Properties.IsLeftButtonPressed)
        {
            float y = (float)pro.Position.X - _xdiff;
            float x = (float)pro.Position.Y + _ydiff;

            _xdiff = (float)pro.Position.X;
            _ydiff = -(float)pro.Position.Y;

            Skin.Rot(x, y);
        }
        else if (pro.Properties.IsRightButtonPressed)
        {
            float x = (float)pro.Position.X - _xdiff;
            float y = (float)pro.Position.Y + _ydiff;

            _xdiff = (float)pro.Position.X;
            _ydiff = -(float)pro.Position.Y;

            Skin.Pos(x / ((float)Bounds.Width / 8), -y / ((float)Bounds.Height / 8));
        }
    }

    private void Timer2_Elapsed(object? sender, ElapsedEventArgs e)
    {
        switch (_type)
        {
            case MoveType.Up:
                Skin.AddDis(0.05f);
                break;
            case MoveType.Down:
                Skin.AddDis(-0.05f);
                break;
        }
    }

    private void Button_3_1_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Button.IsPressedProperty)
        {
            var button = sender as Button;

            if (button?.IsPressed == true)
            {
                _type = button.CommandParameter switch
                {
                    "Up" => MoveType.Up,
                    "Down" => MoveType.Down,
                    _ => throw new Exception("MoveType Error")
                };
                _timer2.Start();
            }
            else
            {
                _timer2.Stop();
            }
        }
    }

    private void Button_2_1_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Button.IsPressedProperty)
        {
            var button = sender as Button;

            if (button?.IsPressed == true)
            {
                _type = button.CommandParameter switch
                {
                    "LeftUp" => MoveType.LeftUp,
                    "Up" => MoveType.Up,
                    "RightUp" => MoveType.RightUp,
                    "Left" => MoveType.Left,
                    "Right" => MoveType.Right,
                    "LeftDown" => MoveType.LeftDown,
                    "Down" => MoveType.Down,
                    "RightDown" => MoveType.RightDown,
                    _ => throw new Exception("MoveType Error")
                };
                _timer1.Start();
            }
            else
            {
                _timer1.Stop();
            }
        }
    }

    private void Timer1_Elapsed(object? sender, ElapsedEventArgs e)
    {
        switch (_type)
        {
            case MoveType.LeftUp:
                Skin.Pos(-0.05f, 0.05f);
                break;
            case MoveType.Up:
                Skin.Pos(0, 0.05f);
                break;
            case MoveType.RightUp:
                Skin.Pos(0.05f, 0.05f);
                break;
            case MoveType.Left:
                Skin.Pos(-0.05f, 0);
                break;
            case MoveType.Right:
                Skin.Pos(0.05f, 0);
                break;
            case MoveType.LeftDown:
                Skin.Pos(-0.05f, -0.05f);
                break;
            case MoveType.Down:
                Skin.Pos(0, -0.05f);
                break;
            case MoveType.RightDown:
                Skin.Pos(0.05f, -0.05f);
                break;
        }
    }

    private void Button_1_1_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Button.IsPressedProperty)
        {
            var button = sender as Button;

            if (button?.IsPressed == true)
            {
                _type = button.CommandParameter switch
                {
                    "LeftUp" => MoveType.LeftUp,
                    "Up" => MoveType.Up,
                    "RightUp" => MoveType.RightUp,
                    "Left" => MoveType.Left,
                    "Right" => MoveType.Right,
                    "LeftDown" => MoveType.LeftDown,
                    "Down" => MoveType.Down,
                    "RightDown" => MoveType.RightDown,
                    _ => throw new Exception("MoveType Error")
                };
                _timer.Start();
            }
            else
            {
                _timer.Stop();
            }
        }
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        switch (_type)
        {
            case MoveType.LeftUp:
                Skin.Rot(-10, -10);
                break;
            case MoveType.Up:
                Skin.Rot(-10, 0);
                break;
            case MoveType.RightUp:
                Skin.Rot(-10, 10);
                break;
            case MoveType.Left:
                Skin.Rot(0, -10);
                break;
            case MoveType.Right:
                Skin.Rot(0, 10);
                break;
            case MoveType.LeftDown:
                Skin.Rot(10, -10);
                break;
            case MoveType.Down:
                Skin.Rot(10, 0);
                break;
            case MoveType.RightDown:
                Skin.Rot(10, 10);
                break;
        }
    }

    public void Opened()
    {
        Window.SetTitle(Title);
    }

    public void Update()
    {
        if (_model.IsLoad)
        {
            Skin.RequestNextFrameRendering();
        }
    }

    public void Closed()
    {
        App.SkinLoad -= App_SkinLoad;

        App.SkinWindow = null;
    }

    private void App_SkinLoad()
    {
        Skin.ChangeSkin();
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        Skin.Reset();
    }
}
