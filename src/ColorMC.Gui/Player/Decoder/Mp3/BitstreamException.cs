using System;

namespace ColorMC.Gui.Player.Decoder.Mp3;

public class BitstreamException : JavaLayerException
{
    private int Errorcode = BitstreamErrors.UNKNOWN_ERROR;

    public BitstreamException(string msg, Exception? t) : base(msg, t)
    {

    }

    public BitstreamException(int errorcode, Exception? t) : this(GetErrorString(errorcode), t)
    {
        this.Errorcode = errorcode;
    }

    static public string GetErrorString(int errorcode)
    {
        // REVIEW: use resource bundle to map error codes
        // to locale-sensitive strings.

        return $"Bitstream errorcode {errorcode:X}";
    }

    public int GetErrorCode()
    {
        return Errorcode;
    }
}
