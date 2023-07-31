using ColorMC.Core.Nbt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Chunk;

public record ChunkData
{
    public NbtList Nbt;
    public ChunkPos[] Pos = new ChunkPos[1024];
}
