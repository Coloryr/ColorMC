using System.Collections.ObjectModel;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Model.Dialog;

public partial class CollectDownloadModel(string name) : BaseDialogModel(name)
{
    /// <summary>
    /// 显示的下载模组项目列表
    /// </summary>
    public ObservableCollection<FileModVersionModel> DownloadList { get; init; } = [];
}
