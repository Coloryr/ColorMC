using ColorMC.Core.Chunk;
using ColorMC.Core.Nbt;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace ColorMC.Gui.UI.Model;

public partial class NbtNodeModel : ObservableObject
{
    public readonly NbtBase Nbt;

    public string? Name => GetName();
    public string? Key { get; set; }
    public NbtType NbtType => Nbt.NbtType;

    public ObservableCollection<NbtNodeModel> Children { get; init; } = new();

    [ObservableProperty]
    private bool _isExpanded;
    [ObservableProperty]
    private long? _size;
    [ObservableProperty]
    private bool _hasChildren;

    public NbtNodeModel? Top { get; }

    public NbtNodeModel(string? key, NbtBase nbt, NbtNodeModel? top)
    {
        Nbt = nbt;
        Key = key;
        Top = top;
        LoadChildren();
    }

    private string? GetName()
    {
        if (Nbt is ChunkNbt chunk)
        {
            return $"({chunk.X}, {chunk.Z})";
        }
        else if (Key == null)
        {
            return Nbt.ToString();
        }
        else

            return $"{Key}: {Nbt}";
    }

    public void Add(string key, NbtType type)
    {
        if (NbtType == NbtType.NbtList)
        {
            var list = (Nbt as NbtList)!;
            list.Add(NbtBase.ById(list.InNbtType));
            LoadChildren();
        }
        else if (NbtType == NbtType.NbtCompound)
        {
            var list = (Nbt as NbtCompound)!;
            list.Add(key, NbtBase.ById(type));
            LoadChildren();
        }
    }

    public void Remove(NbtNodeModel model)
    {
        if (NbtType == NbtType.NbtList)
        {
            var list = (Nbt as NbtList)!;
            list.Remove(model.Nbt);
            LoadChildren();
        }
        else if (NbtType == NbtType.NbtCompound)
        {
            var list = (Nbt as NbtCompound)!;
            list.Remove(model.Key!);
            LoadChildren();
        }
    }

    public void EditKey(string old, string now)
    {
        if (Top == null)
            return;

        if (Top.NbtType == NbtType.NbtCompound)
        {
            var list = (Top.Nbt as NbtCompound)!;
            list.EditKey(old, now);
            Key = now;
            Update();
        }
    }

    public void SetValue(string value)
    {
        Nbt.Value = value;
        Update();
    }

    public void Update()
    {
        OnPropertyChanged(nameof(Name));
    }

    public NbtNodeModel? Find(string name)
    {
        if (name.ToLower() == Key?.ToLower())
        {
            return this;
        }

        if (HasChildren)
        {
            foreach (var item in Children)
            {
                var item1 = item.Find(name);
                if (item1 != null)
                {
                    return item1;
                }
            }
        }

        return null;
    }

    public NbtNodeModel? Find(NbtBase nbt)
    {
        NbtNodeModel? model = null;
        foreach (var item in Children)
        {
            if (item.Nbt == nbt)
            {
                item.Expand();
                model = item;
            }
            else
            {
                item.UnExpand();
            }
        }

        return model;
    }

    private void LoadChildren()
    {
        HasChildren = Nbt.IsGroup() && Nbt.HaveItem();

        Children.Clear();

        if (!HasChildren)
        {
            return;
        }

        if (Nbt is NbtList list)
        {
            foreach (var item in list)
            {
                Children.Add(new NbtNodeModel(null, item, this));
            }
        }

        if (Nbt is NbtCompound list1)
        {
            foreach (var item in list1)
            {
                Children.Add(new NbtNodeModel(item.Key, item.Value, this));
            }
        }

        HasChildren = Children.Count != 0;

        if (HasChildren && Top == null)
        {
            IsExpanded = true;
        }
    }

    public void UnExpand()
    {
        IsExpanded = false;
        OnPropertyChanged(nameof(IsExpanded));
    }

    public void Expand()
    {
        IsExpanded = true;
        OnPropertyChanged(nameof(IsExpanded));
    }
}
