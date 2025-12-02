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
    public long ValueLong { get; set; }

    public override string Value
    {
        get => ValueLong.ToString();
        set => ValueLong = long.Parse(value);
    }

    public NbtLong()
    {
        NbtType = NbtType.NbtLong;
    }

    internal override NbtLong Read(DataInputStream stream)
    {
        ValueLong = stream.ReadLong();
        return this;
    }

    internal override void Write(DataOutputStream stream)
    {
        stream.Write(ValueLong);
    }
}
