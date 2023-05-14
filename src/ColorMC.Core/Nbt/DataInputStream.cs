using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Tomlyn;

namespace ColorMC.Core.Nbt;

public class DataInputStream : IDisposable
{
    private readonly Stream BaseStream;
    public DataInputStream(Stream stream) 
    {
        BaseStream = stream;
    }

    public void Read(byte[] bytes)
    {
        BaseStream.ReadExactly(bytes, 0, bytes.Length);
    }

    public bool ReadBoolean()
    {
        var temp = ReadByte();
        return temp != 0;
    }

    public byte ReadByte()
    {
        var temp = BaseStream.ReadByte();
        if (temp < 0)
            throw new IOException();

        return (byte)temp;
    }
    public short ReadShort()
    {
        var temp = new byte[2];
        BaseStream.ReadExactly(temp, 0, 2);
        Array.Reverse(temp);
        return BitConverter.ToInt16(temp, 0);
    }

    public char ReadChar()
    {
        var temp = new byte[2];
        BaseStream.ReadExactly(temp, 0, 2);
        Array.Reverse(temp);
        return BitConverter.ToChar(temp, 0);
    }

    public int ReadInt()
    {
        var temp = new byte[4];
        BaseStream.ReadExactly(temp, 0, 4);
        Array.Reverse(temp);
        return BitConverter.ToInt32(temp, 0);
    }

    public long ReadLong()
    {
        var temp = new byte[8];
        BaseStream.ReadExactly(temp, 0, 8);
        Array.Reverse(temp);
        return BitConverter.ToInt64(temp, 0);
    }

    public float ReadFloat()
    {
        var temp = new byte[4];
        BaseStream.ReadExactly(temp, 0, 4);
        Array.Reverse(temp);
        return BitConverter.ToSingle(temp, 0);
    }

    public double ReadDouble()
    {
        var temp = new byte[8];
        BaseStream.ReadExactly(temp, 0, 8);
        Array.Reverse(temp);
        return BitConverter.ToDouble(temp, 0);
    }

    public string ReadString()
    {
        var length = ReadShort();
        if (length == 0)
            return "";
        var temp = new byte[length];
        BaseStream.ReadExactly(temp, 0, length);

        return Encoding.UTF8.GetString(temp);
    }

    public void Dispose()
    {
        BaseStream.Dispose();
        GC.SuppressFinalize(this);
    }
}
