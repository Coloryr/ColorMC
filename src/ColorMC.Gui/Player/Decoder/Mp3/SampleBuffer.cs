using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ColorMC.Gui.Player.Decoder.Mp3;

public class SampleBuffer : Obuffer
{
    private readonly short[] buffer;
    private readonly int[] bufferp;
    private readonly int channels;

    /**
     * Constructor
     */
    public SampleBuffer(int number_of_channels)
    {
        buffer = new short[OBUFFERSIZE];
        bufferp = new int[MAXCHANNELS];
        channels = number_of_channels;

        for (int i = 0; i < number_of_channels; ++i)
            bufferp[i] = (short)i;

    }

    public short[] GetBuffer()
    {
        return this.buffer;
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
        buffer[bufferp[channel]] = value;
        bufferp[channel] += channels;
    }

    public new void AppendSamples(int channel, float[] f)
    {
        int pos = bufferp[channel];

        short s;
        float fs;
        for (int i = 0; i < 32;)
        {
            fs = f[i++];
            fs = (fs > 32767.0f ? 32767.0f
                    : (Math.Max(fs, -32767.0f)));

            s = (short)fs;
            buffer[pos] = s;
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
