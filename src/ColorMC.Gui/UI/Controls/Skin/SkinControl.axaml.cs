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
using OpenTK.Mathematics;
using System;

namespace ColorMC.Gui.UI.Controls.Skin;

public partial class SkinControl : UserControl, IUserControl
{
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

        CheckBox1.Click += CheckBox1_Click;
        CheckBox2.Click += CheckBox2_Click;
        CheckBox3.Click += CheckBox3_Click;

        Slider1.PropertyChanged += Slider1_PropertyChanged;
        Slider2.PropertyChanged += Slider2_PropertyChanged;
        Slider3.PropertyChanged += Slider3_PropertyChanged;

        ComboBox2.SelectedIndex = 0;

        App.SkinLoad += App_SkinLoad;
    }

    public void Opened()
    {
        if (SystemInfo.Os == OsType.Android)
        {
            Window.Info.Show("");

            return;
        }

        var temp = Matrix.CreateRotation(Math.PI);
        Skin.RenderTransform = new ImmutableTransform(temp);

        Skin.Init();
        Text1.Text = Skin.Info;

        Window.SetTitle(App.GetLanguage("SkinWindow.Title"));

        Check();
    }

    public void Update()
    {
        Skin.InvalidateVisual();
    }

    public void Closed()
    {
        App.SkinLoad -= App_SkinLoad;
        if (SystemInfo.Os != OsType.Android)
        {
            Skin.Close();
        }

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
        Skin.InvalidateVisual();
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
            Skin.InvalidateVisual();
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
            Skin.InvalidateVisual();
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
            Skin.InvalidateVisual();
        }
    }

    private void ComboBox1_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        if (ComboBox1.SelectedIndex == (int)Skin.steveModelType)
            return;
        if (ComboBox1.SelectedIndex == (int)SkinType.Unkonw)
        {
            window.Info.Show(App.GetLanguage("SkinWindow.Info1"));
            ComboBox1.SelectedIndex = (int)Skin.steveModelType;
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
