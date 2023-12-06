using CommunityToolkit.Mvvm.ComponentModel;

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
