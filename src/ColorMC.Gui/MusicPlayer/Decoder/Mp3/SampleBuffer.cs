namespace ColorMC.Gui.MusicPlayer.Decoder.Mp3;

public class SampleBuffer
{
    public readonly float[] Buffer = new float[2 * 1152 * 2];

    private readonly int[] bufferp = new int[2];
    private readonly int channels;

    public SampleBuffer(int channel)
    {
        channels = channel;

        for (int i = 0; i < channel; ++i)
        {
            bufferp[i] = (short)i;
        }
    }

    public int GetBufferLength()
    {
        return bufferp[0];
    }

    /// <summary>
    /// Accepts 32 new PCM samples.
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="f"></param>
    public void AppendSamples(int channel, float[] f)
    {
        int pos = bufferp[channel];

        for (int i = 0; i < 32;)
        {
            Buffer[pos] = f[i++] / 32768f / 2;
            pos += channels;
        }

        bufferp[channel] = pos;
    }

    /// <summary>
    /// Clears all data in the buffer (for seeking).
    /// </summary>
    public void ClearBuffer()
    {
        for (int i = 0; i < channels; ++i)
        {
            bufferp[i] = i;
        }
    }
}