using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Nbt;

public class NbtString :NbtBase
{
    public const int Size = 288;
    public const byte Type = 8;

    public string Value { get; set; }

    public NbtString()
    {
        NbtType = NbtType.NbtString;
        Value ??= "";
    }

    public override NbtString Read(DataInputStream stream)
    {
        Value = stream.ReadString();
        return this;
    }

    public override void Write(DataOutputStream stream)
    {
        stream.Write(Value);
    }
}
