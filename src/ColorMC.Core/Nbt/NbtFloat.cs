using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Nbt;

public class NbtFloat : NbtBase
{
    public const int Size = 96;
    public const byte Type = 5;

    public float Value { get; set; }

    public NbtFloat()
    {
        NbtType = NbtType.NbtFloat;
    }

    public override NbtFloat Read(DataInputStream stream)
    {
        Value = stream.ReadFloat();
        return this;
    }

    public override void Write(DataOutputStream stream)
    {
        stream.Write(Value);
    }
}
