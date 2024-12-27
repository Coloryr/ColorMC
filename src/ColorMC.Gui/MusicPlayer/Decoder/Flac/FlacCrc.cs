using System;

namespace ColorMC.Gui.MusicPlayer.Decoder.Flac;

public partial class FlacStream
{
    public const int DecodingTabLen = 13;
    public const int DecodingTabMask = (1 << DecodingTabLen) - 1;
    public const int DecodingChunk = 4;

    private static readonly byte[,] s_decodingTab = new byte[31, 1 << DecodingTabLen];
    private static readonly int[,] s_decodingValueTab = new int[31, 1 << DecodingTabLen];

    private static readonly byte[] s_crc8Table = new byte[256];
    private static readonly char[] s_crc16Table = new char[256];

    private int _crc8;
    private int _crc16;
    private int _crcStartIndex;

    private static void DecodingTabInit()
    {
        for (int param = 0; param < s_decodingTab.GetLength(0); param++)
        {
            for (int i = 0; ; i++)
            {
                int numBits = (i >>> param) + 1 + param;
                if (numBits > DecodingTabLen)
                {
                    break;
                }
                int bits = (1 << param) | (i & ((1 << param) - 1));
                int shift = DecodingTabLen - numBits;
                for (int j = 0; j < (1 << shift); j++)
                {
                    s_decodingTab[param, (bits << shift) | j] = (byte)numBits;
                    s_decodingValueTab[param, (bits << shift) | j] = (i >>> 1) ^ -(i & 1);
                }
            }
            if (s_decodingTab[param, 0] != 0)
            {
                throw new Exception();
            }
        }
    }

    private static void CrcInit()
    {
        for (int i = 0; i < s_crc8Table.Length; i++)
        {
            int temp8 = i;
            int temp16 = i << 8;
            for (int j = 0; j < 8; j++)
            {
                temp8 = (temp8 << 1) ^ ((temp8 >>> 7) * 0x107);
                temp16 = (temp16 << 1) ^ ((temp16 >>> 15) * 0x18005);
            }
            s_crc8Table[i] = (byte)temp8;
            s_crc16Table[i] = (char)temp16;
        }
    }

    public void ResetCrcs()
    {
        CheckByteAligned();
        _crcStartIndex = _byteBufferIndex - _bitBufferLen / 8;
        _crc8 = 0;
        _crc16 = 0;
    }

    private void CheckByteAligned()
    {
        if (_bitBufferLen % 8 != 0)
        {
            throw new Exception("Not at a byte boundary");
        }
    }

    private void UpdateCrcs(int less)
    {
        int end = _byteBufferIndex - less;
        for (int i = _crcStartIndex; i < end; i++)
        {
            int b = _byteBuffer[i] & 0xFF;
            _crc8 = s_crc8Table[_crc8 ^ b] & 0xFF;
            _crc16 = s_crc16Table[(_crc16 >>> 8) ^ b] ^ ((_crc16 & 0xFF) << 8);
            if ((_crc8 >>> 8) != 0 || (_crc16 >>> 16) != 0)
            {
                throw new Exception("Crc check error");
            }
        }
        _crcStartIndex = end;
    }

    public int GetCrc8()
    {
        CheckByteAligned();
        UpdateCrcs(_bitBufferLen / 8);
        if ((_crc8 >>> 8) != 0)
        {
            throw new Exception("Crc check error");
        }
        return _crc8;
    }

    public int GetCrc16()
    {
        CheckByteAligned();
        UpdateCrcs(_bitBufferLen / 8);
        if ((_crc16 >>> 16) != 0)
        {
            throw new Exception("Crc check error");
        }
        return _crc16;
    }

    public void Dispose()
    {
        _stream.Dispose();
    }
}
