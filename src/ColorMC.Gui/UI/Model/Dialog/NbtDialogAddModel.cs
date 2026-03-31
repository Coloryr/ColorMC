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
    public partial string Title { get; set; }

    /// <summary>
    /// 标题2
    /// </summary>
    [ObservableProperty]
    public partial string Title1 { get; set; }

    /// <summary>
    /// 输入的键
    /// </summary>
    [ObservableProperty]
    public partial string? Key { get; set; }

    /// <summary>
    /// 是否显示类型
    /// </summary>
    [ObservableProperty]
    public partial bool DisplayType { get; set; }

    /// <summary>
    /// 当前Nbt类型
    /// </summary>
    [ObservableProperty]
    public partial NbtType Type { get; set; } = NbtType.NbtString;
}
