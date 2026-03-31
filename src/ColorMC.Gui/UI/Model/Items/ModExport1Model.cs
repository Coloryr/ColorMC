using ColorMC.Core.Objs;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 打包其他项目
/// </summary>
public partial class ModExport1Model : ObservableObject
{
    /// <summary>
    /// 校验
    /// </summary>
    [ObservableProperty]
    public partial string Sha1 { get; set; }

    /// <summary>
    /// 校验
    /// </summary>
    [ObservableProperty]
    public partial string Sha512 { get; set; }

    /// <summary>
    /// 下载网址
    /// </summary>
    [ObservableProperty]
    public partial string Url { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    public required string Path { get; set; }
    /// <summary>
    /// 文件大小
    /// </summary>
    public required long FileSize { get; set; }
    /// <summary>
    /// 打包类型
    /// </summary>
    public required PackType Type;
}