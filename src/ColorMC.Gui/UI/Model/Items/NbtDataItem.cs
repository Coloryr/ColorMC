using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

public partial class NbtDataItem : ObservableObject
{
    [ObservableProperty]
    private int key;
    [ObservableProperty]
    private object value;
}
