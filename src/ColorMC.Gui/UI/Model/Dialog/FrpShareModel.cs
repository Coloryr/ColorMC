using System.Collections.Generic;
using System.Threading.Tasks;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.NetFrp;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// 映射联机大厅选择
/// </summary>
public partial class FrpShareModel : ObservableObject
{
    /// <summary>
    /// 游戏版本
    /// </summary>
    [ObservableProperty]
    private string _version;
    /// <summary>
    /// 附加信息
    /// </summary>
    [ObservableProperty]
    private string _text;
    /// <summary>
    /// 是否又加载器
    /// </summary>
    [ObservableProperty]
    private bool _isLoader;
    /// <summary>
    /// 加载器类型
    /// </summary>
    [ObservableProperty]
    public int _loader = -1;

    /// <summary>
    /// 加载器列表
    /// </summary>
    public List<string> Loaders { get; init; } = [];
    /// <summary>
    /// 游戏版本列表
    /// </summary>
    public List<string> VersionList { get; init; } = [];

    /// <summary>
    /// 初始化版本列表
    /// </summary>
    /// <param name="version">游戏版本</param>
    /// <returns></returns>
    public async Task Init(string version)
    {
        var list = await GameHelper.GetGameVersionsAsync(GameType.All);
        VersionList.AddRange(list);
        if (VersionList.Contains(version))
        {
            Version = version;
        }

        Loaders.AddRange(LanguageBinding.GetLoader());
        Loader = 0;
    }

    /// <summary>
    /// 确认
    /// </summary>
    [RelayCommand]
    public void Confirm()
    {
        DialogHost.Close(NetFrpModel.NameCon, true);
    }

    /// <summary>
    /// 取消
    /// </summary>
    [RelayCommand]
    public void Cancel()
    {
        DialogHost.Close(NetFrpModel.NameCon, false);
    }
}
