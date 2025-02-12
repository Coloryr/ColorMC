using System;
using System.Collections.ObjectModel;
using ColorMC.Core.Chunk;
using ColorMC.Core.Nbt;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// Nbt标签，用于显示
/// </summary>
public partial class NbtNodeModel : ObservableObject
{
    /// <summary>
    /// Nbt标签
    /// </summary>
    public readonly NbtBase Nbt;

    /// <summary>
    /// 名字
    /// </summary>
    public string? Name => GetName();
    /// <summary>
    /// 键
    /// </summary>
    public string? Key { get; set; }
    /// <summary>
    /// 类型
    /// </summary>
    public NbtType NbtType => Nbt.NbtType;

    /// <summary>
    /// 子标签
    /// </summary>
    public ObservableCollection<NbtNodeModel> Children { get; init; } = [];

    /// <summary>
    /// 是否展开
    /// </summary>
    [ObservableProperty]
    private bool _isExpanded;
    /// <summary>
    /// 长度
    /// </summary>
    [ObservableProperty]
    private long? _size;
    /// <summary>
    /// 是否有子标签
    /// </summary>
    [ObservableProperty]
    private bool _hasChildren;

    /// <summary>
    /// 父标签
    /// </summary>
    public NbtNodeModel? Parent { get; }

    public NbtNodeModel(string? key, NbtBase nbt, NbtNodeModel? top)
    {
        Nbt = nbt;
        Key = key;
        Parent = top;
        LoadChildren();
    }

    /// <summary>
    /// 获取名字
    /// </summary>
    /// <returns></returns>
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
        {
            return $"{Key}: {Nbt}";
        }
    }

    /// <summary>
    /// 添加一个Nbt子标签
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="type">类型</param>
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

    /// <summary>
    /// 删除一个Nbt子标签
    /// </summary>
    /// <param name="model"></param>
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

    /// <summary>
    /// 编辑键
    /// </summary>
    /// <param name="old"></param>
    /// <param name="now"></param>
    public void EditKey(string old, string now)
    {
        if (Parent == null)
        {
            return;
        }

        if (Parent.NbtType == NbtType.NbtCompound)
        {
            var list = (Parent.Nbt as NbtCompound)!;
            list.EditKey(old, now);
            Key = now;
            Update();
        }
    }

    /// <summary>
    /// 设置值
    /// </summary>
    /// <param name="value"></param>
    public void SetValue(string value)
    {
        Nbt.Value = value;
        Update();
    }

    /// <summary>
    /// 更新名字
    /// </summary>
    public void Update()
    {
        OnPropertyChanged(nameof(Name));
    }

    /// <summary>
    /// 查找标签
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public NbtNodeModel? Find(string name)
    {
        if (name.Equals(Key, StringComparison.OrdinalIgnoreCase))
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

    /// <summary>
    /// 查找标签
    /// </summary>
    /// <param name="nbt"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 加载子标签列表
    /// </summary>
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
        else if (Nbt is NbtCompound list1)
        {
            foreach (var item in list1)
            {
                Children.Add(new NbtNodeModel(item.Key, item.Value, this));
            }
        }

        HasChildren = Children.Count != 0;

        if (HasChildren && Parent == null)
        {
            IsExpanded = true;
        }
    }

    /// <summary>
    /// 收起列表
    /// </summary>
    public void UnExpand()
    {
        IsExpanded = false;
        OnPropertyChanged(nameof(IsExpanded));
    }

    /// <summary>
    /// 展开列表
    /// </summary>
    public void Expand()
    {
        IsExpanded = true;
        OnPropertyChanged(nameof(IsExpanded));
    }
}
