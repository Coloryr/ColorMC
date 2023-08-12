using ColorMC.Core.Nbt;

namespace ColorMC.Core.Chunk;

public record ChunkData
{
    public NbtList Nbt;
    public ChunkPos[] Pos = new ChunkPos[1024];
}
