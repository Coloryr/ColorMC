using System.Collections;

namespace ColorMC.Core.Nbt;

/// <summary>
/// ByteArray类型的NBT标签
/// </summary>
public class NbtByteArray : NbtBase, IEnumerable<byte>
{
    /// <summary>
    /// NBT码
    /// </summary>
    public const NbtType Type = NbtType.NbtByteArray;

    /// <summary>
    /// 数据
    /// </summary>
    public new List<byte> Value { get; set; }

    public NbtByteArray()
    {
        NbtType = NbtType.NbtByteArray;
        Value ??= new();
    }

    public IEnumerator<byte> GetEnumerator()
    {
        return Value.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Value.GetEnumerator();
    }

    internal override NbtByteArray Read(DataInputStream stream)
    {
        var length = stream.ReadInt();
        var list = new byte[length];
        stream.Read(list);
        Value.AddRange(list);
        return this;
    }

    internal override void Write(DataOutputStream stream)
    {
        stream.Write(Value.Count);
        stream.Write(Value.ToArray());
    }
}
