using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Player.Decoder.Mp3;

public class DecoderException : JavaLayerException
{
    public DecoderException(string msg, Exception t) : base(msg, t)
    {

    }

    public DecoderException(int errorcode, Exception t) : this(getErrorString(errorcode), t)
    {
        
    }

    static public string getErrorString(int errorcode)
    {
        // REVIEW: use resource file to map error codes
        // to locale-sensitive strings.

        return $"Decoder errorcode {errorcode:X}";
    }
}
