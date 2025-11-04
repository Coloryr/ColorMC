namespace ColorMC.Core.Nbt;

/// <summary>
/// Float类型的NBT标签
/// </summary>
public class NbtFloat : NbtBase
{
    /// <summary>
    /// NBT码
    /// </summary>
    public const NbtType Type = NbtType.NbtFloat;

    /// <summary>
    /// 数据
    /// </summary>
    public float ValueFloat { get; set; }

    public override string Value 
    { 
        get => ValueFloat.ToString(); 
        set => ValueFloat = float.Parse(value); 
    }

    public NbtFloat()
    {
        NbtType = NbtType.NbtFloat;
    }

    internal override NbtFloat Read(DataInputStream stream)
    {
        ValueFloat = stream.ReadFloat();
        return this;
    }

    internal override void Write(DataOutputStream stream)
    {
        stream.Write(ValueFloat);
    }
}
