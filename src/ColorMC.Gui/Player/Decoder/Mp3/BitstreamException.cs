using System;

namespace ColorMC.Gui.Player.Decoder.Mp3;

public class BitstreamException : JavaLayerException
{
    private int errorcode = BitstreamErrors.UNKNOWN_ERROR;

    public BitstreamException(String msg, Exception t) : base(msg, t)
    {

    }

    public BitstreamException(int errorcode, Exception t) : this(getErrorString(errorcode), t)
    {
        this.errorcode = errorcode;
    }

    static public string getErrorString(int errorcode)
    {
        // REVIEW: use resource bundle to map error codes
        // to locale-sensitive strings.

        return $"Bitstream errorcode {errorcode:X}";
    }

    public int getErrorCode()
    {
        return errorcode;
    }
}
