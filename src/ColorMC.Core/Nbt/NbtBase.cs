using System.IO.Compression;
using ColorMC.Core.Chunk;
using ColorMC.Core.Helpers;
using K4os.Compression.LZ4.Streams;

namespace ColorMC.Core.Nbt;

/// <summary>
/// NBT标签
/// </summary>
public abstract class NbtBase
{
    /// <summary>
    /// 类型
    /// </summary>
    public NbtType NbtType { get; protected init; }
    /// <summary>
    /// 压缩类型
    /// </summary>
    public ZipType ZipType { get; set; }
    /// <summary>
    /// 取值
    /// </summary>
    public string Value
    {
        get
        {
            return NbtType switch
            {
                NbtType.NbtByte => (this as NbtByte)!.Value.ToString(),
                NbtType.NbtShort => (this as NbtShort)!.Value.ToString(),
                NbtType.NbtInt => (this as NbtInt)!.Value.ToString(),
                NbtType.NbtLong => (this as NbtLong)!.Value.ToString(),
                NbtType.NbtFloat => (this as NbtFloat)!.Value.ToString(),
                NbtType.NbtDouble => (this as NbtDouble)!.Value.ToString(),
                NbtType.NbtString => (this as NbtString)!.Value.ToString(),
                _ => ""
            };
        }
        set
        {
            if (value == null)
                return;

            switch (NbtType)
            {
                case NbtType.NbtByte:
                    (this as NbtByte)!.Value = byte.Parse(value);
                    break;
                case NbtType.NbtShort:
                    (this as NbtShort)!.Value = short.Parse(value);
                    break;
                case NbtType.NbtInt:
                    (this as NbtInt)!.Value = int.Parse(value);
                    break;
                case NbtType.NbtLong:
                    (this as NbtLong)!.Value = long.Parse(value);
                    break;
                case NbtType.NbtFloat:
                    (this as NbtFloat)!.Value = float.Parse(value);
                    break;
                case NbtType.NbtDouble:
                    (this as NbtDouble)!.Value = double.Parse(value);
                    break;
                case NbtType.NbtString:
                    (this as NbtString)!.Value = value;
                    break;
            };
        }
    }

    /// <summary>
    /// 读标签
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    internal abstract NbtBase Read(DataInputStream stream);

    /// <summary>
    /// 写标签
    /// </summary>
    /// <param name="stream"></param>
    internal abstract void Write(DataOutputStream stream);

    /// <summary>
    /// 转字符串
    /// </summary>
    /// <returns></returns>
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
            NbtType.NbtByteArray => string.Format(LanguageHelper.Get("Core.Game.Info"),
             (this as NbtByteArray)?.Value.Count),
            NbtType.NbtList => string.Format(LanguageHelper.Get("Core.Game.Info"),
             (this as NbtList)?.Count),
            NbtType.NbtIntArray => string.Format(LanguageHelper.Get("Core.Game.Info"),
             (this as NbtIntArray)?.Value.Count),
            NbtType.NbtLongArray => string.Format(LanguageHelper.Get("Core.Game.Info"),
             (this as NbtLongArray)?.Value.Count),
            NbtType.NbtCompound => string.Format(LanguageHelper.Get("Core.Game.Info"),
             (this as NbtCompound)?.Count),
            _ => ""
        };
    }

    /// <summary>
    /// 是否有子NBT
    /// </summary>
    /// <returns></returns>
    public bool HaveItem()
    {
        return NbtType switch
        {
            NbtType.NbtList => this is NbtList { Count: > 0 },
            NbtType.NbtCompound => this is NbtCompound { Count: > 0 },
            _ => false
        };
    }

    /// <summary>
    /// 是否为列表
    /// </summary>
    /// <returns></returns>
    public bool IsGroup()
    {
        return NbtType switch
        {
            NbtType.NbtList => true,
            NbtType.NbtCompound => true,
            _ => false
        };
    }

    public async void Save(string file)
    {
        await Save(this, file);
    }

    public void Save(Stream stream)
    {
        Save(this, stream);
    }

    /// <summary>
    /// 取NBT
    /// </summary>
    /// <param name="id">类型</param>
    /// <returns>NBT标签</returns>
    public static NbtBase ById(NbtType id)
    {
        var type = NbtTypes.VALUES[id];
        return (Activator.CreateInstance(type) as NbtBase)!;
    }

    public static async Task<T?> ReadAsync<T>(Stream stream, bool chunk = false) where T : NbtBase
    {
        return await ReadAsync(stream, chunk) as T;
    }

    public static async Task<NbtBase?> ReadAsync(Stream stream, bool chunk = false)
    {
        if (stream.Length < 2)
        {
            return null;
        }
        DataInputStream stream2;
        var data = new byte[3];
        await stream.ReadExactlyAsync(data);
        stream.Seek(0, SeekOrigin.Begin);
        ZipType zip = ZipType.None;
        if (data[0] == 0x1F && data[1] == 0x8B)
        {
            var steam1 = new GZipStream(stream, CompressionMode.Decompress);
            stream2 = new DataInputStream(steam1);
            zip = ZipType.GZip;
        }
        else if (data[0] == 0x78 && (data[1] is 0x01 or 0x9C or 0xDA))
        {
            var steam1 = new ZLibStream(stream, CompressionMode.Decompress);
            stream2 = new DataInputStream(steam1);
            zip = ZipType.Zlib;
        }
        else if (data[0] == 0x4C && data[1] == 0x5A && data[2] == 0x34)
        {
            var steam1 = LZ4Stream.Decode(stream);
            stream2 = new DataInputStream(steam1);
            zip = ZipType.LZ4;
        }
        else
        {
            stream2 = new DataInputStream(stream);
        }
        var type = stream2.ReadByte();
        if (type >= NbtTypes.VALUES.Count)
        {
            return null;
        }

        var type1 = (NbtType)type;

        if (type1 == NbtType.NbtEnd)
        {
            return new NbtEnd();
        }

        if (type1 == NbtType.NbtCompound)
        {
            var temp = stream2.ReadShort();
            if (temp > 0)
            {
                var temp1 = new byte[temp];
                stream2.Read(temp1);
            }
        }

        NbtBase nbt;

        if (chunk)
        {
            nbt = new ChunkNbt();
        }
        else
        {
            nbt = ById(type1);
        }
        nbt.ZipType = zip;
        await Task.Run(() =>
        {
            nbt.Read(stream2);
        });

        stream2.Dispose();

        return nbt;
    }

    public static async Task<T?> Read<T>(string file) where T : NbtBase
    {
        using var stream = PathHelper.OpenRead(file)!;
        return await ReadAsync(stream) as T;
    }

    /// <summary>
    /// 读NBT
    /// </summary>
    /// <param name="file">文件名</param>
    /// <returns></returns>
    public static async Task<NbtBase?> Read(string file)
    {
        using var stream = PathHelper.OpenRead(file)!;
        return await ReadAsync(stream);
    }

    public static void Save(NbtBase nbt, Stream stream)
    {
        var steam2 = new DataOutputStream(nbt.ZipType switch
        {
            ZipType.GZip => new GZipStream(stream, CompressionLevel.Optimal),
            ZipType.Zlib => new ZLibStream(stream, CompressionLevel.Optimal),
            ZipType.LZ4 => LZ4Stream.Encode(stream),
            _ => stream
        });

        steam2.Write((byte)nbt.NbtType);
        if (nbt.NbtType != NbtType.NbtEnd)
        {
            steam2.Write("");
            nbt.Write(steam2);
        }
        steam2.Dispose();
    }

    /// <summary>
    /// 写NBT
    /// </summary>
    /// <param name="file">文件名</param>
    /// <param name="nbt">标签</param>
    public static Task Save(NbtBase nbt, string file)
    {
        return Task.Run(() =>
        {
            using var stream = PathHelper.OpenWrite(file, true);
            Save(nbt, stream);
        });
    }
}
