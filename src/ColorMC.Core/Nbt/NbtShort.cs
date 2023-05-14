using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Nbt;

public class NbtShort : NbtBase
{
    public const int Size = 128;
    public const byte Type = 2;

    public short Value { get; set; }

    public NbtShort()
    {
        NbtType = NbtType.NbtShort;
    }

    public override NbtShort Read(DataInputStream stream)
    {
        Value = stream.ReadShort();
        return this;
    }

    public override void Write(DataOutputStream stream)
    {
        stream.Write(Value);
    }
}
