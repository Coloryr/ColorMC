using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    private static readonly Dictionary<byte, Type> VALUES = new()
    {
        {0, typeof(NbtEnd) },
        {1, typeof(NbtByte) },
        {2, typeof(NbtShort) },
        {3, typeof(NbtInt) },
        {4, typeof(NbtLong) },
        {5, typeof(NbtFloat) },
        {6, typeof(NbtDouble) },
        {7, typeof(NbtByteArray) },
        {8, typeof(NbtString) },
        {9, typeof(NbtList) },
        {10, typeof(NbtCompound) },
        {11, typeof(NbtIntArray) },
        {12, typeof(NbtLongArray) }
    };

    public static NbtBase ById(byte id)
    {
        var type = VALUES[id];
        return (Activator.CreateInstance(type) as NbtBase)!;
    }
}
