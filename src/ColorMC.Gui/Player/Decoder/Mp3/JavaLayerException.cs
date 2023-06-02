using ColorMC.Core.Utils;
using System;

namespace ColorMC.Gui.Player.Decoder.Mp3;

public class JavaLayerException : Exception
{
    private readonly Exception? exception;

    public JavaLayerException(string msg, Exception? t) : base(msg)
    {
        exception = t;
    }
}
