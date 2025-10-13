using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

public partial class LotteryItemViewModel(string name, string group, Bitmap? bitmap) : ObservableObject
{
    [ObservableProperty]
    private double _left;

    public string Name => name;
    public string Group => group;
    public Bitmap? Bitmap => bitmap;
}
