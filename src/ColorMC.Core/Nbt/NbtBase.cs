using ColorMC.Core.Helpers;
using System.IO;
using System.IO.Compression;

namespace ColorMC.Core.Nbt;

public abstract class NbtBase
{
    public NbtType NbtType { get; protected init; }
    public bool Gzip { get; set; }

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

    public bool HaveItem()
    {
        return NbtType switch
        {
            NbtType.NbtList => (this as NbtList)?.Count > 0,
            NbtType.NbtCompound => (this as NbtCompound)?.Count > 0,
            _ => false
        };
    }

    public bool IsGroup()
    {
        return NbtType switch
        {
            NbtType.NbtList => true,
            NbtType.NbtCompound => true,
            _ => false
        };
    }

    private static readonly Dictionary<byte, Type> VALUES = new()
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

    public static NbtBase ById(byte id)
    {
        var type = VALUES[id];
        return (Activator.CreateInstance(type) as NbtBase)!;
    }

    public static NbtBase? Read(string file)
    {
        using var steam = File.OpenRead(file);
        DataInputStream steam2;
        var data = new byte[2];
        steam.ReadExactly(data);
        steam.Seek(0, SeekOrigin.Begin);
        bool gzip = false;
        if (data[0] == 0x1F && data[1] == 0x8B)
        {
            var steam1 = new GZipStream(steam, CompressionMode.Decompress);
            steam2 = new DataInputStream(steam1);
            gzip = true;
        }
        else
        {
            steam2 = new DataInputStream(steam);
        }

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
        nbt.Gzip = gzip;

        nbt.Read(steam2);

        steam2.Dispose();

        return nbt;
    }

    public static void Write(string file, NbtBase nbt)
    {
        using var steam = File.Create(file);
        DataOutputStream steam2;
        if (nbt.Gzip)
        {
            using var steam1 = new GZipStream(steam, CompressionMode.Compress);
            steam2 = new DataOutputStream(steam1);
        }
        else
        {
            steam2 = new DataOutputStream(steam);
        }

        steam2.Write((byte)nbt.NbtType);
        if (nbt.NbtType != NbtType.NbtEnd)
        {
            steam2.Write("");
            nbt.Write(steam2);
        }
        steam2.Dispose();
    }
}
