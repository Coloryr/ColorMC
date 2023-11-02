using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Dialog;

public partial class Info1Model : ObservableObject
{
    [ObservableProperty]
    private string _text;
    [ObservableProperty]
    private double _value;
    [ObservableProperty]
    private bool _indeterminate;


}
