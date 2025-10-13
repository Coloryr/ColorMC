using System;
using System.Threading.Tasks;
using ColorMC.Core.Downloader;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Setting;

/// <summary>
/// 设置窗口
/// </summary>
public partial class SettingModel
{
    /// <summary>
    /// 配置文件本地位置
    /// </summary>
    [ObservableProperty]
    private string? _local1;
    /// <summary>
    /// 账户文件本地位置
    /// </summary>
    [ObservableProperty]
    private string? _local2;
    /// <summary>
    /// 配置文件本地位置
    /// </summary>
    [ObservableProperty]
    private string? _local3;
    /// <summary>
    /// 配置文件本地位置
    /// </summary>
    [ObservableProperty]
    private string? _local4;
    /// <summary>
    /// 缓存大小
    /// </summary>
    [ObservableProperty]
    private string _tempSize;

    /// <summary>
    /// 运行路径
    /// </summary>
    public string RunDir => ColorMCGui.BaseDir;

    /// <summary>
    /// 清理运行路径
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 设置运行路径
    /// </summary>
    /// <returns></returns>
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
        var path = await PathBinding.SelectPathAsync(top, PathType.RunDir);
        if (path == null)
        {
            return;
        }

        var path1 = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/ColorMC/run";
        PathHelper.WriteText(path1, path);

        ColorMCGui.Reboot();
    }

    /// <summary>
    /// 选中配置文件
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task Open1()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.SelectFileAsync(top, FileType.Config);
        if (file.Path != null)
        {
            Local1 = file.Path;
        }
    }

    /// <summary>
    /// 选中配置文件
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task Open2()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.SelectFileAsync(top, FileType.AuthConfig);
        if (file.Path != null)
        {
            Local2 = file.Path;
        }
    }

    /// <summary>
    /// 选中配置文件
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task Open3()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }

        var file = await PathBinding.SelectFileAsync(top, FileType.Config);
        if (file.Path != null)
        {
            Local3 = file.Path;
        }
    }

    /// <summary>
    /// 选中配置文件
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task Open4()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.SelectFileAsync(top, FileType.Config);
        if (file.Path != null)
        {
            Local4 = file.Path;
        }
    }

    /// <summary>
    /// 导入设置
    /// </summary>
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

    /// <summary>
    /// 导入设置
    /// </summary>
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

    /// <summary>
    /// 导入设置
    /// </summary>
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

    /// <summary>
    /// 导入设置
    /// </summary>
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

    /// <summary>
    /// 清理缓存
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 获取缓存大小
    /// </summary>
    public void LoadTempSize()
    {
        var temp = DownloadManager.DownloadDir;

        TempSize = string.Format(App.Lang("SettingWindow.Tab1.Info20"), PathBinding.GetFolderSize(temp));
    }

    /// <summary>
    /// 重置
    /// </summary>
    private async void Reset()
    {
        var res = await Model.ShowAsync(App.Lang("SettingWindow.Tab1.Info1"));
        if (!res)
            return;

        ConfigBinding.ResetConfig();
        Model.Notify(App.Lang("SettingWindow.Tab1.Info2"));
    }

    /// <summary>
    /// 清理账户
    /// </summary>
    private async void ClearUser()
    {
        var res = await Model.ShowAsync(App.Lang("SettingWindow.Tab1.Info3"));
        if (!res)
            return;

        UserBinding.ClearAllUser();
        Model.Notify(App.Lang("SettingWindow.Tab1.Info4"));
    }

    /// <summary>
    /// 清理窗口设置
    /// </summary>
    private async void ClearWindow()
    {
        var res = await Model.ShowAsync(App.Lang("SettingWindow.Tab1.Info18"));
        if (!res)
            return;

        BaseBinding.ClearWindowSetting();
        Model.Notify(App.Lang("SettingWindow.Tab1.Info19"));
    }

    /// <summary>
    /// 导出账户
    /// </summary>
    private async void DumpUser()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var res = await PathBinding.SaveFileAsync(top, FileType.User, null);
        if (res == true)
        {
            Model.Notify(App.Lang("SettingWindow.Tab1.Info17"));
        }
    }

    /// <summary>
    /// 打开下载地址
    /// </summary>
    private void OpenDownloadPath()
    {
        PathBinding.OpenPath(PathType.DownloadPath);
    }

    /// <summary>
    /// 打开运行路径
    /// </summary>
    private void Open()
    {
        PathBinding.OpenPath(PathType.BasePath);
    }

    /// <summary>
    /// 打开图片缓存路径
    /// </summary>
    private void OpenPicPath()
    {
        PathBinding.OpenPath(PathType.PicPath);
    }
}
