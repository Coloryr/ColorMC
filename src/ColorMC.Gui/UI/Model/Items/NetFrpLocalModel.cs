using ColorMC.Gui.UI.Model.NetFrp;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Items;

public partial class NetFrpLocalModel : ObservableObject
{
    public string Motd { get; init; }
    public string Port { get; init; }

    [ObservableProperty]
    private bool _isStart;

    private readonly NetFrpModel _top;
    public NetFrpLocalModel(NetFrpModel top, string motd, string port)
    {
        _top = top;
        Motd = motd;
        Port = port;
    }

    [RelayCommand]
    public void Start()
    {
        _top.StartThisLan(this);
    }
}
