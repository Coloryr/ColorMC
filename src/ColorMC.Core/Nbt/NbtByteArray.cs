using System.Collections;

namespace ColorMC.Core.Nbt;

public class NbtByteArray : NbtBase, IEnumerable<byte>
{
    public const byte Type = 7;

    public List<byte> Values { get; set; }

    public NbtByteArray()
    {
        NbtType = NbtType.NbtByteArray;
        Values ??= new();
    }

    public IEnumerator<byte> GetEnumerator()
    {
        return Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Values.GetEnumerator();
    }

    public override NbtByteArray Read(DataInputStream stream)
    {
        var length = stream.ReadInt();
        var list = new byte[length];
        stream.Read(list);
        Values.AddRange(list);
        return this;
    }

    public override void Write(DataOutputStream stream)
    {
        stream.Write(Values.Count);
        stream.Write(Values.ToArray());
    }
}
