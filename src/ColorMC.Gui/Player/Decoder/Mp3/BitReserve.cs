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
    private readonly int[] buf = new int[BUFSIZE];
    private int offset, totbit, bufByteIdx;

    /**
     * Return totbit Field.
     */
    public int Hsstell()
    {
        return totbit;
    }

    /**
     * Read a number bits from the bit stream.
     *
     * @param N the number of
     */
    public int Hgetbits(int N)
    {
        totbit += N;

        int val = 0;

        int pos = bufByteIdx;
        if (pos + N < BUFSIZE)
        {
            while (N-- > 0)
            {
                val <<= 1;
                val |= ((buf[pos++] != 0) ? 1 : 0);
            }
        }
        else
        {
            while (N-- > 0)
            {
                val <<= 1;
                val |= ((buf[pos] != 0) ? 1 : 0);
                pos = (pos + 1) & BUFSIZE_MASK;
            }
        }
        bufByteIdx = pos;
        return val;
    }

    /**
     * Returns next bit from reserve.
     *
     * @returns 0 if next bit is reset, or 1 if next bit is set.
     */
    public int Hget1bit()
    {
        totbit++;
        int val = buf[bufByteIdx];
        bufByteIdx = (bufByteIdx + 1) & BUFSIZE_MASK;
        return val;
    }

    /**
     * Write 8 bits into the bit stream.
     */
    public void Hputbuf(int val)
    {
        int ofs = offset;
        buf[ofs++] = val & 0x80;
        buf[ofs++] = val & 0x40;
        buf[ofs++] = val & 0x20;
        buf[ofs++] = val & 0x10;
        buf[ofs++] = val & 0x08;
        buf[ofs++] = val & 0x04;
        buf[ofs++] = val & 0x02;
        buf[ofs++] = val & 0x01;

        if (ofs == BUFSIZE)
            offset = 0;
        else
            offset = ofs;

    }

    /**
     * Rewind N bits in Stream.
     */
    public void RewindNbits(int N)
    {
        totbit -= N;
        bufByteIdx -= N;
        if (bufByteIdx < 0)
            bufByteIdx += BUFSIZE;
    }

    /**
     * Rewind N bytes in Stream.
     */
    public void RewindNbytes(int N)
    {
        int bits = (N << 3);
        totbit -= bits;
        bufByteIdx -= bits;
        if (bufByteIdx < 0)
            bufByteIdx += BUFSIZE;
    }
}
