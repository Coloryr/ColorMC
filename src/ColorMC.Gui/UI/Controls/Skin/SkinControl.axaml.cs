using Avalonia;
using Avalonia.Controls;
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
    private readonly SkinModel model;

    private readonly Timer timer;
    private readonly Timer timer1;
    private readonly Timer timer2;
    private MoveType type;

    public IBaseWindow Window => App.FindRoot(VisualRoot);
    public SkinControl()
    {
        InitializeComponent();

        model = new(this);
        DataContext = model;

        Skin.SetModel(model);

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

        timer = new(TimeSpan.FromMilliseconds(20))
        {
            AutoReset = true
        };
        timer.BeginInit();
        timer.Elapsed += Timer_Elapsed;
        timer.EndInit();

        timer1 = new(TimeSpan.FromMilliseconds(20))
        {
            AutoReset = true
        };
        timer1.BeginInit();
        timer1.Elapsed += Timer1_Elapsed;
        timer1.EndInit();

        timer2 = new(TimeSpan.FromMilliseconds(20))
        {
            AutoReset = true
        };
        timer2.BeginInit();
        timer2.Elapsed += Timer2_Elapsed; ;
        timer2.EndInit();

        App.SkinLoad += App_SkinLoad;
    }

    private void Timer2_Elapsed(object? sender, ElapsedEventArgs e)
    {
        switch (type)
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
                type = button.CommandParameter switch
                {
                    "Up" => MoveType.Up,
                    "Down" => MoveType.Down
                };
                timer2.Start();
            }
            else
            {
                timer2.Stop();
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
                type = button.CommandParameter switch
                {
                    "LeftUp" => MoveType.LeftUp,
                    "Up" => MoveType.Up,
                    "RightUp" => MoveType.RightUp,
                    "Left" => MoveType.Left,
                    "Right" => MoveType.Right,
                    "LeftDown" => MoveType.LeftDown,
                    "Down" => MoveType.Down,
                    "RightDown" => MoveType.RightDown
                };
                timer1.Start();
            }
            else
            {
                timer1.Stop();
            }
        }
    }

    private void Timer1_Elapsed(object? sender, ElapsedEventArgs e)
    {
        switch (type)
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
                type = button.CommandParameter switch
                {
                    "LeftUp" => MoveType.LeftUp,
                    "Up" => MoveType.Up,
                    "RightUp" => MoveType.RightUp,
                    "Left" => MoveType.Left,
                    "Right" => MoveType.Right,
                    "LeftDown" => MoveType.LeftDown,
                    "Down" => MoveType.Down,
                    "RightDown" => MoveType.RightDown
                };
                timer.Start();
            }
            else
            {
                timer.Stop();
            }
        }
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        switch (type)
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
        Window.SetTitle(App.GetLanguage("SkinWindow.Title"));
    }

    public void Update()
    {
        if (model.IsLoad)
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
