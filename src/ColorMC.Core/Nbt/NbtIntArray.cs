namespace ColorMC.Core.Nbt;

/// <summary>
/// IntArray类型的NBT标签
/// </summary>
public class NbtIntArray : NbtBase
{
    /// <summary>
    /// NBT码
    /// </summary>
    public const NbtType Type = NbtType.NbtIntArray;

    /// <summary>
    /// 数据
    /// </summary>
    public new List<int> Value { get; set; }

    public NbtIntArray()
    {
        NbtType = NbtType.NbtIntArray;
        Value ??= new();
    }

    internal override NbtIntArray Read(DataInputStream stream)
    {
        var length = stream.ReadInt();
        for (var a = 0; a < length; a++)
        {
            Value.Add(stream.ReadInt());
        }
        return this;
    }

    internal override void Write(DataOutputStream stream)
    {
        stream.Write(Value.Count);

        foreach (var item in Value)
        {
            stream.Write(item);
        }
    }
}
