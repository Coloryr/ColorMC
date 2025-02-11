using ColorMC.Core.Nbt;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// Nbt添加
/// </summary>
/// <param name="usename">窗口Id</param>
public partial class NbtDialogAddModel(string usename) : ObservableObject
{
    /// <summary>
    /// Nbt类型
    /// </summary>
    public string[] TypeSource { get; init; } = LanguageBinding.GetNbtName();

    /// <summary>
    /// 是否取消
    /// </summary>
    public bool Cancel { get; set; }

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

    [RelayCommand]
    public void AddConfirm()
    {
        Cancel = false;
        DialogHost.Close(usename);
    }

    [RelayCommand]
    public void AddCancel()
    {
        Cancel = true;
        DialogHost.Close(usename);
    }
}
