using ColorMC.Core.Nbt;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace ColorMC.Gui.UI.Model.ConfigEdit;

public partial class NbtNodeModel : ObservableObject
{
    private readonly NbtBase nbt;

    public string? Name => key == null ? nbt.ToString() : $"{key}: {nbt}";
    public string? key;
    public NbtType NbtType => nbt.NbtType;

    [ObservableProperty]
    private ObservableCollection<NbtNodeModel> children;
    [ObservableProperty]
    public bool isExpanded;
    [ObservableProperty]
    public long? size;
    [ObservableProperty]
    public bool hasChildren;

    public NbtNodeModel(string? key, NbtBase nbt)
    {
        this.nbt = nbt;
        this.key = key;
        HasChildren = nbt.IsGroup() && nbt.HaveItem();
        LoadChildren();
    }

    private void LoadChildren()
    {
        if (!HasChildren)
        {
            Children = new();
        }

        var result = new ObservableCollection<NbtNodeModel>();

        if (nbt is NbtList list)
        {
            foreach (var item in list)
            {
                result.Add(new NbtNodeModel(null, item));
            }
        }

        if (nbt is NbtCompound list1)
        {
            foreach (var item in list1)
            {
                result.Add(new NbtNodeModel(item.Key, item.Value));
            }
        }

        if (result.Count == 0)
            HasChildren = false;

        Children = result;
    }
}
