namespace ColorMC.Gui.Player.Decoder.Mp3;

public class DecoderErrors : JavaLayerErrors
{

    /**
     * Layer not supported by the decoder.
     */
    public static int UNSUPPORTED_LAYER
    {
        get
        {
            return 0x200 + 1;
        }
    }

    /**
     * Illegal allocation in subband layer. Indicates a corrupt stream.
     */
    public static int ILLEGAL_SUBBAND_ALLOCATION
    {
        get
        {
            return 0x200 + 2;
        }
    }
}
