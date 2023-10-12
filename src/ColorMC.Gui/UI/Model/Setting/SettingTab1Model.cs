using ColorMC.Core.Objs;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingModel : MenuModel
{
    [ObservableProperty]
    private string? _local1;
    [ObservableProperty]
    private string? _local2;
    [ObservableProperty]
    private string? _local3;

    [RelayCommand]
    public async Task Open1()
    {
        var file = await PathBinding.SelectFile(FileType.Config);

        if (file != null)
        {
            Local1 = file;
        }
    }

    [RelayCommand]
    public async Task Open2()
    {
        var file = await PathBinding.SelectFile(FileType.AuthConfig);

        if (file != null)
        {
            Local2 = file;
        }
    }

    [RelayCommand]
    public async Task Open3()
    {
        var file = await PathBinding.SelectFile(FileType.Config);

        if (file != null)
        {
            Local3 = file;
        }
    }

    [RelayCommand]
    public void Import1()
    {
        var local = Local1;
        if (string.IsNullOrWhiteSpace(local))
        {
            Model.Show(App.GetLanguage("SettingWindow.Tab1.Error1"));
            return;
        }
        Model.Progress(App.GetLanguage("SettingWindow.Tab1.Info5"));

        try
        {
            var res = ConfigBinding.LoadConfig(local);
            if (!res)
            {
                Model.Show(App.GetLanguage("SettingWindow.Tab1.Error2"));
                return;
            }
            Model.Notify(App.GetLanguage("SettingWindow.Tab1.Info6"));
        }
        catch (Exception e1)
        {
            Model.Show(App.GetLanguage("SettingWindow.Tab1.Error3"));
            App.ShowError(App.GetLanguage("SettingWindow.Tab1.Error3"), e1);
        }
        finally
        {
            Model.ProgressClose();
        }
    }

    [RelayCommand]
    public void Import2()
    {
        var local = Local2;
        if (string.IsNullOrWhiteSpace(local))
        {
            Model.Show(App.GetLanguage("SettingWindow.Tab1.Error1"));
            return;
        }
        Model.Progress(App.GetLanguage("SettingWindow.Tab1.Info8"));

        try
        {
            var res = ConfigBinding.LoadAuthDatabase(local);
            if (!res)
            {
                Model.Show(App.GetLanguage("SettingWindow.Tab1.Error4"));
                return;
            }
            Model.Notify(App.GetLanguage("SettingWindow.Tab1.Info9"));
        }
        catch (Exception)
        {
            Model.Show(App.GetLanguage("SettingWindow.Tab1.Error5"));
        }
        finally
        {
            Model.ProgressClose();
        }
    }

    [RelayCommand]
    public void Import3()
    {
        var local = Local3;
        if (string.IsNullOrWhiteSpace(local))
        {
            Model.Show(App.GetLanguage("SettingWindow.Tab1.Error1"));
            return;
        }
        Model.Progress(App.GetLanguage("SettingWindow.Tab1.Info5"));

        try
        {
            var res = ConfigBinding.LoadGuiConfig(local);
            if (!res)
            {
                Model.Show(App.GetLanguage("SettingWindow.Tab1.Error2"));
                return;
            }
            Model.Notify(App.GetLanguage("SettingWindow.Tab1.Info6"));
        }
        catch (Exception e1)
        {
            Model.Show(App.GetLanguage("SettingWindow.Tab1.Error3"));
            App.ShowError(App.GetLanguage("SettingWindow.Tab1.Error3"), e1);
        }
        finally
        {
            Model.ProgressClose();
        }
    }

    [RelayCommand]
    public async Task Reset()
    {
        var res = await Model.ShowWait(App.GetLanguage("SettingWindow.Tab1.Info1"));
        if (!res)
            return;

        ConfigBinding.ResetConfig();
        Model.Notify(App.GetLanguage("SettingWindow.Tab1.Info2"));
    }

    [RelayCommand]
    public async Task ClearUser()
    {
        var res = await Model.ShowWait(App.GetLanguage("SettingWindow.Tab1.Info3"));
        if (!res)
            return;

        UserBinding.ClearAllUser();
        Model.Notify(App.GetLanguage("SettingWindow.Tab1.Info4"));
    }

    [RelayCommand]
    public void Open()
    {
        PathBinding.OpPath(PathType.BasePath);
    }
}
