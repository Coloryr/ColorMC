using ColorMC.Core;
using Esprima.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Player.Decoder.Mp3;

public class JavaLayerException : Exception
{
    private readonly Exception exception;

    public JavaLayerException(string msg, Exception t) : base(msg)
    {
        exception = t;
    }

    public void printStackTrace()
    {
        Logs.Error(Message, exception);
    }
}
