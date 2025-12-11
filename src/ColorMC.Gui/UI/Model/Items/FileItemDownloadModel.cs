using System.Threading;
using System.Threading.Tasks;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 下载标记
/// </summary>
public partial class FileItemDownloadModel : ObservableObject
{
    public WindowModel? Window;

    public string Name { get; init; }

    /// <summary>
    /// 当前信息
    /// </summary>
    [ObservableProperty]
    private string? _info;
    /// <summary>
    /// 当前信息
    /// </summary>
    [ObservableProperty]
    private string? _subInfo;
    /// <summary>
    /// 进度
    /// </summary>
    [ObservableProperty]
    private double _now;
    /// <summary>
    /// 子进度
    /// </summary>
    [ObservableProperty]
    private double _nowSub;
    /// <summary>
    /// 是否显示子进度条
    /// </summary>
    [ObservableProperty]
    private bool _showSub;

    public CancellationToken Token => _cancel.Token;

    private readonly CancellationTokenSource _cancel = new();

    [RelayCommand]
    public async Task Cancel()
    {
        if (Window == null)
        {
            return;
        }
        if (await Window.ShowChoice(LangUtils.Get("AddModPackWindow.Text43")))
        {
            _cancel.Cancel();
        }
    }
}