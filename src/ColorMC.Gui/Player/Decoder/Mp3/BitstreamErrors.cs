namespace ColorMC.Gui.Player.Decoder.Mp3;

public static class BitstreamErrors
{
    /**
     * An undeterminable error occurred.
     */
    public const int UNKNOWN_ERROR = 0x100;
    /**
     * A problem occurred reading from the stream.
     */
    public const int STREAM_ERROR = 0x100 + 2;
    /**
     * The end of the stream was reached.
     */
    public const int STREAM_EOF = 0x100 + 4;


    /**
     * Frame data are missing.
     */
    public const int INVALIDFRAME = 0x100 + 5;
}
