using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Nbt;

public class NbtInt :NbtBase
{
    public const int Size = 96;
    public const byte Type = 3;

    public int Value { get; set; }

    public NbtInt()
    {
        NbtType = NbtType.NbtInt;
    }

    public override NbtInt Read(DataInputStream stream)
    {
        Value = stream.ReadInt();
        return this;
    }

    public override void Write(DataOutputStream stream)
    {
        stream.Write(Value);
    }
}
