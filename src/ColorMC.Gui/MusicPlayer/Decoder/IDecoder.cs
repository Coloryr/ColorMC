using System;
using ColorMC.Gui.Objs;

namespace ColorMC.Gui.MusicPlayer.Decoder;

public interface IDecoder : IDisposable
{
    bool IsFile { get; }
    SoundPack? DecodeFrame();
    double GetTimeCount();
    MusicInfo? GetInfo();
    void Reset();
}
