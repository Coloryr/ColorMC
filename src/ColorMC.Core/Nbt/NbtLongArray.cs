namespace ColorMC.Core.Nbt;

/// <summary>
/// DoubleArray类型的NBT标签
/// </summary>
public class NbtLongArray : NbtBase
{
    /// <summary>
    /// NBT码
    /// </summary>
    public const NbtType Type = NbtType.NbtLongArray;

    /// <summary>
    /// 数据
    /// </summary>
    public List<long> ValueLongArray { get; set; }
    public override string Value
    {
        get => $"[{ValueLongArray.Count}]";
        set => throw new NotSupportedException();
    }

    public NbtLongArray()
    {
        NbtType = NbtType.NbtLongArray;
        ValueLongArray ??= [];
    }

    internal override NbtLongArray Read(DataInputStream stream)
    {
        var length = stream.ReadInt();
        for (var a = 0; a < length; a++)
        {
            ValueLongArray.Add(stream.ReadLong());
        }
        return this;
    }

    internal override void Write(DataOutputStream stream)
    {
        stream.Write(ValueLongArray.Count);

        foreach (var item in ValueLongArray)
        {
            stream.Write(item);
        }
    }
}
