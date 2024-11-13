namespace ColorMC.Gui.MusicPlayer.Decoder.Mp3;

public sealed class Crc16
{
    private ushort _crc;

    /**
     * Dummy Constructor
     */
    public Crc16()
    {
        _crc = 0xFFFF;
    }

    /**
     * Feed a bitstring to the crc calculation (0 < length <= 32).
     */
    public void AddBits(int bitstring, int length)
    {
        int bitmask = 1 << length - 1;
        do
            if ((_crc & 0x8000) == 0 ^ (bitstring & bitmask) == 0)
            {
                _crc <<= 1;
                ushort polynomial = 0x8005;
                _crc ^= polynomial;
            }
            else
                _crc <<= 1;
        while ((bitmask >>>= 1) != 0);
    }

    /**
     * Return the calculated checksum.
     * Erase it for next calls to add_bits().
     */
    public ushort Checksum()
    {
        ushort sum = _crc;
        _crc = 0xFFFF;
        return sum;
    }
}
