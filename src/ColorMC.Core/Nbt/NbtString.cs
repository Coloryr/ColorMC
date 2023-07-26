namespace ColorMC.Core.Nbt;

/// <summary>
/// String类型的NBT标签
/// </summary>
public class NbtString : NbtBase
{
    /// <summary>
    /// NBT码
    /// </summary>
    public const NbtType Type = NbtType.NbtString;

    /// <summary>
    /// 数据
    /// </summary>
    public new string Value { get; set; }

    public NbtString()
    {
        NbtType = NbtType.NbtString;
        Value ??= "";
    }

    internal override NbtString Read(DataInputStream stream)
    {
        Value = stream.ReadString();
        return this;
    }

    internal override void Write(DataOutputStream stream)
    {
        stream.Write(Value);
    }
}
