using System.Text;

namespace ColorMC.Core.Nbt;

/// <summary>
/// 数据输入流
/// </summary>
public class DataInputStream : IDisposable
{
    private readonly Stream s_baseStream;
    
    public DataInputStream(Stream stream)
    {
        s_baseStream = stream;
    }

    /// <summary>
    /// 读一定长度的字节
    /// </summary>
    /// <param name="bytes">要读的数据</param>
    public void Read(byte[] bytes)
    {
        s_baseStream.ReadExactly(bytes, 0, bytes.Length);
    }

    /// <summary>
    /// 读布尔值
    /// </summary>
    /// <returns>值</returns>
    public bool ReadBoolean()
    {
        var temp = ReadByte();
        return temp != 0;
    }

    /// <summary>
    /// 读Byte值
    /// </summary>
    /// <returns>值</returns>
    public byte ReadByte()
    {
        var temp = s_baseStream.ReadByte();
        if (temp < 0)
            throw new IOException();

        return (byte)temp;
    }

    /// <summary>
    /// 读Short值
    /// </summary>
    /// <returns>值</returns>
    public short ReadShort()
    {
        var temp = new byte[2];
        s_baseStream.ReadExactly(temp, 0, 2);
        Array.Reverse(temp);
        return BitConverter.ToInt16(temp, 0);
    }

    /// <summary>
    /// 读Int值
    /// </summary>
    /// <returns>值</returns>
    public int ReadInt()
    {
        var temp = new byte[4];
        s_baseStream.ReadExactly(temp, 0, 4);
        Array.Reverse(temp);
        return BitConverter.ToInt32(temp, 0);
    }

    /// <summary>
    /// 读Long值
    /// </summary>
    /// <returns>值</returns>
    public long ReadLong()
    {
        var temp = new byte[8];
        s_baseStream.ReadExactly(temp, 0, 8);
        Array.Reverse(temp);
        return BitConverter.ToInt64(temp, 0);
    }

    /// <summary>
    /// 读Float值
    /// </summary>
    /// <returns>值</returns>
    public float ReadFloat()
    {
        var temp = new byte[4];
        s_baseStream.ReadExactly(temp, 0, 4);
        Array.Reverse(temp);
        return BitConverter.ToSingle(temp, 0);
    }

    /// <summary>
    /// 读Double
    /// </summary>
    /// <returns>值</returns>
    public double ReadDouble()
    {
        var temp = new byte[8];
        s_baseStream.ReadExactly(temp, 0, 8);
        Array.Reverse(temp);
        return BitConverter.ToDouble(temp, 0);
    }

    /// <summary>
    /// 读字符串
    /// </summary>
    /// <returns>值</returns>
    public string ReadString()
    {
        var length = ReadShort();
        if (length == 0)
            return "";
        var temp = new byte[length];
        s_baseStream.ReadExactly(temp, 0, length);

        return Encoding.UTF8.GetString(temp);
    }

    /// <summary>
    /// 关闭
    /// </summary>
    public void Dispose()
    {
        s_baseStream.Dispose();
        GC.SuppressFinalize(this);
    }
}
