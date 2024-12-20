﻿using System;

namespace ColorMC.Gui.MusicPlayer.Decoder.Mp3;

public class BitstreamException(string msg, Exception? t) : Exception(msg, t)
{
    private readonly int _errorcode = BitstreamErrors.UNKNOWN_ERROR;

    public BitstreamException(int errorcode, Exception? t) : this(GetErrorString(errorcode), t)
    {
        _errorcode = errorcode;
    }

    static public string GetErrorString(int errorcode)
    {
        // REVIEW: use resource bundle to map error codes
        // to locale-sensitive strings.

        return $"Bitstream errorcode {errorcode:X}";
    }

    public int GetErrorCode()
    {
        return _errorcode;
    }
}
