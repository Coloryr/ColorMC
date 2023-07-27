using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Items;

public partial class NbtDataItem : ObservableObject
{
    [ObservableProperty]
    private int key;
    [ObservableProperty]
    private object value;
}
