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
    public static bool IsGroup(this NbtType type) 
    {
        return type switch
        {
            NbtType.NbtList => true,
            NbtType.NbtCompound => true,
            _ => false
        };
    }
}
