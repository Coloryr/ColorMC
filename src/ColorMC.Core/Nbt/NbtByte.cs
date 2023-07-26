namespace ColorMC.Core.Nbt;

/// <summary>
/// Byte类型的NBT标签
/// </summary>
public class NbtByte : NbtBase
{
    /// <summary>
    /// NBT码
    /// </summary>
    public const NbtType Type = NbtType.NbtByte;

    /// <summary>
    /// 数据
    /// </summary>
    public new byte Value { get; set; }

    public NbtByte()
    {
        NbtType = NbtType.NbtByte;
    }

    internal override NbtBase Read(DataInputStream stream)
    {
        Value = stream.ReadByte();
        return this;
    }

    internal override void Write(DataOutputStream stream)
    {
        stream.Write(Value);
    }
}
