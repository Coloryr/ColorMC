namespace ColorMC.Gui.Player.Decoder;

public interface IDecoder
{
    BuffPack decodeFrame();

    void close();

    bool set();

    int getOutputFrequency();

    int getOutputChannels();

    void set(int time);
}

