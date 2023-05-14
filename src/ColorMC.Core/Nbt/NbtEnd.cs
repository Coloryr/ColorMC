using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Nbt;

public class NbtEnd : NbtBase
{
    public const int Size = 64;
    public const byte Type = 0;

    public NbtEnd()
    {
        NbtType = NbtType.NbtEnd;
    }

    public override NbtEnd Read(DataInputStream stream)
    {
        return this;
    }

    public override void Write(DataOutputStream stream)
    {
        
    }
}
