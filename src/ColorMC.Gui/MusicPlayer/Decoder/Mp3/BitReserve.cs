namespace ColorMC.Gui.Player.Decoder.Mp3;

public sealed class BitReserve
{
    /// <summary>
    /// Size of the internal buffer to store the reserved bits.
    /// Must be a power of 2. And x8, as each bit is stored as a single entry.
    /// </summary>
    private const int BUFSIZE = 4096 * 8;

    /**
     * Mask that can be used to quickly implement the
     * modulus operation on BUFSIZE.
     */
    private const int BUFSIZE_MASK = BUFSIZE - 1;
    private readonly int[] _buf = new int[BUFSIZE];
    private int _offset, _bufByteIdx;

    public int Hsstell { get; private set; }

    /**
     * Read a number bits from the bit stream.
     *
     * @param N the number of
     */
    public int Hgetbits(int N)
    {
        Hsstell += N;

        int val = 0;

        int pos = _bufByteIdx;
        if (pos + N < BUFSIZE)
        {
            while (N-- > 0)
            {
                val <<= 1;
                val |= ((_buf[pos++] != 0) ? 1 : 0);
            }
        }
        else
        {
            while (N-- > 0)
            {
                val <<= 1;
                val |= ((_buf[pos] != 0) ? 1 : 0);
                pos = (pos + 1) & BUFSIZE_MASK;
            }
        }
        _bufByteIdx = pos;
        return val;
    }

    /**
     * Returns next bit from reserve.
     *
     * @returns 0 if next bit is reset, or 1 if next bit is set.
     */
    public int Hget1bit()
    {
        Hsstell++;
        int val = _buf[_bufByteIdx];
        _bufByteIdx = (_bufByteIdx + 1) & BUFSIZE_MASK;
        return val;
    }

    /**
     * Write 8 bits into the bit stream.
     */
    public void Hputbuf(int val)
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

        if (ofs == BUFSIZE)
            _offset = 0;
        else
            _offset = ofs;

    }

    /**
     * Rewind N bits in Stream.
     */
    public void RewindNbits(int N)
    {
        Hsstell -= N;
        _bufByteIdx -= N;
        if (_bufByteIdx < 0)
            _bufByteIdx += BUFSIZE;
    }

    /**
     * Rewind N bytes in Stream.
     */
    public void RewindNbytes(int N)
    {
        int bits = (N << 3);
        Hsstell -= bits;
        _bufByteIdx -= bits;
        if (_bufByteIdx < 0)
            _bufByteIdx += BUFSIZE;
    }
}
