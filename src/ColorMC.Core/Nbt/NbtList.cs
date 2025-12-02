using System.Collections;

namespace ColorMC.Core.Nbt;

/// <summary>
/// 列表类型的NBT标签
/// </summary>
public class NbtList : NbtBase, IEnumerable<NbtBase>
{
    /// <summary>
    /// NBT码
    /// </summary>
    public const NbtType Type = NbtType.NbtList;

    private readonly List<NbtBase> _values = [];

    /// <summary>
    /// 储存的NBT类型
    /// </summary>
    public NbtType InNbtType { get; set; }

    /// <summary>
    /// 数据操作
    /// </summary>
    public NbtBase this[int index]
    {
        get => _values[index];
        set => _values[index] = value;
    }

    /// <summary>
    /// NBT数量
    /// </summary>
    public int Count => _values.Count;

    public override string Value
    {
        get => $"[{_values.Count}]";
        set => throw new NotSupportedException();
    }

    public NbtList()
    {
        NbtType = NbtType.NbtList;
        InNbtType = NbtType.NbtEnd;
    }

    /// <summary>
    /// 添加一个NBT标签
    /// </summary>
    /// <param name="nbt">NBT标签</param>
    public void Add(NbtBase nbt)
    {
        if (nbt.NbtType != InNbtType)
        {
            return;
        }

        _values.Add(nbt);
    }

    /// <summary>
    /// 删除指定
    /// </summary>
    /// <param name="index">下标</param>
    public void RemoveAt(int index)
    {
        _values.RemoveAt(index);
    }

    /// <summary>
    /// 删除NBT标签
    /// </summary>
    /// <param name="item">NBT标签</param>
    public void Remove(NbtBase item)
    {
        _values.Remove(item);
    }

    /// <summary>
    /// 复制内容
    /// </summary>
    /// <returns>NBT列表</returns>
    public List<NbtBase> CopyList()
    {
        return new List<NbtBase>(_values);
    }

    internal override NbtList Read(DataInputStream stream)
    {
        var type = stream.ReadByte();
        InNbtType = (NbtType)type;
        int length = stream.ReadInt();
        if (NbtType == 0 && length > 0)
        {
            throw new Exception("Missing type on ListTag");
        }

        for (int a = 0; a < length; a++)
        {
            var nbt = ById((NbtType)type);
            nbt.Read(stream);
            _values.Add(nbt);
        }

        return this;
    }

    internal override void Write(DataOutputStream stream)
    {
        if (_values.Count == 0)
        {
            InNbtType = NbtType.NbtEnd;
        }
        else
        {
            InNbtType = _values[0].NbtType;
        }

        stream.Write((byte)InNbtType);
        stream.Write(_values.Count);

        foreach (var item in _values)
        {
            item.Write(stream);
        }
    }

    public IEnumerator<NbtBase> GetEnumerator()
    {
        return _values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _values.GetEnumerator();
    }
}
