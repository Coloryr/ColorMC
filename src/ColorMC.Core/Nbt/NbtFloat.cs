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
    public new float Value { get; set; }

    public NbtFloat()
    {
        NbtType = NbtType.NbtFloat;
    }

    internal override NbtFloat Read(DataInputStream stream)
    {
        Value = stream.ReadFloat();
        return this;
    }

    internal override void Write(DataOutputStream stream)
    {
        stream.Write(Value);
    }
}
