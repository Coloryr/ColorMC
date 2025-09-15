namespace ColorMC.Core.Nbt;

/// <summary>
/// NBT类型
/// </summary>
public enum NbtType
{
    NbtEnd = 0,
    NbtByte = 1,
    NbtShort = 2,
    NbtInt = 3,
    NbtLong = 4,
    NbtFloat = 5,
    NbtDouble = 6,
    NbtByteArray = 7,
    NbtString = 8,
    NbtList = 9,
    NbtCompound = 10,
    NbtIntArray = 11,
    NbtLongArray = 12
}

/// <summary>
/// Nbt压缩类型
/// </summary>
public enum ZipType : byte
{
    GZip = 1,
    Zlib = 2,
    None = 3,
    LZ4 = 4
}
