using System.Collections.Generic;
using ColorMC.Core.Objs;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// Mod下载项目显示
/// </summary>
public partial class DownloadModModel : ObservableObject
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
