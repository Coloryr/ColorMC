namespace ColorMC.Core.Nbt;

/// <summary>
/// Double类型的NBT标签
/// </summary>
public class NbtDouble : NbtBase
{
    /// <summary>
    /// NBT码
    /// </summary>
    public const NbtType Type = NbtType.NbtDouble;

    /// <summary>
    /// 数据
    /// </summary>
    public double ValueDouble { get; set; }

    public override string Value 
    { 
        get => ValueDouble.ToString(); 
        set => ValueDouble = double.Parse(value); 
    }

    public NbtDouble()
    {
        NbtType = NbtType.NbtDouble;
    }

    internal override NbtDouble Read(DataInputStream stream)
    {
        ValueDouble = stream.ReadDouble();
        return this;
    }

    internal override void Write(DataOutputStream stream)
    {
        stream.Write(ValueDouble);
    }
}
