namespace ColorMC.Core.Nbt;

public class NbtIntArray : NbtBase
{
    public const byte Type = 11;

    public List<int> Values { get; set; }

    public NbtIntArray()
    {
        NbtType = NbtType.NbtIntArray;
        Values ??= new();
    }

    public override NbtIntArray Read(DataInputStream stream)
    {
        var length = stream.ReadInt();
        var list = new byte[length * 4];
        var list1 = new int[length];
        stream.Read(list);
        Buffer.BlockCopy(list, 0, list1, 0, list.Length);
        Values.AddRange(list1);
        return this;
    }

    public override void Write(DataOutputStream stream)
    {
        stream.Write(Values.Count);

        var list1 = Value.ToArray();
        var list = new byte[list1.Length * 4];
        Buffer.BlockCopy(list1, 0, list, 0, list.Length);

        stream.Write(list);
    }
}
