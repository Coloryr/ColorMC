namespace ColorMC.Gui.Player.Decoder.Mp3;

public sealed class BitReserve
{
    /**
     * Size of the internal buffer to store the reserved bits.
     * Must be a power of 2. And x8, as each bit is stored as a single
     * entry.
     */
    private const int BUFSIZE = 4096 * 8;

    /**
     * Mask that can be used to quickly implement the
     * modulus operation on BUFSIZE.
     */
    private const int BUFSIZE_MASK = BUFSIZE - 1;
    private readonly int[] buf = new int[BUFSIZE];
    private int offset, totbit, buf_byte_idx;

    public BitReserve()
    {

        offset = 0;
        totbit = 0;
        buf_byte_idx = 0;
    }


    /**
     * Return totbit Field.
     */
    public int Hsstell()
    {
        return (totbit);
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

        int pos = buf_byte_idx;
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
        buf_byte_idx = pos;
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
        int val = buf[buf_byte_idx];
        buf_byte_idx = (buf_byte_idx + 1) & BUFSIZE_MASK;
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
        buf_byte_idx -= N;
        if (buf_byte_idx < 0)
            buf_byte_idx += BUFSIZE;
    }

    /**
     * Rewind N bytes in Stream.
     */
    public void RewindNbytes(int N)
    {
        int bits = (N << 3);
        totbit -= bits;
        buf_byte_idx -= bits;
        if (buf_byte_idx < 0)
            buf_byte_idx += BUFSIZE;
    }
}
