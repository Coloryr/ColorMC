namespace ColorMC.Core.Nbt;

/// <summary>
/// Short类型的NBT标签
/// </summary>
public class NbtShort : NbtBase
{
    /// <summary>
    /// NBT码
    /// </summary>
    public const NbtType Type = NbtType.NbtShort;

    /// <summary>
    /// 数据
    /// </summary>
    public short ValueShort { get; set; }

    public override string Value 
    {
        get => ValueShort.ToString(); 
        set => ValueShort = short.Parse(value); 
    }

    public NbtShort()
    {
        NbtType = NbtType.NbtShort;
    }

    internal override NbtShort Read(DataInputStream stream)
    {
        ValueShort = stream.ReadShort();
        return this;
    }

    internal override void Write(DataOutputStream stream)
    {
        stream.Write(ValueShort);
    }
}
