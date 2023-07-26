namespace ColorMC.Core.Nbt;

/// <summary>
/// Long类型的NBT标签
/// </summary>
public class NbtLong : NbtBase
{
    /// <summary>
    /// NBT码
    /// </summary>

    public const NbtType Type = NbtType.NbtLong;

    /// <summary>
    /// 数据
    /// </summary>
    public new long Value { get; set; }

    public NbtLong()
    {
        NbtType = NbtType.NbtLong;
    }

    internal override NbtLong Read(DataInputStream stream)
    {
        Value = stream.ReadLong();
        return this;
    }

    internal override void Write(DataOutputStream stream)
    {
        stream.Write(Value);
    }
}
