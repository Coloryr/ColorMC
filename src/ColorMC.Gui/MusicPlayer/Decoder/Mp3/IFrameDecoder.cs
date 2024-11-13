namespace ColorMC.Gui.MusicPlayer.Decoder.Mp3;

public interface IFrameDecoder
{
    /// <summary>
    /// Decodes one frame of MPEG audio.
    /// </summary>
    void DecodeFrame();
}
