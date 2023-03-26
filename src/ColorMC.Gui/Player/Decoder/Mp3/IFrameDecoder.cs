namespace ColorMC.Gui.Player.Decoder.Mp3;

public interface IFrameDecoder
{
    /**
     * Decodes one frame of MPEG audio.
     */
    void DecodeFrame();
}
