using System.Collections.Generic;
using System.Threading.Tasks;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.NetFrp;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// 映射联机大厅选择
/// </summary>
public partial class FrpShareModel : ObservableObject
{
    [ObservableProperty]
    private string _version;

    [ObservableProperty]
    private string _text;

    [ObservableProperty]
    private bool _isLoader;

    [ObservableProperty]
    public int _loader = -1;

    public List<string> Loaders { get; init; } = [];

    public List<string> VersionList { get; init; } = [];

    public async Task Init(string version)
    {
        var list = await GameBinding.GetGameVersions(GameType.All);
        VersionList.AddRange(list);
        if (VersionList.Contains(version))
        {
            Version = version;
        }

        Loaders.AddRange(LanguageBinding.GetLoader());
        Loader = 0;
    }

    [RelayCommand]
    public void Confirm()
    {
        DialogHost.Close(NetFrpModel.NameCon, true);
    }

    [RelayCommand]
    public void Cancel()
    {
        DialogHost.Close(NetFrpModel.NameCon, false);
    }
}
