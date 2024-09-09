using System.Collections.Generic;
using System.Threading.Tasks;
using ColorMC.Core.Objs;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.NetFrp;

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
        DialogHost.Close("ShareCon", true);
    }

    [RelayCommand]
    public void Cancel()
    {
        DialogHost.Close("ShareCon", false);
    }
}
