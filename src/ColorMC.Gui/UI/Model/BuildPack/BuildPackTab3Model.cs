using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.BuildPack;

/// <summary>
/// 导出客户端
/// 自定义文件
/// </summary>
public partial class BuildPackModel
{
    /// <summary>
    /// 自定义文件列表
    /// </summary>
    public ObservableCollection<PackFileItem> Files { get; init; } = [];

    /// <summary>
    /// 选中的自定义文件
    /// </summary>
    [ObservableProperty]
    private PackFileItem? _fileItem;

    /// <summary>
    /// 添加自定义文件
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task AddFile()
    {
        var top = Window.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.SelectFileAsync(top, FileType.File);
        if (file.Path == null)
        {
            return;
        }

        Files.Add(new PackFileItem()
        {
            Local = file.Path,
            File = file.FileName!
        });
    }

    /// <summary>
    /// 删除自定义文件
    /// </summary>
    public void DeleteFile()
    {
        if (FileItem != null)
        {
            Files.Remove(FileItem);
        }
    }
}
