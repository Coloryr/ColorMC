using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System;

namespace ColorMC.Gui.UI.Controls.Hello;

public partial class Tab1Control : UserControl
{
    private IBaseWindow Window;
    public Tab1Control()
    {
        InitializeComponent();

        Button_SelectFile.Click += Button_SelectFile_Click;
        Button_Input.Click += Button_Input_Click;

        Button_SelectFile1.Click += Button_SelectFile1_Click;
        Button_Input1.Click += Button_Input1_Click;

        Button_SelectFile2.Click += Button_SelectFile2_Click;
        Button_Input2.Click += Button_Input2_Click;

        Button_Next.Click += Button_Next_Click;
    }

    private void Button_Input2_Click(object? sender, RoutedEventArgs e)
    {
        string local = TextBox_Local2.Text;
        if (string.IsNullOrWhiteSpace(local))
        {
            Window.Info.Show("�����ļ�Ϊ��");
            return;
        }
        Window.Info1.Show("���ڼ�������");

        try
        {
            var res = ConfigBinding.LoadGuiConfig(local);
            if (!res)
            {
                Window.Info.Show("�����ļ���ȡ����");
                return;
            }
            Window.Update();
            Window.Info2.Show("�����ļ��Ѽ���");
        }
        catch (Exception)
        {
            Window.Info.Show("�����ļ���ȡʧ��");
        }
        finally
        {
            Window?.Info1.Close();
        }
    }

    private async void Button_SelectFile2_Click(object? sender, RoutedEventArgs e)
    {
        OpenFileDialog openFile = new()
        {
            Title = "ѡ�������ļ�",
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

        var file = await openFile.ShowAsync(Window.Window);
        if (file?.Length > 0)
        {
            var item = file[0];
            TextBox_Local2.Text = item;
        }
    }

    public void SetWindow(HelloWindow window)
    {
        Window = window;
    }

    public void SetWindow(SettingWindow window)
    {
        Window = window;
        Button_Next.IsVisible = false;
        Button_Next.IsEnabled = false;
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
            Window.Info.Show("�����ļ�Ϊ��");
            return;
        }
        Window.Info1.Show("���ڼ�������");

        try
        {
            var res = ConfigBinding.LoadConfig(local);
            if (!res)
            {
                Window.Info.Show("�����ļ���ȡ����");
                return;
            }
            Window.Update();
            Window.Info2.Show("�����ļ��Ѽ���");
        }
        catch (Exception)
        {
            Window.Info.Show("�����ļ���ȡʧ��");
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
            Title = "ѡ�������ļ�",
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

        var file = await openFile.ShowAsync(Window.Window);
        if (file?.Length > 0)
        {
            var item = file[0];
            TextBox_Local.Text = item;
        }
    }

    private void Button_Input1_Click(object? sender, RoutedEventArgs e)
    {
        string local = TextBox_Local1.Text;
        if (string.IsNullOrWhiteSpace(local))
        {
            Window.Info.Show("�����ļ�Ϊ��");
            return;
        }
        Window.Info1.Show("���ڼ����˻����ݿ�");

        try
        {
            var res = ConfigBinding.LoadAuthDatabase(local);
            if (!res)
            {
                Window.Info.Show("�˻����ݿ��ȡ����");
                return;
            }
            Window.Update();
            Window.Info2.Show("�˻����ݿ��Ѽ���");
        }
        catch (Exception)
        {
            Window.Info.Show("�˻����ݿ��ȡʧ��");
        }
        finally
        {
            Window.Info1.Close();
        }
    }

    private async void Button_SelectFile1_Click(object? sender, RoutedEventArgs e)
    {
        OpenFileDialog openFile = new()
        {
            Title = "ѡ���˻����ݿ�",
            AllowMultiple = false,
            Filters = new()
            {
                new FileDialogFilter()
                {
                    Extensions = new()
                    {
                        "db"
                    }
                }
            }
        };

        var file = await openFile.ShowAsync(Window.Window);
        if (file?.Length > 0)
        {
            var item = file[0];
            TextBox_Local1.Text = item;
        }
    }
}
