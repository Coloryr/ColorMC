using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace ColorMC.Gui.UI.Model;

/// <summary>
/// 其他项目
/// </summary>
public partial class ModExport1Model : ObservableObject
{
    [ObservableProperty]
    private string _sha1;
    [ObservableProperty]
    private string _sha512;
    [ObservableProperty]
    private string _url;

    public required string Path { get; set; }
    public required long FileSize { get; set; }

    public required PackType Type;
}