using System.Threading.Tasks;
using ColorMC.Gui.Net.Apis;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
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

        var data = await ColorMCCloudAPI.GetNewLogAsync();
        if (data == null)
        {
            Window.Show(LanguageUtils.Get("App.Text113"));
        }
        else
        {
            var dialog = new LongTextModel(Window.WindowId)
            { 
                Text1 = LanguageUtils.Get("App.Text24"),
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
        var res = await Model.TextAsync(LanguageUtils.Get("App.Text35"), _updateStr);
        if (res)
        {
            if (_isNewUpdate)
            {
                WebBinding.OpenWeb(WebType.ColorMCDownload);
            }
            else
            {
                UpdateUtils.StartUpdate(Model);
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
        var data = await UpdateUtils.CheckMain();
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
