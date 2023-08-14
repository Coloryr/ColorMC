using ColorMC.Core.Objs;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Model.Items;

public partial class DownloadModDisplayModel : ObservableObject
{
    [ObservableProperty]
    private bool _download;

    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; set; }
    public int SelectVersion { get; set; }

    public List<string> Version => ModVersion;

    public bool Optional;
    public List<string> ModVersion;
    public List<(DownloadItemObj Item, ModInfoObj Info)> Items;
}
