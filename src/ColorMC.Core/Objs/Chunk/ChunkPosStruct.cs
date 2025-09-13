namespace ColorMC.Core.Objs.Chunk;

/// <summary>
/// 区块头数据
/// </summary>
public struct ChunkPosStruct
{
    /// <summary>
    /// 序号
    /// </summary>
    public int Index { get; set; }
    /// <summary>
    /// 位置
    /// </summary>
    public int Pos { get; set; }
    /// <summary>
    /// 总计扇区数
    /// </summary>
    public byte Count { get; set; }
    /// <summary>
    /// 时间
    /// </summary>
    public int Time { get; set; }
    /// <summary>
    /// 实际大小
    /// </summary>
    public int Size { get; set; }

    public ChunkPosStruct()
    {
        
    }

    public ChunkPosStruct(int pos, byte count, int time, int index)
    {
        Pos = pos;
        Count = count;
        Time = time;
        Index = index;
    }
}
