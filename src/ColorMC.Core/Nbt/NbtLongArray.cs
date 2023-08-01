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
        for (var a = 0; a < length; a++)
        {
            Value.Add(stream.ReadLong());
        }
        return this;
    }

    internal override void Write(DataOutputStream stream)
    {
        stream.Write(Value.Count);

        foreach (var item in Value)
        {
            stream.Write(item);
        }
    }
}
