using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.Objs;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

public partial class DnsItemModel(string url, DnsType type) : ObservableObject
{
    public string Url => url;

    public string Type => type.GetName();

    public DnsType Dns => type;
}
