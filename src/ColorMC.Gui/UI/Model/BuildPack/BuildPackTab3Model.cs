using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.SelectFile(top, FileType.File);
        if (file.Item1 == null)
        {
            return;
        }

        Files.Add(new PackFileItem()
        {
            Local = file.Item1,
            File = file.Item2!
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
