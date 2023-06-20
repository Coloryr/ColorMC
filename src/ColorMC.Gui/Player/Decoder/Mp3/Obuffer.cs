namespace ColorMC.Gui.Player.Decoder.Mp3;

public abstract class Obuffer
{
    public const int OBUFFERSIZE = 2 * 1152;    // max. 2 * 1152 samples per frame
    public const int MAXCHANNELS = 2;        // max. number of channels

    /**
     * Takes a 16 Bit PCM sample.
     */
    public abstract void Append(int channel, short value);

    /**
     * Accepts 32 new PCM samples.
     */
    public virtual void AppendSamples(int channel, float[] f)
    {
        short s;
        for (int i = 0; i < 32;)
        {
            s = Clip(f[i++]);
            Append(channel, s);
        }
    }

    /**
     * Clip Sample to 16 Bits
     */
    private short Clip(float sample)
    {
        return (short)((sample > 32767.0f) ? 32767 :
                ((sample < -32768.0f) ? -32768 :
                        (short)sample));
    }

    /**
     * Clears all data in the buffer (for seeking).
     */
    public abstract void ClearBuffer();

}