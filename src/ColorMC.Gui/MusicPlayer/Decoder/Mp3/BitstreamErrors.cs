namespace ColorMC.Gui.MusicPlayer.Decoder.Mp3;

public static class BitStreamErrors
{
    /// <summary>
    /// An undeterminable error occurred.
    /// </summary>
    public const int UNKNOWN_ERROR = 0x100;
    /// <summary>
    /// A problem occurred reading from the stream.
    /// </summary>
    public const int STREAM_ERROR = 0x100 + 2;
    /// <summary>
    /// The end of the stream was reached.
    /// </summary>
    public const int STREAM_EOF = 0x100 + 4;

    /// <summary>
    /// Frame data are missing.
    /// </summary>
    public const int INVALIDFRAME = 0x100 + 5;
}
