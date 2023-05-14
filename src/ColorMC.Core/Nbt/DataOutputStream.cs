using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Nbt;

public class DataOutputStream : IDisposable
{
    private readonly Stream baseStream;
    public DataOutputStream(Stream stream)
    {
        baseStream = stream;
    }

    public void Dispose()
    {
        baseStream.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Write(byte value)
    {
        baseStream.WriteByte(value);
    }

    public void Write(byte[] value)
    {
        baseStream.Write(value);
    }

    public void Write(bool value)
    {
        baseStream.WriteByte(value ? (byte)1 : (byte)0);
    }

    public void Write(short value)
    {
        var temp = BitConverter.GetBytes(value);
        Array.Reverse(temp);
        baseStream.Write(temp);
    }

    public void Write(char value)
    {
        var temp = BitConverter.GetBytes(value);
        Array.Reverse(temp);
        baseStream.Write(temp);
    }

    public void Write(int value)
    {
        var temp = BitConverter.GetBytes(value);
        Array.Reverse(temp);
        baseStream.Write(temp);
    }

    public void Write(long value)
    {
        var temp = BitConverter.GetBytes(value);
        Array.Reverse(temp);
        baseStream.Write(temp);
    }

    public void Write(float value)
    {
        var temp = BitConverter.GetBytes(value);
        Array.Reverse(temp);
        baseStream.Write(temp);
    }

    public void Write(double value)
    {
        var temp = BitConverter.GetBytes(value);
        Array.Reverse(temp);
        baseStream.Write(temp);
    }

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
            baseStream.Write(data);
        }
    }
}
