using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Player.Decoder.Mp3;

public class Mp3Decoder : DecoderErrors, IDisposable
{
    static private readonly Params DEFAULT_PARAMS = new();

    /**
     * The Obuffer instance that will receive the decoded
     * PCM samples.
     */
    private SampleBuffer output;
    /**
     * Synthesis filter for the left channel.
     */
    private SynthesisFilter filter1;
    /**
     * Sythesis filter for the right channel.
     */
    private SynthesisFilter filter2;
    /**
     * The decoder used to decode layer III frames.
     */
    private LayerIIIDecoder l3decoder;
    private LayerIIDecoder l2decoder;
    private LayerIDecoder l1decoder;
    public int outputFrequency;
    public int outputChannels;
    public int bitrate;
    private bool initialized;
    private Bitstream bitstream;
    private Stream stream;

    /**
     * Creates a new <code>Decoder</code> instance with default
     * parameters.
     */

    public Mp3Decoder(Stream stream)
    {
        this.stream = stream;
        Equalizer eq = DEFAULT_PARAMS.GetInitialEqualizerSettings();
        if (eq != null)
        {
            /*
      The Bistream from which the MPEG audio frames are read.
     */
            //private Bitstream				stream;
            Equalizer equalizer = new Equalizer();
            equalizer.SetFrom(eq);
        }
    }

    private byte[] byteBuf = new byte[4096];

    protected byte[] GetByteArray(int length)
    {
        if (byteBuf.Length < length)
        {
            byteBuf = new byte[length + 1024];
        }
        return byteBuf;
    }

    protected byte[] ToByteArray(short[] samples, int offs, int len)
    {
        byte[] b = GetByteArray(len * 2);
        int idx = 0;
        short s;
        while (len-- > 0)
        {
            s = samples[offs++];
            b[idx++] = (byte)s;
            b[idx++] = (byte)(s >>> 8);
        }
        return b;
    }

    private readonly BuffPack pack = new BuffPack();

    /**
     * Decodes one frame from an MPEG audio bitstream.
     *
     * @return A SampleBuffer containing the decoded samples.
     */
    public BuffPack? DecodeFrame()
    {
        var header = bitstream.ReadFrame();
        if (header == null)
            return null;
        int layer = header.Layer();
        output.ClearBuffer();
        IFrameDecoder decoder = RetrieveDecoder(header, bitstream, layer);
        decoder.DecodeFrame();
        bitstream.CloseFrame();
        pack.buff = ToByteArray(output.GetBuffer(), 0, output.GetBufferLength());
        pack.len = output.GetBufferLength() * 2;
        return pack;
    }

    public void Dispose()
    {
        bitstream.Dispose();
    }

    public bool Load()
    {
        bitstream = new Bitstream(stream);
        var header = bitstream.ReadFrame();
        if (header == null)
        {
            return false;
        }
        if (!initialized)
        {
            Initialize(header);
        }

        return true;
    }

    protected IFrameDecoder RetrieveDecoder(Header header, Bitstream stream, int layer)
    {
        IFrameDecoder decoder = null;

        // REVIEW: allow channel output selection type
        // (LEFT, RIGHT, BOTH, DOWNMIX)
        switch (layer)
        {
            case 3:
                if (l3decoder == null)
                {
                    l3decoder = new LayerIIIDecoder(stream,
                            header, filter1, filter2,
                            output, OutputChannels.BOTH_CHANNELS);
                }

                decoder = l3decoder;
                break;
            case 2:
                if (l2decoder == null)
                {
                    l2decoder = new LayerIIDecoder();
                    l2decoder.Create(stream,
                            header, filter1, filter2,
                            output, OutputChannels.BOTH_CHANNELS);
                }
                decoder = l2decoder;
                break;
            case 1:
                if (l1decoder == null)
                {
                    l1decoder = new LayerIDecoder();
                    l1decoder.Create(stream,
                            header, filter1, filter2,
                            output, OutputChannels.BOTH_CHANNELS);
                }
                decoder = l1decoder;
                break;
        }

        if (decoder == null)
        {
            throw new DecoderException(UNSUPPORTED_LAYER, null );
        }

        return decoder;
    }

    private void Initialize(Header header)
    {

        // REVIEW: allow customizable scale factor
        float scalefactor = 32700.0f;

        int mode = header.Mode();
        int channels = mode == Header.SINGLE_CHANNEL ? 1 : 2;

        // set up output buffer if not set up by client.
        if (output == null)
            output = new SampleBuffer(channels);

        filter1 = new SynthesisFilter(0, scalefactor);

        // REVIEW: allow mono output for stereo
        if (channels == 2)
            filter2 = new SynthesisFilter(1, scalefactor);

        outputChannels = channels;
        outputFrequency = header.Frequency();
        bitrate = header.Bitrate();

        initialized = true;
    }

    /**
     * The <code>Params</code> class presents the customizable
     * aspects of the decoder.
     * <p>
     * Instances of this class are not thread safe.
     */
    public class Params : ICloneable
    {

        private readonly Equalizer equalizer = new Equalizer();

        public Params()
        {
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        /**
         * Retrieves the equalizer settings that the decoder's equalizer
         * will be initialized from.
         * <p>
         * The <code>Equalizer</code> instance returned
         * cannot be changed in real time to affect the
         * decoder output as it is used only to initialize the decoders
         * EQ settings. To affect the decoder's output in realtime,
         * use the Equalizer returned from the getEqualizer() method on
         * the decoder.
         *
         * @return The <code>Equalizer</code> used to initialize the
         * EQ settings of the decoder.
         */
        public Equalizer GetInitialEqualizerSettings()
        {
            return equalizer;
        }

    }
}

