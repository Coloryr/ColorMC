using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class AddDnsModel : ObservableObject
{
    [ObservableProperty]
    private bool _isDns = true;
    [ObservableProperty]
    private bool _isHttps;

    [ObservableProperty]
    private string _url;

    [RelayCommand]
    public void Confirm()
    {
        DialogHost.Close("NetworkSetting", true);
    }

    [RelayCommand]
    public void Cancel()
    {
        DialogHost.Close("NetworkSetting", false);
    }
}
