using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ColorMC.Gui.UI.Model.Items;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Dialog;

public partial class CollectDownloadModel(string window) : ObservableObject
{
    /// <summary>
    /// 显示的下载模组项目列表
    /// </summary>
    public ObservableCollection<FileModVersionModel> DownloadList { get; init; } = [];

    /// <summary>
    /// 下载所有选中的收藏
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task DownloadAll()
    {
        DialogHost.Close(window, true, this);
    }

    [RelayCommand]
    public void CloseView()
    {
        DialogHost.Close(window, false, this);
    }
}
