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
        var nbt = NbtTypes.ById(type);
        
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
