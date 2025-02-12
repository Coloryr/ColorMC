using System;
using System.IO;
using System.Threading.Tasks;
using ColorMC.Core.Downloader;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingModel
{
    [ObservableProperty]
    private string? _local1;
    [ObservableProperty]
    private string? _local2;
    [ObservableProperty]
    private string? _local3;
    [ObservableProperty]
    private string? _local4;
    [ObservableProperty]
    private string _tempSize;

    public string RunDir => ColorMCGui.RunDir;

    [RelayCommand]
    public async Task ChangeBackRunDir()
    {
        var res = await Model.ShowAsync(App.Lang("SettingWindow.Tab1.Info16"));
        if (!res)
        {
            return;
        }

        var path1 = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/ColorMC/run";
        PathHelper.Delete(path1);

        ColorMCGui.Reboot();
    }

    [RelayCommand]
    public async Task ChangeRunDir()
    {
        var res = await Model.ShowAsync(App.Lang("SettingWindow.Tab1.Info13"));
        if (!res)
        {
            return;
        }

        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var path = await PathBinding.SelectPath(top, PathType.RunDir);
        if (path == null)
        {
            return;
        }

        var path1 = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/ColorMC/run";
        PathHelper.WriteText(path1, path);

        ColorMCGui.Reboot();
    }

    [RelayCommand]
    public async Task Open1()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.SelectFile(top, FileType.Config);
        if (file.Item1 != null)
        {
            Local1 = file.Item1;
        }
    }

    [RelayCommand]
    public async Task Open2()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.SelectFile(top, FileType.AuthConfig);
        if (file.Item1 != null)
        {
            Local2 = file.Item1;
        }
    }

    [RelayCommand]
    public async Task Open3()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }

        var file = await PathBinding.SelectFile(top, FileType.Config);
        if (file.Item1 != null)
        {
            Local3 = file.Item1;
        }
    }

    [RelayCommand]
    public async Task Open4()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.SelectFile(top, FileType.Config);
        if (file.Item1 != null)
        {
            Local4 = file.Item1;
        }
    }

    [RelayCommand]
    public void Import1()
    {
        var local = Local1;
        if (string.IsNullOrWhiteSpace(local))
        {
            Model.Show(App.Lang("SettingWindow.Tab1.Error1"));
            return;
        }
        Model.Progress(App.Lang("SettingWindow.Tab1.Info5"));

        try
        {
            var res = ConfigBinding.LoadConfig(local);
            if (!res)
            {
                Model.Show(App.Lang("SettingWindow.Tab1.Error2"));
                return;
            }
            Model.Notify(App.Lang("SettingWindow.Tab1.Info6"));
        }
        catch (Exception e1)
        {
            Model.Show(App.Lang("SettingWindow.Tab1.Error3"));
            WindowManager.ShowError(App.Lang("SettingWindow.Tab1.Error3"), e1);
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
            Model.Show(App.Lang("SettingWindow.Tab1.Error1"));
            return;
        }
        Model.Progress(App.Lang("SettingWindow.Tab1.Info8"));

        try
        {
            var res = ConfigBinding.LoadAuthDatabase(local);
            if (!res)
            {
                Model.Show(App.Lang("SettingWindow.Tab1.Error4"));
                return;
            }
            Model.Notify(App.Lang("SettingWindow.Tab1.Info9"));
        }
        catch (Exception)
        {
            Model.Show(App.Lang("SettingWindow.Tab1.Error5"));
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
            Model.Show(App.Lang("SettingWindow.Tab1.Error1"));
            return;
        }
        Model.Progress(App.Lang("SettingWindow.Tab1.Info5"));

        try
        {
            var res = ConfigBinding.LoadGuiConfig(local);
            if (!res)
            {
                Model.Show(App.Lang("SettingWindow.Tab1.Error2"));
                return;
            }
            Model.Notify(App.Lang("SettingWindow.Tab1.Info6"));
        }
        catch (Exception e1)
        {
            Model.Show(App.Lang("SettingWindow.Tab1.Error3"));
            WindowManager.ShowError(App.Lang("SettingWindow.Tab1.Error3"), e1);
        }
        finally
        {
            Model.ProgressClose();
        }
    }

    [RelayCommand]
    public void Import4()
    {
        var local = Local4;
        if (string.IsNullOrWhiteSpace(local))
        {
            Model.Show(App.Lang("SettingWindow.Tab1.Error1"));
            return;
        }
        Model.Progress(App.Lang("SettingWindow.Tab1.Info5"));

        try
        {
            var res = ConfigBinding.LoadFrpConfig(local);
            if (!res)
            {
                Model.Show(App.Lang("SettingWindow.Tab1.Error2"));
                return;
            }
            Model.Notify(App.Lang("SettingWindow.Tab1.Info6"));
        }
        catch (Exception e1)
        {
            Model.Show(App.Lang("SettingWindow.Tab1.Error3"));
            WindowManager.ShowError(App.Lang("SettingWindow.Tab1.Error3"), e1);
        }
        finally
        {
            Model.ProgressClose();
        }
    }

    [RelayCommand]
    public async Task ClearTemp()
    {
        var res = await Model.ShowAsync(App.Lang("SettingWindow.Tab1.Text21"));
        if (res)
        {
            BaseBinding.DeleteTemp();
            LoadTempSize();
        }
    }

    public void LoadTempSize()
    {
        var temp = DownloadManager.DownloadDir;

        TempSize = string.Format(App.Lang("SettingWindow.Tab1.Info20"), BaseBinding.GetFolderSize(temp));
    }

    private async void Reset()
    {
        var res = await Model.ShowAsync(App.Lang("SettingWindow.Tab1.Info1"));
        if (!res)
            return;

        ConfigBinding.ResetConfig();
        Model.Notify(App.Lang("SettingWindow.Tab1.Info2"));
    }

    private async void ClearUser()
    {
        var res = await Model.ShowAsync(App.Lang("SettingWindow.Tab1.Info3"));
        if (!res)
            return;

        UserBinding.ClearAllUser();
        Model.Notify(App.Lang("SettingWindow.Tab1.Info4"));
    }

    private async void ClearWindow()
    {
        var res = await Model.ShowAsync(App.Lang("SettingWindow.Tab1.Info18"));
        if (!res)
            return;

        BaseBinding.ClearWindowSetting();
        Model.Notify(App.Lang("SettingWindow.Tab1.Info19"));
    }

    private async void DumpUser()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var res = await PathBinding.SaveFile(top, FileType.User, null);
        if (res == true)
        {
            Model.Notify(App.Lang("SettingWindow.Tab1.Info17"));
        }
    }

    private void OpenDownloadPath()
    {
        PathBinding.OpenPath(PathType.DownloadPath);
    }

    private void Open()
    {
        PathBinding.OpenPath(PathType.BasePath);
    }

    private void OpenPicPath()
    {
        PathBinding.OpenPath(PathType.PicPath);
    }
}
