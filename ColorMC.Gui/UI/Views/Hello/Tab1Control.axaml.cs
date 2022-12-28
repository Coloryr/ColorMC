using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.Utils;
using System;

namespace ColorMC.Gui.UI.Views.Hello;

public partial class Tab1Control : UserControl
{
    private HelloWindow Window;
    public Tab1Control()
    {
        InitializeComponent();

        Button_SelectFile.Click += Button_SelectFile_Click;
        Button_Input.Click += Button_Input_Click;
        Button_Next.Click += Button_Next_Click;
    }

    public void SetWindow(HelloWindow window)
    {
        Window = window;
    }

    private void Button_Next_Click(object? sender, RoutedEventArgs e)
    {
        Window.Next();
    }

    private void Button_Input_Click(object? sender, RoutedEventArgs e)
    {
        string local = TextBox_Local.Text;
        if (string.IsNullOrWhiteSpace(local))
        {
            Window.Info.Show("输入文件为空");
            return;
        }
        Window.Info1.Show("正在加载配置");

        try
        {
            ConfigUtils.Load(local);
            Window.Update();
            Window.Next();

        }
        catch (Exception)
        {
            Window.Info.Show("配置文件读取失败");
        }
        finally
        {
            Window.Info1.Close();
        }
    }

    private async void Button_SelectFile_Click(object? sender, RoutedEventArgs e)
    {
        OpenFileDialog openFile = new()
        {
            Title = "选择配置文件",
            AllowMultiple = false,
            Filters = new()
            {
                new FileDialogFilter()
                {
                    Extensions = new()
                    {
                        "json"
                    }
                }
            }
        };

        var file = await openFile.ShowAsync(Window);
        if (file?.Length > 0)
        {
            var item = file[0];
            TextBox_Local.Text = item;
        }

        
    }
}
