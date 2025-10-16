using Avalonia.Media.Imaging;
using ColorMC.Gui.UI.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

public partial class BlockItemModel(string key, string name, string group, Bitmap? bitmap) : ObservableObject
{
    public IBlockTop? Top;

    [ObservableProperty]
    private double _left;

    public string Key => key;

    public string Name => name;
    public string Group => group;
    public Bitmap? Bitmap => bitmap;

    public void Use()
    {
        Top?.Use(this);
    }
}
