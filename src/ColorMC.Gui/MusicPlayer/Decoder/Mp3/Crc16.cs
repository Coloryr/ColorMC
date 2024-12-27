namespace ColorMC.Gui.MusicPlayer.Decoder.Mp3;

public class Crc16
{
    private ushort _crc = 0xFFFF;

    /// <summary>
    /// Feed a bitstring to the crc calculation.
    /// </summary>
    /// <param name="bitstring"></param>
    /// <param name="length">(0 < length <= 32)</param>
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

    /// <summary>
    /// Return the calculated checksum.
    /// Erase it for next calls to add_bits().
    /// </summary>
    /// <returns></returns>
    public ushort Checksum()
    {
        ushort sum = _crc;
        _crc = 0xFFFF;
        return sum;
    }
}
