using Avalonia.Controls;
using ColorMC.Gui.UIBinding;
using System.Collections.Generic;
using Avalonia.Interactivity;

namespace ColorMC.Gui.UI.Views.Hello;

public partial class Tab5Control : UserControl
{
    private HelloWindow Window;
    public Tab5Control()
    {
        InitializeComponent();

        Button_AddGame.Click += AddGame_Click;
        Button_AddJava.Click += AddJava_Click;
        Button_AddUser.Click += AddUser_Click;
        Button_LoadConfig.Click += LoadConfig_Click;
        Button_Next.Click += Button_Next_Click;

        Load();
    }

    private void Button_Next_Click(object? sender, RoutedEventArgs e)
    {
        Window.Done();
    }

    private void LoadConfig_Click(object? sender, RoutedEventArgs e)
    {
        Window.SwitchTab(1);
    }

    private void AddUser_Click(object? sender, RoutedEventArgs e)
    {
        Window.SwitchTab(3);
    }

    private void AddJava_Click(object? sender, RoutedEventArgs e)
    {
        Window.SwitchTab(2);
    }

    private void AddGame_Click(object? sender, RoutedEventArgs e)
    {
        Window.SwitchTab(4);
    }

    public void SetWindow(HelloWindow window)
    {
        Window = window;
    }

    public void Load()
    {
        Label_Count1.Content = JavaBinding.GetJavaInfo().Count;
        Label_Count2.Content = UserBinding.GetAllUser().Count;
        Label_Count3.Content = OtherBinding.GetGames().Count;
    }
}
