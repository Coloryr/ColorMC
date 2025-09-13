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
    public static PointPos PosToChunk(PointPos pos)
    {
        return new PointPos(pos.X >> 4, pos.Y >> 4);
    }

    /// <summary>
    /// 区块转MCA坐标
    /// </summary>
    /// <param name="pos">区块 X Z</param>
    /// <returns>MCA X Z</returns>
    public static PointPos ChunkToRegion(PointPos pos)
    {
        return new PointPos(pos.X >> 5, pos.Y >> 5);
    }

    /// <summary>
    /// 区块坐标转文件头位置
    /// </summary>
    public static int ChunkToHeadPos(PointPos pos)
    {
        return (pos.X & 31) + (pos.Y & 31) * 32;
    }
}
