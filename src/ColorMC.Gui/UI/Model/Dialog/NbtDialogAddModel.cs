using ColorMC.Core.Nbt;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// Nbt添加
/// </summary>
/// <param name="usename">窗口Id</param>
public partial class NbtDialogAddModel(string name) : BaseDialogModel(name)
{
    /// <summary>
    /// Nbt类型
    /// </summary>
    public string[] TypeSource { get; init; } = LangUtils.GetNbtName();

    /// <summary>
    /// 标题1
    /// </summary>
    [ObservableProperty]
    private string _title;
    /// <summary>
    /// 标题2
    /// </summary>
    [ObservableProperty]
    private string _title1;
    /// <summary>
    /// 输入的键
    /// </summary>
    [ObservableProperty]
    private string? _key;
    /// <summary>
    /// 是否显示类型
    /// </summary>
    [ObservableProperty]
    private bool _displayType;
    /// <summary>
    /// 当前Nbt类型
    /// </summary>
    [ObservableProperty]
    private NbtType _type = NbtType.NbtString;
}
