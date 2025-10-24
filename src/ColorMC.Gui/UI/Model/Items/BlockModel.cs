using Avalonia.Media.Imaging;
using ColorMC.Gui.UI.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 方块
/// </summary>
/// <param name="key">方块ID</param>
/// <param name="name">显示名字</param>
/// <param name="group">显示ID</param>
/// <param name="bitmap">图片</param>
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
