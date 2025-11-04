namespace ColorMC.Core.Nbt;

/// <summary>
/// Int类型的NBT标签
/// </summary>
public class NbtInt : NbtBase
{
    /// <summary>
    /// NBT码
    /// </summary>
    public const NbtType Type = NbtType.NbtInt;

    /// <summary>
    /// 数据
    /// </summary>
    public int ValueInt { get; set; }

    public override string Value 
    {
        get => ValueInt.ToString();
        set => ValueInt = int.Parse(value); 
    }

    public NbtInt()
    {
        NbtType = NbtType.NbtInt;
    }

    internal override NbtInt Read(DataInputStream stream)
    {
        ValueInt = stream.ReadInt();
        return this;
    }

    internal override void Write(DataOutputStream stream)
    {
        stream.Write(ValueInt);
    }
}
