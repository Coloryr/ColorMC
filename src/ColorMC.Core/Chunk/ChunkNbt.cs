using ColorMC.Core.Nbt;

namespace ColorMC.Core.Chunk;

/// <summary>
/// 区块NBT
/// </summary>
public class ChunkNbt : NbtCompound
{
    /// <summary>
    /// 区块X坐标
    /// </summary>
    public int X { get; set; }
    /// <summary>
    /// 区块Z坐标
    /// </summary>
    public int Z { get; set; }
}
