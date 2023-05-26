namespace ColorMC.Core.Nbt;

public class NbtLongArray : NbtBase
{
    public const byte Type = 12;

    public new List<long> Value { get; set; }

    public NbtLongArray()
    {
        NbtType = NbtType.NbtLongArray;
        Value ??= new();
    }

    public override NbtLongArray Read(DataInputStream stream)
    {
        var length = stream.ReadInt();
        var list = new byte[length * 8];
        var list1 = new long[length];
        stream.Read(list);

        Buffer.BlockCopy(list, 0, list1, 0, list.Length);
        Value.AddRange(list1);
        return this;
    }

    public override void Write(DataOutputStream stream)
    {
        stream.Write(Value.Count);

        var list1 = Value.ToArray();
        var list = new byte[list1.Length * 8];
        Buffer.BlockCopy(list1, 0, list, 0, list.Length);

        stream.Write(list);
    }
}
