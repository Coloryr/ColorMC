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
        var list = new byte[length * 4];
        var list1 = new int[length];
        stream.Read(list);
        Buffer.BlockCopy(list, 0, list1, 0, list.Length);
        Value.AddRange(list1);
        return this;
    }

    internal override void Write(DataOutputStream stream)
    {
        stream.Write(Value.Count);

        var list1 = Value.ToArray();
        var list = new byte[list1.Length * 4];
        Buffer.BlockCopy(list1, 0, list, 0, list.Length);

        stream.Write(list);
    }
}
