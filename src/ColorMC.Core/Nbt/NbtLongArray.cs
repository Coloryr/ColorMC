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
    public new List<long> Value { get; set; }

    public NbtLongArray()
    {
        NbtType = NbtType.NbtLongArray;
        Value ??= new();
    }

    internal override NbtLongArray Read(DataInputStream stream)
    {
        var length = stream.ReadInt();
        var list = new byte[length * 8];
        var list1 = new long[length];
        stream.Read(list);

        Buffer.BlockCopy(list, 0, list1, 0, list.Length);
        Value.AddRange(list1);
        return this;
    }

    internal override void Write(DataOutputStream stream)
    {
        stream.Write(Value.Count);

        var list1 = Value.ToArray();
        var list = new byte[list1.Length * 8];
        Buffer.BlockCopy(list1, 0, list, 0, list.Length);

        stream.Write(list);
    }
}
