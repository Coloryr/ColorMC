using ColorMC.Core.Objs;

namespace ColorMC.Core.Chunk;

/// <summary>
/// 区块计算用
/// </summary>
public static class ChunkUtils
{
    /// <summary>
    /// 坐标转区块坐标
    /// </summary>
    /// <param name="pos">坐标 X Z</param>
    /// <returns>>区块 X Z</returns>
    public static PointStruct PosToChunk(PointStruct pos)
    {
        return new PointStruct(pos.X >> 4, pos.Y >> 4);
    }

    /// <summary>
    /// 区块转MCA坐标
    /// </summary>
    /// <param name="pos">区块 X Z</param>
    /// <returns>MCA X Z</returns>
    public static PointStruct ChunkToRegion(PointStruct pos)
    {
        return new PointStruct(pos.X >> 5, pos.Y >> 5);
    }

    /// <summary>
    /// 区块坐标转文件头位置
    /// </summary>
    public static int ChunkToHeadPos(PointStruct pos)
    {
        return (pos.X & 31) + (pos.Y & 31) * 32;
    }
}
