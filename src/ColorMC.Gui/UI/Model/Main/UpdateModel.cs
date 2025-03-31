using System.Threading.Tasks;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

#if !DEBUG
using ColorMC.Gui.Objs;
using ColorMC.Gui.Utils;
#endif

namespace ColorMC.Gui.UI.Model.Main;

/// <summary>
/// 主界面
/// </summary>
public partial class MainModel
{
    /// <summary>
    /// 是否有启动器更新
    /// </summary>
    [ObservableProperty]
    private bool _haveUpdate;
#if !DEBUG
    private bool _isNewUpdate;
    private string _updateStr;
#endif

    /// <summary>
    /// 获取更信息
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task NewInfo()
    {
        if (_isGetNewInfo)
        {
            return;
        }
        _isGetNewInfo = true;

        var data = await WebBinding.GetNewLog();
        if (data == null)
        {
            Model.Show(App.Lang("MainWindow.Error1"));
        }
        else
        {
            Model.Text(App.Lang("MainWindow.Info40"), data);
        }

        _isGetNewInfo = false;
    }

    /// <summary>
    /// 升级
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task Upgrade()
    {
#if !DEBUG
        var res = await Model.TextAsync(App.Lang("BaseBinding.Info2"), _updateStr);
        if (res)
        {
            if (_isNewUpdate)
            {
                WebBinding.OpenWeb(WebType.ColorMCDownload);
            }
            else
            {
                LauncherUpgrade.StartUpdate();
            }
        }
#endif
    }

    /// <summary>
    /// 检查更新
    /// </summary>
    private async void CheckUpdate()
    {
#if DEBUG
        HaveUpdate = false;
#else
        var data = await LauncherUpgrade.Check();
        if (!data.Item1)
        {
            return;
        }
        HaveUpdate = true;
        _isNewUpdate = data.Item2 || ColorMCGui.IsAot || ColorMCGui.IsMin;
        _updateStr = data.Item3!;
        LoadCard();
#endif
    }
}
