using System;

namespace ColorMC.Gui.MusicPlayer.Decoder.Mp3;

public class DecoderException(string msg, Exception? t) : Exception(msg, t)
{
    public DecoderException(int errorcode, Exception? t) : this(GetErrorString(errorcode), t)
    {

    }

    public static string GetErrorString(int errorcode)
    {
        // REVIEW: use resource file to map error codes
        // to locale-sensitive strings.

        return $"Decoder errorcode {errorcode:X}";
    }
}
