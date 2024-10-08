﻿using ColorMC.Core.Objs;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

public partial class DnsItemModel(string url, DnsType type) : ObservableObject
{
    public string Url => url;

    public string Type => type.GetName();

    public DnsType Dns => type;
}
