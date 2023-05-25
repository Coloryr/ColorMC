using ColorMC.Core.Nbt;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;

namespace ColorMC.Gui.UI.Model.ConfigEdit;

public partial class NbtNodeModel : ObservableObject
{
    public readonly NbtBase Nbt;

    public string? Name => Key == null ? Nbt.ToString() : $"{Key}: {Nbt}";
    public string? Key { get; set; }
    public NbtType NbtType => Nbt.NbtType;

    public ObservableCollection<NbtNodeModel> Children { get; init; } = new();

    [ObservableProperty]
    public bool isExpanded;
    [ObservableProperty]
    public long? size;
    [ObservableProperty]
    public bool hasChildren;

    public NbtNodeModel? Top { get; }

    public NbtNodeModel(string? key, NbtBase nbt, NbtNodeModel? top)
    {
        Nbt = nbt;
        Key = key;
        Top = top;
        LoadChildren();
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
            OnPropertyChanged(nameof(Name));
        }
    }

    public void SetValue(string value)
    {
        Nbt.SetValue(value);
        OnPropertyChanged(nameof(Name));
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
}
