using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Gui.UIBinding;
using System;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class Tab1Control : UserControl
{
    public Tab1Control()
    {
        InitializeComponent();

        Button_SelectFile.Click += Button_SelectFile_Click;
        Button_Input.Click += Button_Input_Click;

        Button_SelectFile1.Click += Button_SelectFile1_Click;
        Button_Input1.Click += Button_Input1_Click;

        Button_SelectFile2.Click += Button_SelectFile2_Click;
        Button_Input2.Click += Button_Input2_Click;

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        Button3.Click += Button3_Click;
    }

    private void Button3_Click(object? sender, RoutedEventArgs e)
    {
        BaseBinding.OpenBaseDir();
    }

    private async void Button2_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        var res = await window.OkInfo.ShowWait(App.GetLanguage("SettingWindow.Tab1.Info3"));
        if (!res)
            return;

        UserBinding.ClearAllUser();
        window.NotifyInfo.Show(App.GetLanguage("SettingWindow.Tab1.Info4"));
    }

    private async void Button1_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        var res = await window.OkInfo.ShowWait(App.GetLanguage("SettingWindow.Tab1.Info1"));
        if (!res)
            return;

        ConfigBinding.ResetConfig();
        window.NotifyInfo.Show(App.GetLanguage("SettingWindow.Tab1.Info2"));
    }

    private void Button_Input2_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        var local = TextBox_Local2.Text;
        if (string.IsNullOrWhiteSpace(local))
        {
            window.OkInfo.Show(App.GetLanguage("SettingWindow.Tab1.Error1"));
            return;
        }
        window.ProgressInfo.Show(App.GetLanguage("SettingWindow.Tab1.Info5"));

        try
        {
            var res = ConfigBinding.LoadGuiConfig(local);
            if (!res)
            {
                window.OkInfo.Show(App.GetLanguage("SettingWindow.Tab1.Error2"));
                return;
            }
            window.NotifyInfo.Show(App.GetLanguage("SettingWindow.Tab1.Info6"));
        }
        catch (Exception e1)
        {
            window.OkInfo.Show(App.GetLanguage("SettingWindow.Tab1.Error3"));
            App.ShowError(App.GetLanguage("SettingWindow.Tab1.Error3"), e1);
        }
        finally
        {
            window?.ProgressInfo.Close();
        }
    }

    private async void Button_SelectFile2_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        var file = await BaseBinding.OpFile(window, FileType.Config);

        if (file != null)
        {
            TextBox_Local2.Text = file;
        }
    }

    private void Button_Input_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        var local = TextBox_Local.Text;
        if (string.IsNullOrWhiteSpace(local))
        {
            window.OkInfo.Show(App.GetLanguage("SettingWindow.Tab1.Error1"));
            return;
        }
        window.ProgressInfo.Show(App.GetLanguage("SettingWindow.Tab1.Info5"));

        try
        {
            var res = ConfigBinding.LoadConfig(local);
            if (!res)
            {
                window.OkInfo.Show(App.GetLanguage("SettingWindow.Tab1.Error2"));
                return;
            }
            window.NotifyInfo.Show(App.GetLanguage("SettingWindow.Tab1.Info6"));
        }
        catch (Exception e1)
        {
            window.OkInfo.Show(App.GetLanguage("SettingWindow.Tab1.Error3"));
            App.ShowError(App.GetLanguage("SettingWindow.Tab1.Error3"), e1);
        }
        finally
        {
            window.ProgressInfo.Close();
        }
    }

    private async void Button_SelectFile_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        var file = await BaseBinding.OpFile(window, FileType.Config);

        if (file != null)
        {
            TextBox_Local.Text = file;
        }
    }

    private void Button_Input1_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        var local = TextBox_Local1.Text;
        if (string.IsNullOrWhiteSpace(local))
        {
            window.OkInfo.Show(App.GetLanguage("SettingWindow.Tab1.Error1"));
            return;
        }
        window.ProgressInfo.Show(App.GetLanguage("SettingWindow.Tab1.Info8"));

        try
        {
            var res = ConfigBinding.LoadAuthDatabase(local);
            if (!res)
            {
                window.OkInfo.Show(App.GetLanguage("SettingWindow.Tab1.Error4"));
                return;
            }
            window.NotifyInfo.Show(App.GetLanguage("SettingWindow.Tab1.Info9"));
        }
        catch (Exception)
        {
            window.OkInfo.Show(App.GetLanguage("SettingWindow.Tab1.Error5"));
        }
        finally
        {
            window.ProgressInfo.Close();
        }
    }

    private async void Button_SelectFile1_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        var file = await BaseBinding.OpFile(window, FileType.AuthConfig);

        if (file != null)
        {
            TextBox_Local1.Text = file;
        }
    }
}
