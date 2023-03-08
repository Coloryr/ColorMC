using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System;

namespace ColorMC.Gui.UI.Controls.Hello;

public partial class Tab2Control : UserControl
{
    public Tab2Control()
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
        var window = App.FindRoot(this);
        var local = TextBox_Local2.Text;
        if (string.IsNullOrWhiteSpace(local))
        {
            window.Info.Show(App.GetLanguage("HelloWindow.Tab2.Error1"));
            return;
        }
        window.Info1.Show(App.GetLanguage("HelloWindow.Tab2.Info1"));

        try
        {
            var res = ConfigBinding.LoadGuiConfig(local);
            if (!res)
            {
                window.Info.Show(App.GetLanguage("HelloWindow.Tab2.Error2"));
                return;
            }
            window.Info2.Show(App.GetLanguage("HelloWindow.Tab2.Info2"));
        }
        catch (Exception e1)
        {
            window.Info.Show(App.GetLanguage("HelloWindow.Tab2.Error3"));
            App.ShowError(App.GetLanguage("HelloWindow.Tab2.Error3"), e1);
        }
        finally
        {
            window?.Info1.Close();
        }
    }

    private async void Button_SelectFile2_Click(object? sender, RoutedEventArgs e)
    {
        var window = (VisualRoot as Window)!;
        var file = await BaseBinding.OpFile(window, FileType.Config);

        if (file != null)
        {
            TextBox_Local2.Text = file;
            Button_Input2_Click(sender, e);
        }
    }

    private void Button_Next_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(this);
        (window.Con as HelloControl)?.Next();
    }

    private void Button_Input_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(this);
        var local = TextBox_Local.Text;
        if (string.IsNullOrWhiteSpace(local))
        {
            window.Info.Show(App.GetLanguage("HelloWindow.Tab2.Error1"));
            return;
        }
        window.Info1.Show(App.GetLanguage("HelloWindow.Tab2.Info1"));

        try
        {
            var res = ConfigBinding.LoadConfig(local);
            if (!res)
            {
                window.Info.Show(App.GetLanguage("HelloWindow.Tab2.Error2"));
                return;
            }
            window.Info2.Show(App.GetLanguage("HelloWindow.Tab2.Info2"));
        }
        catch (Exception e1)
        {
            window.Info.Show(App.GetLanguage("HelloWindow.Tab2.Error3"));
            App.ShowError(App.GetLanguage("HelloWindow.Tab2.Error3"), e1);
        }
        finally
        {
            window.Info1.Close();
        }
    }

    private async void Button_SelectFile_Click(object? sender, RoutedEventArgs e)
    {
        var window = (VisualRoot as Window)!;
        var file = await BaseBinding.OpFile(window, FileType.Config);

        if (file != null)
        {
            TextBox_Local.Text = file;
            Button_Input_Click(sender, e);
        }
    }

    private void Button_Input1_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(this);
        var local = TextBox_Local1.Text;
        if (string.IsNullOrWhiteSpace(local))
        {
            window.Info.Show(App.GetLanguage("HelloWindow.Tab2.Error1"));
            return;
        }
        window.Info1.Show(App.GetLanguage("HelloWindow.Tab2.Info4"));

        try
        {
            var res = ConfigBinding.LoadAuthDatabase(local);
            if (!res)
            {
                window.Info.Show(App.GetLanguage("HelloWindow.Tab2.Error4"));
                return;
            }
            window.Info2.Show(App.GetLanguage("HelloWindow.Tab2.Info5"));
        }
        catch (Exception)
        {
            window.Info.Show(App.GetLanguage("HelloWindow.Tab2.Error5"));
        }
        finally
        {
            window.Info1.Close();
        }
    }

    private async void Button_SelectFile1_Click(object? sender,
        RoutedEventArgs e)
    {
        var window = (VisualRoot as Window)!;
        var file = await BaseBinding.OpFile(window, FileType.AuthConfig);

        if (file != null)
        {
            TextBox_Local1.Text = file;
            Button_Input1_Click(sender, e);
        }
    }
}
