using System.Threading.Tasks;
using ColorMC.Gui.Net.Apis;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.Input;

#if !DEBUG
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
#endif

namespace ColorMC.Gui.UI.Model.Main;

/// <summary>
/// 主界面
/// </summary>
public partial class MainModel
{
#if !DEBUG
    private LaunchCheckRes _updateRes;
    private bool _isNewUpdate;
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

        var data = await ColorMCCloudAPI.GetNewLogAsync();
        if (data == null)
        {
            Window.Show(LangUtils.Get("App.Text113"));
        }
        else
        {
            var dialog = new LongTextModel(Window.WindowId)
            {
                Text1 = LangUtils.Get("App.Text24"),
                Text2 = data
            };
            Window.ShowDialog(dialog);
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
        var dialog = new LongTextModel(Window.WindowId)
        {
            Text1 = string.Format(LangUtils.Get("App.Text35"), _updateRes.Version),
            Text2 = _updateRes.Text ?? ""
        };
        var res = await Window.ShowDialogWait(dialog);
        if (res is true)
        {
            if (_isNewUpdate)
            {
                WebBinding.OpenWeb(WebType.ColorMCDownload);
            }
            else
            {
                UpdateUtils.StartUpdate(Window);
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
        CardUpdate = false;
#else
        _updateRes = await UpdateUtils.CheckMain();
        if (!_updateRes.IsOk)
        {
            Window.Notify(LangUtils.Get("SettingWindow.Tab3.Text38"));
            return;
        }
        CardUpdate = _updateRes.HaveUpdate;
        _isNewUpdate = _updateRes.NewVersion || ColorMCGui.IsAot || ColorMCGui.IsMin;
        LoadCard();
#endif
    }
}
