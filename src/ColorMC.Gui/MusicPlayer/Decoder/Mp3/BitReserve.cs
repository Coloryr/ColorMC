namespace ColorMC.Gui.MusicPlayer.Decoder.Mp3;

public class BitReserve
{
    /// <summary>
    /// Size of the internal buffer to store the reserved bits.
    /// Must be a power of 2. And x8, as each bit is stored as a single entry.
    /// </summary>
    public const int BufSize = 4096 * 8;

    /// <summary>
    /// Mask that can be used to quickly implement the
    /// modulus operation on BUFSIZE.
    /// </summary>
    private const int _bufSizeMask = BufSize - 1;

    private readonly int[] _buf = new int[BufSize];
    private int _offset, _bufByteIdx;

    public int Hsstell { get; private set; }

    /// <summary>
    /// Read a number bits from the bit stream.
    /// </summary>
    /// <param name="n">the number of</param>
    /// <returns></returns>
    public int Getbits(int n)
    {
        Hsstell += n;

        int val = 0;

        int pos = _bufByteIdx;
        if (pos + n < BufSize)
        {
            while (n-- > 0)
            {
                val <<= 1;
                val |= _buf[pos++] != 0 ? 1 : 0;
            }
        }
        else
        {
            while (n-- > 0)
            {
                val <<= 1;
                val |= _buf[pos] != 0 ? 1 : 0;
                pos = pos + 1 & _bufSizeMask;
            }
        }
        _bufByteIdx = pos;
        return val;
    }

    /// <summary>
    /// Returns next bit from reserve.
    /// </summary>
    /// <returns>0 if next bit is reset, or 1 if next bit is set.</returns>
    public int Get1bit()
    {
        Hsstell++;
        int val = _buf[_bufByteIdx];
        _bufByteIdx = _bufByteIdx + 1 & _bufSizeMask;
        return val;
    }

    /// <summary>
    /// Write 8 bits into the bit stream.
    /// </summary>
    /// <param name="val"></param>
    public void Putbuf(int val)
    {
        int ofs = _offset;
        _buf[ofs++] = val & 0x80;
        _buf[ofs++] = val & 0x40;
        _buf[ofs++] = val & 0x20;
        _buf[ofs++] = val & 0x10;
        _buf[ofs++] = val & 0x08;
        _buf[ofs++] = val & 0x04;
        _buf[ofs++] = val & 0x02;
        _buf[ofs++] = val & 0x01;

        if (ofs == BufSize)
            _offset = 0;
        else
            _offset = ofs;
    }

    /// <summary>
    /// Rewind N bits in Stream.
    /// </summary>
    /// <param name="n"></param>
    public void RewindNbits(int n)
    {
        Hsstell -= n;
        _bufByteIdx -= n;
        if (_bufByteIdx < 0)
        {
            _bufByteIdx += BufSize;
        }
    }

    /// <summary>
    /// Rewind N bytes in Stream.
    /// </summary>
    /// <param name="n"></param>
    public void RewindNbytes(int n)
    {
        int bits = n << 3;
        Hsstell -= bits;
        _bufByteIdx -= bits;
        if (_bufByteIdx < 0)
        {
            _bufByteIdx += BufSize;
        }
    }
}
