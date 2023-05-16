using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Nbt;

public class NbtLong : NbtBase
{
    public const int Type = 4;

    public long Value { get; set; }

    public NbtLong()
    {
        NbtType = NbtType.NbtLong;
    }

    public override NbtLong Read(DataInputStream stream)
    {
        Value = stream.ReadLong();
        return this;
    }

    public override void Write(DataOutputStream stream)
    {
        stream.Write(Value);
    }
}
