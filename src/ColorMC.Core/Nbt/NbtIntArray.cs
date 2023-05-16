using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Nbt;

public class NbtIntArray : NbtBase
{
    public const byte Type = 11;

    public List<int> Value { get; set; }

    public NbtIntArray()
    {
        NbtType = NbtType.NbtIntArray;
        Value ??= new();
    }

    public override NbtIntArray Read(DataInputStream stream)
    {
        var length = stream.ReadInt();
        var list = new byte[length * 4];
        var list1 = new int[length];
        stream.Read(list);
        Buffer.BlockCopy(list, 0, list1, 0, list.Length);
        Value.AddRange(list1);
        return this;
    }

    public override void Write(DataOutputStream stream)
    {
        stream.Write(Value.Count);

        var list1 = Value.ToArray();
        var list = new byte[list1.Length * 4];
        Buffer.BlockCopy(list1, 0, list, 0, list.Length);

        stream.Write(list);
    }
}
