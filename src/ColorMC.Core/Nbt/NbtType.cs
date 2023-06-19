namespace ColorMC.Core.Nbt;

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

public static class NbtTypes
{
    public static readonly Dictionary<byte, Type> VALUES = new()
    {
        {NbtEnd.Type, typeof(NbtEnd) },
        {NbtByte.Type, typeof(NbtByte) },
        {NbtShort.Type, typeof(NbtShort) },
        {NbtInt.Type, typeof(NbtInt) },
        {NbtLong.Type, typeof(NbtLong) },
        {NbtFloat.Type, typeof(NbtFloat) },
        {NbtDouble.Type, typeof(NbtDouble) },
        {NbtByteArray.Type, typeof(NbtByteArray) },
        {NbtString.Type, typeof(NbtString) },
        {NbtList.Type, typeof(NbtList) },
        {NbtCompound.Type, typeof(NbtCompound) },
        {NbtIntArray.Type, typeof(NbtIntArray) },
        {NbtLongArray.Type, typeof(NbtLongArray) }
    };

    public static readonly Dictionary<NbtType, Type> VALUES1 = new()
    {
        {NbtType.NbtEnd, typeof(NbtEnd) },
        {NbtType.NbtByte, typeof(NbtByte) },
        {NbtType.NbtShort, typeof(NbtShort) },
        {NbtType.NbtInt, typeof(NbtInt) },
        {NbtType.NbtLong, typeof(NbtLong) },
        {NbtType.NbtFloat, typeof(NbtFloat) },
        {NbtType.NbtDouble, typeof(NbtDouble) },
        {NbtType.NbtByteArray, typeof(NbtByteArray) },
        {NbtType.NbtString, typeof(NbtString) },
        {NbtType.NbtList, typeof(NbtList) },
        {NbtType.NbtCompound, typeof(NbtCompound) },
        {NbtType.NbtIntArray, typeof(NbtIntArray) },
        {NbtType.NbtLongArray, typeof(NbtLongArray) }
    };
}
