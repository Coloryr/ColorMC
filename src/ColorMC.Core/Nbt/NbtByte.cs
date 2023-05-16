using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Nbt;

public class NbtByte : NbtBase
{
    public const byte Type = 1;

    public byte Value { get; set; } 

    public NbtByte()
    {
        NbtType = NbtType.NbtByte;
    }

    public override NbtBase Read(DataInputStream stream)
    {
        Value = stream.ReadByte();
        return this;
    }

    public override void Write(DataOutputStream stream)
    {
        stream.Write(Value);
    }
}
