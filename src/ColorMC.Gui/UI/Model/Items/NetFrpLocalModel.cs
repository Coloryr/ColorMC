using ColorMC.Gui.UI.Model.NetFrp;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Items;

public partial class NetFrpLocalModel(NetFrpModel top, string motd, string port) : SelectItemModel
{
    public string Motd => motd;
    public string Port => port;

    [ObservableProperty]
    private bool _isStart;

    [RelayCommand]
    public void Start()
    {
        top.StartThisFrp(this);
    }
}
