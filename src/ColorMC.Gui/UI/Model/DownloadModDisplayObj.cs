using ColorMC.Core.Objs;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace ColorMC.Gui.Objs;

public partial class DownloadModDisplayModel : ObservableObject
{
    /// <summary>
    /// 名字
    /// </summary>
    [ObservableProperty]
    private string name;
    [ObservableProperty]
    private bool download;
    [ObservableProperty]
    private int selectVersion;

    public List<string> Version => ModVersion;

    public bool Optional;
    public List<string> ModVersion;
    public List<(DownloadItemObj Item, ModInfoObj Info)> Items;
}
