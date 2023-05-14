using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Nbt;

public class NbtDouble : NbtBase
{
    public const int Size = 128;
    public const byte Type = 6;

    public double Value { get; set; }

    public NbtDouble()
    {
        NbtType = NbtType.NbtDouble;
    }

    public override NbtDouble Read(DataInputStream stream)
    {
        Value = stream.ReadDouble();
        return this;
    }

    public override void Write(DataOutputStream stream)
    {
        stream.Write(Value);
    }
}
