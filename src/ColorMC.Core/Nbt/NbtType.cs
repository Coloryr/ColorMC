﻿namespace ColorMC.Core.Nbt;

public enum NbtType : byte
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
