namespace ColorMC.Gui.Player.Decoder.Mp3;

public class BitstreamErrors : JavaLayerErrors
{
    /**
     * An undeterminable error occurred.
     */
    public static int UNKNOWN_ERROR
    {
        get
        {
            return 0x100;
        }
    }

    /**
     * A problem occurred reading from the stream.
     */
    public static int STREAM_ERROR
    {
        get
        {
            return 0x100 + 2;
        }
    }

    /**
     * The end of the stream was reached.
     */
    public static int STREAM_EOF
    {
        get
        {
            return 0x100 + 4;
        }
    }

    /**
     * Frame data are missing.
     */
    public static int INVALIDFRAME
    {
        get
        {
            return 0x100 + 5;
        }
    }
}
