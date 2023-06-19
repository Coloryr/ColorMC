using ColorMC.Core.Helpers;
using System.IO.Compression;

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
    /// 是否为Gzip
    /// </summary>
    public bool Gzip { get; set; }
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
                    (this as NbtString)!.Value = value!;
                    break;
            };
        }
    }

    /// <summary>
    /// 读标签
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public abstract NbtBase Read(DataInputStream stream);

    /// <summary>
    /// 写标签
    /// </summary>
    /// <param name="stream"></param>
    public abstract void Write(DataOutputStream stream);

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

    public void Save(string file)
    {
        Save(this, file);
    }

    /// <summary>
    /// 取NBT
    /// </summary>
    /// <param name="id">Id</param>
    /// <returns></returns>
    public static NbtBase ById(byte id)
    {
        var type = NbtTypes.VALUES[id];
        return (Activator.CreateInstance(type) as NbtBase)!;
    }

    /// <summary>
    /// 取NBT
    /// </summary>
    /// <param name="id">Id</param>
    /// <returns></returns>
    public static NbtBase ById(NbtType id)
    {
        var type = NbtTypes.VALUES1[id];
        return (Activator.CreateInstance(type) as NbtBase)!;
    }

    /// <summary>
    /// 读NBT
    /// </summary>
    /// <param name="file">文件名</param>
    /// <returns></returns>
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

    /// <summary>
    /// 写NBT
    /// </summary>
    /// <param name="file">文件名</param>
    /// <param name="nbt">标签</param>
    public static void Save(NbtBase nbt, string file)
    {
        using var steam = File.Create(file);
        DataOutputStream steam2;
        if (nbt.Gzip)
        {
            var steam1 = new GZipStream(steam, CompressionMode.Compress);
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
