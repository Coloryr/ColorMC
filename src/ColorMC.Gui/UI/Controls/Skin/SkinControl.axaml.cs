using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System;
using System.Timers;
using System.Numerics;

namespace ColorMC.Gui.UI.Controls.Skin;

public enum MoveType
{
    LeftUp, Up, RightUp,
    Left, Right,
    LeftDown, Down, RightDown
}

public partial class SkinControl : UserControl, IUserControl
{
    private bool load = false;

    private Timer timer;
    private Timer timer1;
    private Timer timer2;
    private MoveType type;

    public IBaseWindow Window => App.FindRoot(VisualRoot);
    public SkinControl()
    {
        InitializeComponent();

        ComboBox1.ItemsSource = UserBinding.GetSkinType();
        ComboBox2.ItemsSource = BaseBinding.GetSkinRotateName();

        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;
        ComboBox2.SelectionChanged += ComboBox2_SelectionChanged;

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        Button3.Click += Button3_Click;
        Button4.Click += Button4_Click;
        Button5.Click += Button5_Click;

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


        CheckBox1.Click += CheckBox1_Click;
        CheckBox2.Click += CheckBox2_Click;
        CheckBox3.Click += CheckBox3_Click;

        Slider1.PropertyChanged += Slider1_PropertyChanged;
        Slider2.PropertyChanged += Slider2_PropertyChanged;
        Slider3.PropertyChanged += Slider3_PropertyChanged;

        ComboBox2.SelectedIndex = 0;

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

    public void Skin_Loaded()
    {
        Text1.Text = Skin.Info;

        Check();

        load = true;
    }

    public void Opened()
    {
        Window.SetTitle(App.GetLanguage("SkinWindow.Title"));
    }

    public void Update()
    {
        if (load)
        {
            Skin.RequestNextFrameRendering();
        }
    }

    public void Closed()
    {
        App.SkinLoad -= App_SkinLoad;

        App.SkinWindow = null;
    }

    private void CheckBox3_Click(object? sender, RoutedEventArgs e)
    {
        Skin.SetAnimation(CheckBox3.IsChecked == true);
    }

    private void CheckBox2_Click(object? sender, RoutedEventArgs e)
    {
        Skin.SetCapeDisplay(CheckBox2.IsChecked == true);
    }

    private void Button5_Click(object? sender, RoutedEventArgs e)
    {
        switch (ComboBox2.SelectedIndex)
        {
            case 0:
                Skin.ArmRotate.X = 0;
                Skin.ArmRotate.Y = 0;
                Skin.ArmRotate.Z = 0;
                break;
            case 1:
                Skin.LegRotate.X = 0;
                Skin.LegRotate.Y = 0;
                Skin.LegRotate.Z = 0;
                break;
            case 2:
                Skin.HeadRotate.X = 0;
                Skin.HeadRotate.Y = 0;
                Skin.HeadRotate.Z = 0;
                break;
            default:
                return;
        }
        Slider1.Value = 0;
        Slider2.Value = 0;
        Slider3.Value = 0;
        Skin.RequestNextFrameRendering();
    }

    private void Slider3_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == "Value")
        {
            switch (ComboBox2.SelectedIndex)
            {
                case 0:
                    Skin.ArmRotate.Z = (float)Slider2.Value;
                    break;
                case 1:
                    Skin.LegRotate.Z = (float)Slider3.Value;
                    break;
                case 2:
                    Skin.HeadRotate.Z = (float)Slider3.Value;
                    break;
                default:
                    return;
            }
            Skin.RequestNextFrameRendering();
        }
    }

    private void Slider2_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == "Value")
        {
            switch (ComboBox2.SelectedIndex)
            {
                case 0:
                    Skin.ArmRotate.Y = (float)Slider2.Value;
                    break;
                case 1:
                    Skin.LegRotate.Y = (float)Slider2.Value;
                    break;
                case 2:
                    Skin.HeadRotate.Y = (float)Slider2.Value;
                    break;
                default:
                    return;
            }
            Skin.RequestNextFrameRendering();
        }
    }

    private void Slider1_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name == "Value")
        {
            switch (ComboBox2.SelectedIndex)
            {
                case 0:
                    Skin.ArmRotate.X = (float)Slider1.Value;
                    break;
                case 1:
                    Skin.LegRotate.X = (float)Slider1.Value;
                    break;
                case 2:
                    Skin.HeadRotate.X = (float)Slider1.Value;
                    break;
                default:
                    return;
            }
            Skin.RequestNextFrameRendering();
        }
    }

    private void ComboBox1_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        if (ComboBox1.SelectedIndex == (int)Skin.SteveModelType)
            return;
        if (ComboBox1.SelectedIndex == (int)SkinType.Unkonw)
        {
            window.Info.Show(App.GetLanguage("SkinWindow.Info1"));
            ComboBox1.SelectedIndex = (int)Skin.SteveModelType;
            return;
        }
        Skin.ChangeType(ComboBox1.SelectedIndex);
    }

    private void ComboBox2_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Vector3 rotate;
        switch (ComboBox2.SelectedIndex)
        {
            case 0:
                rotate = Skin.ArmRotate;
                Slider1.Minimum = 0;
                Slider2.Minimum = -360;
                Slider3.Minimum = 0;
                Slider3.IsEnabled = false;
                break;
            case 1:
                rotate = Skin.LegRotate;
                Slider1.Minimum = 0;
                Slider2.Minimum = -360;
                Slider3.Minimum = 0;
                Slider3.IsEnabled = false;
                break;
            case 2:
                rotate = Skin.HeadRotate;
                Slider3.IsEnabled = true;
                Slider1.Minimum = -360;
                Slider2.Minimum = -360;
                Slider3.Minimum = -360;
                break;
            default:
                return;
        }

        Slider1.Value = rotate.X;
        Slider2.Value = rotate.Y;
        Slider3.Value = rotate.Z;
    }

    private void CheckBox1_Click(object? sender, RoutedEventArgs e)
    {
        Skin.SetTopDisplay(CheckBox1.IsChecked == true);
    }

    private async void Button4_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        var res = await BaseBinding.SaveFile(window, FileType.Skin, null);
        if (res == true)
        {
            window.Info2.Show(App.GetLanguage("Gui.Info10"));
        }
    }

    private void App_SkinLoad()
    {
        Skin.ChangeSkin();

        Dispatcher.UIThread.Post(Check);
    }

    private void Button3_Click(object? sender, RoutedEventArgs e)
    {
        UserBinding.EditSkin(App.FindRoot(this));
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        Skin.Reset();
    }

    private void Check()
    {
        if (Skin.HaveSkin)
        {
            Button4.IsEnabled = true;
        }
        else
        {
            Button4.IsEnabled = false;
        }
    }

    private async void Button1_Click(object? sender, RoutedEventArgs e)
    {
        await UserBinding.LoadSkin();
    }
}
