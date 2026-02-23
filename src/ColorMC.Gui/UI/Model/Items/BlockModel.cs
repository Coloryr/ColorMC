using Avalonia.Media.Imaging;
using ColorMC.Gui.UI.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 方块
/// </summary>
/// <param name="key">方块ID</param>
/// <param name="name">显示名字</param>
/// <param name="bitmap">图片</param>
/// <param name="count">数量</param>
public partial class BlockItemModel(string key, string? name, Bitmap? bitmap, int count) : ObservableObject
{
    public IBlockTop? Top;

    [ObservableProperty]
    private double _left;

    public string Key => key;

    public string? Name => name;
    public Bitmap? Bitmap => bitmap;

    public int Count => count;
    public bool ShowCount => count > 1;

    public void Use()
    {
        Top?.Use(this);
    }
}
