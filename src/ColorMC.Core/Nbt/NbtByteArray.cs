using System.Collections;

namespace ColorMC.Core.Nbt;

public class NbtByteArray : NbtBase, IEnumerable<byte>
{
    public const byte Type = 7;

    public new List<byte> Value { get; set; }

    public NbtByteArray()
    {
        NbtType = NbtType.NbtByteArray;
        Value ??= new();
    }

    public IEnumerator<byte> GetEnumerator()
    {
        return Value.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Value.GetEnumerator();
    }

    public override NbtByteArray Read(DataInputStream stream)
    {
        var length = stream.ReadInt();
        var list = new byte[length];
        stream.Read(list);
        Value.AddRange(list);
        return this;
    }

    public override void Write(DataOutputStream stream)
    {
        stream.Write(Value.Count);
        stream.Write(Value.ToArray());
    }
}
