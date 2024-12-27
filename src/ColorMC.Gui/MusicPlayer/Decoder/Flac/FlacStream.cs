using System;
using System.IO;
using System.Text;

namespace ColorMC.Gui.MusicPlayer.Decoder.Flac;

public partial class FlacStream
{
    private Stream _stream;

    internal FlacStream(Stream stream)
    {
        _stream = stream;
        DecodingTabInit();
        CrcInit();
        Seek(0);
    }

    public bool CheckHead()
    {
        var temp = new byte[4];
        _stream.ReadExactly(temp);
        //fLaC
        if (temp[0] == 0x66 && temp[1] == 0x4C && temp[2] == 0x61 && temp[3] == 0x43)
        {
            return true;
        }

        return false;
    }

    public FlacInfoBlock? DecodeInfo()
    {
        var temp = new byte[4];
        _stream.ReadExactly(temp);
        bool last = (temp[0] & 0x80) == 1;
        var type = (BlockType)(temp[0] & 0x7F);
        int size = temp[1] << 16 | temp[2] << 8 | temp[3];
        switch (type)
        {
            case BlockType.STREAMINFO:
                return new StreamInfoBlock(this, last, type, size);
            case BlockType.SEEKTABLE:
                return new SeekTableBlock(this, last, type, size);
            case BlockType.VORBIS_COMMEN:
                return new VorbisCommenBlock(this, last, type, size);
            case BlockType.PICTURE:
                return new PictureBlock(this, last, type, size);
            case BlockType.PADDING:
                _stream.Seek(size, SeekOrigin.Current);
                break;
        }

        return null;
    }

    public int ReadInt16BE()
    {
        var temp = new byte[2];
        _stream.ReadExactly(temp);
        return ReadInt16BE(temp, 0);
    }

    public int ReadInt16LE()
    {
        var temp = new byte[2];
        _stream.ReadExactly(temp);
        return BitConverter.ToInt16(temp);
    }

    public int ReadInt32LE()
    {
        var temp = new byte[4];
        _stream.ReadExactly(temp);
        return BitConverter.ToInt32(temp);
    }

    public int ReadInt32BE()
    {
        var temp = new byte[4];
        _stream.ReadExactly(temp);
        return ReadInt32BE(temp, 0);
    }

    public string ReadStringLE()
    {
        var size1 = ReadInt32LE();
        var temp1 = new byte[size1];
        _stream.ReadExactly(temp1);
        return Encoding.UTF8.GetString(temp1);
    }

    public string ReadStringBE()
    {
        var size1 = ReadInt32BE();
        var temp1 = new byte[size1];
        _stream.ReadExactly(temp1);
        return Encoding.UTF8.GetString(temp1);
    }

    public long ReadInt64BE()
    {
        var temp = new byte[8];
        _stream.ReadExactly(temp);
        return ReadInt64BE(temp, 0);
    }

    public static int ReadInt16BE(byte[] data, int offset)
    {
        return (data[offset] << 8) | data[offset + 1];
    }

    public static int ReadInt24BE(byte[] data, int offset)
    {
        return (data[offset] << 16) | (data[offset + 1] << 8) | data[offset + 2];
    }

    public static int ReadInt32BE(byte[] data, int offset)
    {
        return data[offset] << 24
            | data[offset + 1] << 16
            | data[offset + 2] << 8
            | data[offset + 3];
    }

    public static long ReadInt36BE(byte[] data, int offset)
    {
        return ((long)(data[offset] & 0x0F) << 32)
            | ((long)data[offset + 1] << 24)
            | ((long)data[offset + 2] << 16)
            | ((long)data[offset + 3] << 8)
            | data[offset + 4];
    }

    public static long ReadInt64BE(byte[] data, int offset)
    {
        return ((long)data[offset] << 56)
            | ((long)data[offset + 1] << 48)
            | ((long)data[offset + 2] << 40)
            | ((long)data[offset + 3] << 32)
            | ((long)data[offset + 4] << 24)
            | ((long)data[offset + 5] << 16)
            | ((long)data[offset + 6] << 8)
            | data[offset + 7];
    }

    public void ReadExactly(byte[] data)
    {
        _stream.ReadExactly(data);
    }

    public int ReadByte()
    {
        var temp = new byte[1];
        _stream.ReadExactly(temp);

        return temp[0];
    }
}
