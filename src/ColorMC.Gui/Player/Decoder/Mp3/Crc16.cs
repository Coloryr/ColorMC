using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Player.Decoder.Mp3;

public sealed class Crc16
{
    private ushort crc;

    /**
     * Dummy Constructor
     */
    public Crc16()
    {
        crc = 0xFFFF;
    }

    /**
     * Feed a bitstring to the crc calculation (0 < length <= 32).
     */
    public void AddBits(int bitstring, int length)
    {
        int bitmask = 1 << (length - 1);
        do
            if (((crc & 0x8000) == 0) ^ ((bitstring & bitmask) == 0))
            {
                crc <<= 1;
                ushort polynomial = 0x8005;
                crc ^= polynomial;
            }
            else
                crc <<= 1;
        while ((bitmask >>>= 1) != 0);
    }

    /**
     * Return the calculated checksum.
     * Erase it for next calls to add_bits().
     */
    public ushort Checksum()
    {
        ushort sum = crc;
        crc = 0xFFFF;
        return sum;
    }
}
