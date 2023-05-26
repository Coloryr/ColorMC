using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingTab1Model : ObservableObject
{
    private readonly IUserControl Con;

    [ObservableProperty]
    private string? local1;
    [ObservableProperty]
    private string? local2;
    [ObservableProperty]
    private string? local3;

    public SettingTab1Model(IUserControl con)
    {
        Con = con;
    }

    [RelayCommand]
    public async void Open1()
    {
        var window = Con.Window;
        var file = await BaseBinding.OpFile(window, FileType.Config);

        if (file != null)
        {
            Local1 = file;
        }
    }

    [RelayCommand]
    public async void Open2()
    {
        var window = Con.Window;
        var file = await BaseBinding.OpFile(window, FileType.AuthConfig);

        if (file != null)
        {
            Local2 = file;
        }
    }

    [RelayCommand]
    public async void Open3()
    {
        var window = Con.Window;
        var file = await BaseBinding.OpFile(window, FileType.Config);

        if (file != null)
        {
            Local3 = file;
        }
    }

    [RelayCommand]
    public void Import1()
    {
        var window = Con.Window;
        var local = Local1;
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

    [RelayCommand]
    public void Import2()
    {
        var window = Con.Window;
        var local = Local2;
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

    [RelayCommand]
    public void Import3()
    {
        var window = Con.Window;
        var local = Local3;
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

    [RelayCommand]
    public async void Reset()
    {
        var window = Con.Window;
        var res = await window.OkInfo.ShowWait(App.GetLanguage("SettingWindow.Tab1.Info1"));
        if (!res)
            return;

        ConfigBinding.ResetConfig();
        window.NotifyInfo.Show(App.GetLanguage("SettingWindow.Tab1.Info2"));
    }

    [RelayCommand]
    public async void ClearUser()
    {
        var window = Con.Window;
        var res = await window.OkInfo.ShowWait(App.GetLanguage("SettingWindow.Tab1.Info3"));
        if (!res)
            return;

        UserBinding.ClearAllUser();
        window.NotifyInfo.Show(App.GetLanguage("SettingWindow.Tab1.Info4"));
    }

    [RelayCommand]
    public void Open()
    {
        BaseBinding.OpenBaseDir();
    }
}
