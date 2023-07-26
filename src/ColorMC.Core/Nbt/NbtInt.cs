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
    public new int Value { get; set; }

    public NbtInt()
    {
        NbtType = NbtType.NbtInt;
    }

    internal override NbtInt Read(DataInputStream stream)
    {
        Value = stream.ReadInt();
        return this;
    }

    internal override void Write(DataOutputStream stream)
    {
        stream.Write(Value);
    }
}
