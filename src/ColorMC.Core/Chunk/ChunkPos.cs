namespace ColorMC.Core.Chunk;

public record ChunkPos
{
    public int Index { get; set; }
    public int Pos { get; set; }
    public byte Count { get; set; }
    public int Time { get; set; }
    public int Size { get; set; }

    public ChunkPos(int pos, byte count, int time, int index)
    {
        Pos = pos;
        Count = count;
        Time = time;
        Index = index;
    }
}
