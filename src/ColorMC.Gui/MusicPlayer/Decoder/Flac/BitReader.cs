using System;

namespace ColorMC.Gui.MusicPlayer.Decoder.Flac;

public partial class FlacStream
{
    private long _byteBufferStartPos;
    private byte[] _byteBuffer = new byte[4096];
    private int _byteBufferLen;
    private int _byteBufferIndex;
    private long _bitBuffer;
    private int _bitBufferLen;

    private void FillBitBuffer()
    {
        int i = _byteBufferIndex;
        int n = Math.Min((64 - _bitBufferLen) >>> 3, _byteBufferLen - i);
        byte[] b = _byteBuffer;
        if (n > 0)
        {
            for (int j = 0; j < n; j++, i++)
            {
                _bitBuffer = (_bitBuffer << 8) | (long)(b[i] & 0xFF);
            }
            _bitBufferLen += n << 3;
        }
        else if (_bitBufferLen <= 56)
        {
            int temp = ReadBuffer();
            if (temp == -1)
            {
                throw new Exception();
            }
            _bitBuffer = (_bitBuffer << 8) | (long)temp;
            _bitBufferLen += 8;
        }
        if (8 > _bitBufferLen || _bitBufferLen > 64)
        {
            throw new Exception("bitBufferLen is out of range");
        }
        _byteBufferIndex += n;
    }

    public void ReadRiceSignedInts(int param, long[] result, int start, int end)
    {
        if (param < 0 || param > 31)
        {
            throw new ArgumentOutOfRangeException(nameof(param));
        }
        long unaryLimit = 1L << (53 - param);

        void DoChunk()
        {
            while (start <= end - DecodingChunk)
            {
                if (_bitBufferLen < DecodingChunk * DecodingTabLen)
                {
                    if (_byteBufferIndex <= _byteBufferLen - 8)
                    {
                        FillBitBuffer();
                    }
                    else
                    {
                        break;
                    }
                }
                for (int i = 0; i < DecodingChunk; i++, start++)
                {
                    int extractedBits = (int)(_bitBuffer >>> (_bitBufferLen - DecodingTabLen)) & DecodingTabMask;
                    int consumed = s_decodingTab[param, extractedBits];
                    if (consumed == 0)
                    {
                        return;
                    }
                    _bitBufferLen -= consumed;
                    result[start] = s_decodingValueTab[param, extractedBits];
                }
            }
        }

        while (true)
        {
            DoChunk();
            if (start >= end)
            {
                break;
            }
            long val = 0;
            while (ReadUintWithCrc(1) == 0)
            {
                if (val >= unaryLimit)
                {
                    throw new Exception("Residual value too large");
                }
                val++;
            }
            val = (val << param) | (long)ReadUintWithCrc(param);
            if ((val >>> 53) != 0)
            {
                throw new Exception("read data is to larger");
            }
            val = (val >>> 1) ^ -(val & 1);
            if ((val >> 52) != 0 && (val >> 52) != -1)
            {
                throw new Exception("read data is to larger");
            }
            result[start] = val;
            start++;
        }
    }

    public int ReadShiftInt(int n)
    {
        int shift = 32 - n;
        return (ReadUintWithCrc(n) << shift) >> shift;
    }

    public int GetBitPosition()
    {
        return (-_bitBufferLen) & 0x07;
    }

    private void Seek(long pos)
    {
        _byteBufferStartPos = pos;
        _byteBufferLen = 0;
        _byteBufferIndex = 0;
        _bitBuffer = 0;
        _bitBufferLen = 0;
        ResetCrcs();
    }

    public int ReadUintWithCrc(int bits)
    {
        if (bits < 0 || bits > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(bits));
        }
        while (_bitBufferLen < bits)
        {
            int b = ReadBuffer();
            if (b == -1)
            {
                return -1;
            }
            _bitBuffer = (_bitBuffer << 8) | (long)b;
            _bitBufferLen += 8;
            if (0 > _bitBufferLen || _bitBufferLen > 64)
            {
                throw new IndexOutOfRangeException();
            }
        }
        int result = (int)(_bitBuffer >>> (_bitBufferLen - bits));
        if (bits != 32)
        {
            result &= (1 << bits) - 1;
            if ((result >>> bits) != 0)
            {
                throw new Exception();
            }
        }
        _bitBufferLen -= bits;
        if (0 > _bitBufferLen || _bitBufferLen > 64)
        {
            throw new IndexOutOfRangeException();
        }
        return result;
    }

    private int ReadBuffer()
    {
        if (_byteBufferIndex >= _byteBufferLen)
        {
            if (_byteBufferLen == -1)
            {
                return -1;
            }
            _byteBufferStartPos += _byteBufferLen;
            UpdateCrcs(0);
            _byteBufferLen = _stream.Read(_byteBuffer, 0, _byteBuffer.Length);
            if (_byteBufferLen == 0)
            {
                _byteBufferLen = -1;
                return -1;
            }
            _crcStartIndex = 0;
            _byteBufferIndex = 0;
        }
        if (_byteBufferIndex >= _byteBufferLen)
        {
            throw new IndexOutOfRangeException();
        }
        int temp = _byteBuffer[_byteBufferIndex] & 0xFF;
        _byteBufferIndex++;
        return temp;
    }

    private static int NumberOfLeadingZeros(int i)
    {
        if (i == 0) return 32;
        int n = 1;
        if (i >>> 16 == 0) { n += 16; i <<= 16; }
        if (i >>> 24 == 0) { n += 8; i <<= 8; }
        if (i >>> 28 == 0) { n += 4; i <<= 4; }
        if (i >>> 30 == 0) { n += 2; i <<= 2; }
        n -= i >>> 31;
        return n;
    }

    public long ReadCodedNumber()
    {
        int size = ReadUintWithCrc(8);
        int n = NumberOfLeadingZeros(~(size << 24));
        if (0 > n || n > 8)
        {
            throw new IndexOutOfRangeException();
        }
        if (n == 0)
        {
            return size;
        }
        else if (n == 1 || n == 8)
        {
            throw new Exception("Invalid coded number");
        }
        else
        {
            long result = size & (0x7F >>> n);
            for (int i = 0; i < n - 1; i++)
            {
                int temp = ReadUintWithCrc(8);
                if ((temp & 0xC0) != 0x80)
                {
                    throw new Exception("Invalid coded number");
                }
                result = (result << 6) | (long)(temp & 0x3F);
            }
            if ((result >>> 36) != 0)
            {
                throw new Exception();
            }
            return result;
        }
    }
}
