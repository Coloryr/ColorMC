using System.Collections.Generic;
using System.Threading.Tasks;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// 映射联机大厅选择
/// </summary>
public partial class FrpShareModel(string name) : BaseDialogModel(name)
{
    /// <summary>
    /// 游戏版本
    /// </summary>
    [ObservableProperty]
    public partial string Version { get; set; }

    /// <summary>
    /// 附加信息
    /// </summary>
    [ObservableProperty]
    public partial string Text { get; set; }

    /// <summary>
    /// 是否又加载器
    /// </summary>
    [ObservableProperty]
    public partial bool IsLoader { get; set; }

    /// <summary>
    /// 加载器类型
    /// </summary>
    [ObservableProperty]
    public partial int Loader { get; set; } = -1;

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

        Loaders.AddRange(LangUtils.GetLoader());
        Loader = 0;
    }
}
