﻿using System.Threading.Tasks;
using ColorMC.Gui.UI.Model.NetFrp;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;

namespace ColorMC.Gui.UI.Model.Items;

public partial class NetFrpCloudServerModel : ObservableObject
{
    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 地址
    /// </summary>
    public string IP { get; set; }
    /// <summary>
    /// 当前在线人数
    /// </summary>
    public string Now { get; set; }
    /// <summary>
    /// 最大在线人数
    /// </summary>
    public string Max { get; set; }

    [JsonIgnore]
    public NetFrpModel Top;

    [RelayCommand]
    public void Join()
    {
        Top.Join(this);
    }

    [RelayCommand]
    public async Task Copy()
    {
        await BaseBinding.CopyTextClipboard(IP);
    }

    [RelayCommand]
    public void Test()
    {
        Top.Test(this);
    }
}
