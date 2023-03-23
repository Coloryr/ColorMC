using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Player.Decoder.Mp3;

public interface JavaLayerErrors
{
    /**
     * The first bitstream error code. See the {@link DecoderErrors DecoderErrors}
     * interface for other bitstream error codes.
     */
    static virtual int BITSTREAM_ERROR
    {
        get
        {
            return 0x100;
        }
    }

    /**
     * The first decoder error code. See the {@link DecoderErrors DecoderErrors}
     * interface for other decoder error codes.
     */
    static virtual int DECODER_ERROR 
    {
        get 
        {
            return 0x200;
        }
    }
}
