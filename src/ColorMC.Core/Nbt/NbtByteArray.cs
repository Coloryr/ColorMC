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
    public List<byte> ValueByteArray { get; set; }

    public override string Value
    {
        get => $"[{ValueByteArray.Count}]";
        set => throw new NotSupportedException();
    }

    public NbtByteArray()
    {
        NbtType = NbtType.NbtByteArray;
        ValueByteArray ??= [];
    }

    public IEnumerator<byte> GetEnumerator()
    {
        return ValueByteArray.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ValueByteArray.GetEnumerator();
    }

    internal override NbtByteArray Read(DataInputStream stream)
    {
        var length = stream.ReadInt();
        var list = new byte[length];
        stream.Read(list);
        ValueByteArray.AddRange(list);
        return this;
    }

    internal override void Write(DataOutputStream stream)
    {
        stream.Write(ValueByteArray.Count);
        stream.Write([.. ValueByteArray]);
    }
}
