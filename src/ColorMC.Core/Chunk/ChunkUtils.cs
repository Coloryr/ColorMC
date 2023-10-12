namespace ColorMC.Core.Chunk;

/// <summary>
/// 区块计算用
/// </summary>
public static class ChunkUtils
{
    /// <summary>
    /// 坐标转区块坐标
    /// </summary>
    public static (int X, int Z) PosToChunk(int x, int z)
    {
        return (x >> 4, z >> 4);
    }

    /// <summary>
    /// 区块转MCA坐标
    /// </summary>
    public static (int X, int Z) ChunkToRegion(int x, int z)
    {
        return (x >> 5, z >> 5);
    }

    //public static (int, int) PosToRegion(int x, int z)
    //{
    //    (x, z) = PosToChunk(x, z);
    //    return ChunkToRegion(x, z);
    //}

    //public static int PosToChunkPos(int x, int y, int z)
    //{
    //    return y << 8 | z << 4 | x;
    //}

    /// <summary>
    /// 区块坐标转文件头位置
    /// </summary>
    public static int ChunkToHeadPos(int x, int z)
    {
        return (x & 31) + (z & 31) * 32;
    }
}
