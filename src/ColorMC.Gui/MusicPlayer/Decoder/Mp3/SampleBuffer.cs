using System;

namespace ColorMC.Gui.Player.Decoder.Mp3;

public class SampleBuffer : Obuffer
{
    public readonly short[] Buffer;
    private readonly int[] bufferp;
    private readonly int channels;

    /**
     * Constructor
     */
    public SampleBuffer(int number_of_channels)
    {
        Buffer = new short[OBUFFERSIZE];
        bufferp = new int[MAXCHANNELS];
        channels = number_of_channels;

        for (int i = 0; i < number_of_channels; ++i)
            bufferp[i] = (short)i;

    }

    public int GetBufferLength()
    {
        return bufferp[0];
    }

    /**
     * Takes a 16 Bit PCM sample.
     */
    public override void Append(int channel, short value)
    {
        Buffer[bufferp[channel]] = value;
        bufferp[channel] += channels;
    }

    public override void AppendSamples(int channel, float[] f)
    {
        int pos = bufferp[channel];

        short s;
        float fs;
        for (int i = 0; i < 32;)
        {
            fs = f[i++];
            fs = fs > 32767.0f ? 32767.0f
                    : (Math.Max(fs, -32767.0f));

            s = (short)fs;
            Buffer[pos] = s;
            pos += channels;
        }

        bufferp[channel] = pos;
    }

    /**
     *
     */
    public override void ClearBuffer()
    {
        for (int i = 0; i < channels; ++i)
            bufferp[i] = (short)i;
    }

}
