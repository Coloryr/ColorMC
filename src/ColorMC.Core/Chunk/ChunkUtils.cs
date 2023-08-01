using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Chunk;

public static class ChunkUtils
{
    public static (int X, int Z) PosToChunk(int x, int z)
    {
        return (x >> 4, z >> 4);
    }

    public static (int X, int Z) ChunkToRegion(int x, int z)
    {
        return (x >> 5, z >> 5);
    }

    public static (int, int) PosToRegion(int x, int z)
    {
        (x, z) = PosToChunk(x, z);
        return ChunkToRegion(x, z);
    }

    public static int PosToChunkPos(int x, int y, int z)
    {
        return y << 8 | z << 4 | x;
    }

    public static int ChunkToHeadPos(int x, int z)
    {
        return (x & 31) + (z & 31) * 32;
    }
}
