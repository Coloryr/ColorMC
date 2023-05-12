using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Controls.Add;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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

    public bool Optional;

    public List<string> ModVersion;
    public List<(DownloadItemObj Item, ModInfoObj Info)> Items;

    public List<string> Version { get { return ModVersion; } }
}
