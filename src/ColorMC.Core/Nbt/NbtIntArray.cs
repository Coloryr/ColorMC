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
    public List<int> ValueIntArray { get; set; }

    public override string Value 
    { 
        get => $"[{ValueIntArray.Count}]"; 
        set => throw new NotSupportedException(); 
    }

    public NbtIntArray()
    {
        NbtType = NbtType.NbtIntArray;
        ValueIntArray ??= new();
    }

    internal override NbtIntArray Read(DataInputStream stream)
    {
        var length = stream.ReadInt();
        for (var a = 0; a < length; a++)
        {
            ValueIntArray.Add(stream.ReadInt());
        }
        return this;
    }

    internal override void Write(DataOutputStream stream)
    {
        stream.Write(ValueIntArray.Count);

        foreach (var item in ValueIntArray)
        {
            stream.Write(item);
        }
    }
}
