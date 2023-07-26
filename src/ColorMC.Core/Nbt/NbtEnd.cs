namespace ColorMC.Core.Nbt;

/// <summary>
/// 结束类型的NBT标签
/// </summary>
public class NbtEnd : NbtBase
{
    /// <summary>
    /// NBT码
    /// </summary>
    public const NbtType Type = NbtType.NbtEnd;

    public NbtEnd()
    {
        NbtType = NbtType.NbtEnd;
    }

    internal override NbtEnd Read(DataInputStream stream)
    {
        return this;
    }

    internal override void Write(DataOutputStream stream)
    {

    }
}
