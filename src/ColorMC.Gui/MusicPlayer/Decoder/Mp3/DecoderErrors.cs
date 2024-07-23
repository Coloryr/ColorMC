namespace ColorMC.Gui.Player.Decoder.Mp3;

public static class DecoderErrors
{
    /**
     * Layer not supported by the decoder.
     */
    public const int UNSUPPORTED_LAYER = 0x200 + 1;
    /**
     * Illegal allocation in subband layer. Indicates a corrupt stream.
     */
    public const int ILLEGAL_SUBBAND_ALLOCATION = 0x200 + 2;
}
