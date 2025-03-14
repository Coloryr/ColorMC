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

public partial class BuildPackModel
{
    public ObservableCollection<PackFileItem> Files { get; init; } = [];

    [ObservableProperty]
    private PackFileItem? _fileItem;

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

    public void DeleteFile()
    {
        if (FileItem != null)
        {
            Files.Remove(FileItem);
        }
    }
}
