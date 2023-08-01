using ColorMC.Core.Nbt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Chunk;

public class ChunkNbt : NbtCompound
{
    public int X { get; set; }
    public int Z { get; set; }
}
