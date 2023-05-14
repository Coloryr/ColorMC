using ColorMC.Core.Helpers;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Nbt;

public abstract class NbtBase
{
    public NbtType NbtType { get; protected init; }

    public abstract NbtBase Read(DataInputStream stream);

    public abstract void Write(DataOutputStream stream);

    public override string? ToString()
    {
        return NbtType switch
        {
            NbtType.NbtByte => (this as NbtByte)?.Value.ToString(),
            NbtType.NbtShort => (this as NbtShort)?.Value.ToString(),
            NbtType.NbtInt => (this as NbtInt)?.Value.ToString(),
            NbtType.NbtLong => (this as NbtLong)?.Value.ToString(),
            NbtType.NbtFloat => (this as NbtFloat)?.Value.ToString(),
            NbtType.NbtDouble => (this as NbtDouble)?.Value.ToString(),
            NbtType.NbtString => (this as NbtString)?.Value.ToString(),
            NbtType.NbtByteArray => string.Format(LanguageHelper.GetName("Core.Game.Info"),
             (this as NbtByteArray)?.Value.Count),
            NbtType.NbtList => string.Format(LanguageHelper.GetName("Core.Game.Info"),
             (this as NbtList)?.Count),
            NbtType.NbtIntArray => string.Format(LanguageHelper.GetName("Core.Game.Info"),
             (this as NbtIntArray)?.Value.Count),
            NbtType.NbtLongArray => string.Format(LanguageHelper.GetName("Core.Game.Info"),
             (this as NbtLongArray)?.Value.Count),
            NbtType.NbtCompound => string.Format(LanguageHelper.GetName("Core.Game.Info"),
             (this as NbtCompound)?.Count),
            _ => ""
        };
    }

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

    public static NbtBase? Read(string file)
    {
        using var steam = File.OpenRead(file);
        using var steam1 = new GZipStream(steam, CompressionMode.Decompress);
        using var steam2 = new DataInputStream(steam1);

        var type = steam2.ReadByte();
        if (type == 0)
        {
            return new NbtEnd();
        }

        if (type == 10)
        {
            var temp = steam2.ReadShort();
            if (temp > 0)
            {
                var temp1 = new byte[temp];
                steam2.Read(temp1);
            }
        }
        var nbt = ById(type);
        
        return nbt.Read(steam2);
    }

    public static void Write(string file, NbtBase nbt)
    {
        using var steam = File.Create(file);
        using var steam1 = new GZipStream(steam, CompressionMode.Compress);
        using var steam2 = new DataOutputStream(steam1);

        steam2.Write((byte)nbt.NbtType);
        if (nbt.NbtType != NbtType.NbtEnd)
        {
            steam2.Write("");
            nbt.Write(steam2);
        }
    }
}
