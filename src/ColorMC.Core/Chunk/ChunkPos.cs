using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Chunk;

public record ChunkPos
{
    public int Pos { get; set; }
    public byte Count { get; set; }
    public int Time { get; set; }

    public ChunkPos(int pos, byte count, int time)
    {
        Pos = pos;
        Count = count;
        Time = time;
    }
}
