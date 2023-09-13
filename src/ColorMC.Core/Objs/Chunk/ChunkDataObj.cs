using ColorMC.Core.Nbt;

namespace ColorMC.Core.Objs.Chunk;

/// <summary>
/// 区块数据
/// </summary>
public record ChunkDataObj
{
    /// <summary>
    /// NBT标签
    /// </summary>
    public NbtList Nbt;
    /// <summary>
    /// 区块地址数据
    /// </summary>
    public ChunkPosObj[] Pos = new ChunkPosObj[1024];
}
