using System.Text;

namespace ColorMC.Core.Nbt;

/// <summary>
/// 输出流
/// </summary>
public class DataOutputStream : IDisposable
{
    private readonly Stream _baseStream;

    public DataOutputStream(Stream stream)
    {
        _baseStream = stream;
    }

    /// <summary>
    /// 写Byte
    /// </summary>
    /// <param name="value">值</param>
    public void Write(byte value)
    {
        _baseStream.WriteByte(value);
    }

    /// <summary>
    /// 写Byte数组
    /// </summary>
    /// <param name="value">值</param>
    public void Write(byte[] value)
    {
        _baseStream.Write(value);
    }

    /// <summary>
    /// 写Bool
    /// </summary>
    /// <param name="value">值</param>
    public void Write(bool value)
    {
        _baseStream.WriteByte(value ? (byte)1 : (byte)0);
    }

    /// <summary>
    /// 写Short
    /// </summary>
    /// <param name="value">值</param>
    public void Write(short value)
    {
        var temp = BitConverter.GetBytes(value);
        Array.Reverse(temp);
        _baseStream.Write(temp);
    }

    /// <summary>
    /// 写Int
    /// </summary>
    /// <param name="value">值</param>
    public void Write(int value)
    {
        var temp = BitConverter.GetBytes(value);
        Array.Reverse(temp);
        _baseStream.Write(temp);
    }

    /// <summary>
    /// 写Long
    /// </summary>
    /// <param name="value">值</param>
    public void Write(long value)
    {
        var temp = BitConverter.GetBytes(value);
        Array.Reverse(temp);
        _baseStream.Write(temp);
    }

    /// <summary>
    /// 写Float
    /// </summary>
    /// <param name="value">值</param>
    public void Write(float value)
    {
        var temp = BitConverter.GetBytes(value);
        Array.Reverse(temp);
        _baseStream.Write(temp);
    }

    /// <summary>
    /// 写Double
    /// </summary>
    /// <param name="value">值</param>
    public void Write(double value)
    {
        var temp = BitConverter.GetBytes(value);
        Array.Reverse(temp);
        _baseStream.Write(temp);
    }

    /// <summary>
    /// 写字符串
    /// </summary>
    /// <param name="value">值</param>
    public void Write(string value)
    {
        var data = Encoding.UTF8.GetBytes(value);
        if (data.Length > 65535)
        {
            throw new Exception("To long string");
        }
        Write((short)data.Length);
        if (data.Length > 0)
        {
            _baseStream.Write(data);
        }
    }

    /// <summary>
    /// 关闭
    /// </summary>
    public void Dispose()
    {
        _baseStream.Dispose();
        GC.SuppressFinalize(this);
    }
}
