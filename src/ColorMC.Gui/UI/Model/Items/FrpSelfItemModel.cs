using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.NetFrp;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Items;

public partial class NetFrpSelfItemModel(NetFrpModel model, FrpSelfObj obj) : SelectItemModel
{
    public string Name => obj.Name;
    public string IP => obj.IP + ":" + obj.Port;
    public int NetPort => obj.NetPort;
    public string Key => string.IsNullOrWhiteSpace(obj.Key) ? "" : "********";

    public FrpSelfObj Obj => obj;

    public void Reload()
    {
        OnPropertyChanged(nameof(IP));
        OnPropertyChanged(nameof(NetPort));
        OnPropertyChanged(nameof(Key));
    }

    [RelayCommand]
    public void Edit()
    {
        model.Edit(this);
    }

    [RelayCommand]
    public void Delete()
    {
        model.Delete(this);
    }

    public void Select()
    {
        model.Select(this);
    }
}
