namespace ColorMC.Gui.Player.Decoder.Mp3;

public class LayerIDecoder : IFrameDecoder
{
    protected Bitstream stream;
    protected Header header;
    protected SynthesisFilter filter1, filter2;
    protected Obuffer buffer;
    protected int which_channels;
    protected int mode;

    protected int num_subbands;
    protected Subband[] subbands;
    protected Crc16 crc;    // new Crc16[1] to enable CRC checking.

    public LayerIDecoder()
    {
        crc = new Crc16();
    }

    public void Create(Bitstream stream0, Header header0,
                       SynthesisFilter filtera, SynthesisFilter filterb,
                       Obuffer buffer0, int which_ch0)
    {
        stream = stream0;
        header = header0;
        filter1 = filtera;
        filter2 = filterb;
        buffer = buffer0;
        which_channels = which_ch0;

    }

    public void DecodeFrame()
    {

        num_subbands = header.NumberOfSubbands;
        subbands = new Subband[32];
        mode = header.Mode;

        CreateSubbands();

        ReadAllocation();
        ReadScaleFactorSelection();

        if ((crc != null) || header.ChecksumOK())
        {
            ReadScaleFactors();

            ReadSampleData();
        }

    }

    protected void CreateSubbands()
    {
        int i;
        if (mode == Header.SINGLE_CHANNEL)
            for (i = 0; i < num_subbands; ++i)
                subbands[i] = new SubbandLayer1(i);
        else if (mode == Header.JOINT_STEREO)
        {
            for (i = 0; i < header.IntensityStereoBound; ++i)
                subbands[i] = new SubbandLayer1Stereo(i);
            for (; i < num_subbands; ++i)
                subbands[i] = new SubbandLayer1IntensityStereo(i);
        }
        else
        {
            for (i = 0; i < num_subbands; ++i)
                subbands[i] = new SubbandLayer1Stereo(i);
        }
    }

    protected void ReadAllocation()
    {
        // start to read audio data:
        for (int i = 0; i < num_subbands; ++i)
            subbands[i].ReadAllocation(stream, header, crc);

    }

    protected void ReadScaleFactorSelection()
    {
        // scale factor selection not present for layer I.
    }

    protected void ReadScaleFactors()
    {
        for (int i = 0; i < num_subbands; ++i)
            subbands[i].ReadScalefactor(stream, header);
    }

    protected void ReadSampleData()
    {
        bool read_ready = false;
        bool write_ready = false;
        int mode = header.Mode;
        int i;
        do
        {
            for (i = 0; i < num_subbands; ++i)
                read_ready = subbands[i].ReadSampledata(stream);
            do
            {
                for (i = 0; i < num_subbands; ++i)
                    write_ready = subbands[i].PutNextSample(which_channels, filter1, filter2);

                filter1.CalculatePcmSamples(buffer);
                if ((which_channels == OutputChannels.BOTH_CHANNELS) && (mode != Header.SINGLE_CHANNEL))
                    filter2.CalculatePcmSamples(buffer);
            } while (!write_ready);
        } while (!read_ready);

    }

    /**
     * Abstract base class for subband classes of layer I and II
     */
    public abstract class Subband
    {
        /*
         *  Changes from version 1.1 to 1.2:
         *    - array size increased by one, although a scalefactor with index 63
         *      is illegal (to prevent segmentation faults)
         */
        // Scalefactors for layer I and II, Annex 3-B.1 in ISO/IEC DIS 11172:
        public static readonly float[] scalefactors =
        {
            2.00000000000000f, 1.58740105196820f, 1.25992104989487f, 1.00000000000000f,
            0.79370052598410f, 0.62996052494744f, 0.50000000000000f, 0.39685026299205f,
            0.31498026247372f, 0.25000000000000f, 0.19842513149602f, 0.15749013123686f,
            0.12500000000000f, 0.09921256574801f, 0.07874506561843f, 0.06250000000000f,
            0.04960628287401f, 0.03937253280921f, 0.03125000000000f, 0.02480314143700f,
            0.01968626640461f, 0.01562500000000f, 0.01240157071850f, 0.00984313320230f,
            0.00781250000000f, 0.00620078535925f, 0.00492156660115f, 0.00390625000000f,
            0.00310039267963f, 0.00246078330058f, 0.00195312500000f, 0.00155019633981f,
            0.00123039165029f, 0.00097656250000f, 0.00077509816991f, 0.00061519582514f,
            0.00048828125000f, 0.00038754908495f, 0.00030759791257f, 0.00024414062500f,
            0.00019377454248f, 0.00015379895629f, 0.00012207031250f, 0.00009688727124f,
            0.00007689947814f, 0.00006103515625f, 0.00004844363562f, 0.00003844973907f,
            0.00003051757813f, 0.00002422181781f, 0.00001922486954f, 0.00001525878906f,
            0.00001211090890f, 0.00000961243477f, 0.00000762939453f, 0.00000605545445f,
            0.00000480621738f, 0.00000381469727f, 0.00000302772723f, 0.00000240310869f,
            0.00000190734863f, 0.00000151386361f, 0.00000120155435f, 0.00000000000000f /* illegal scalefactor */
        };

        public abstract void ReadAllocation(Bitstream stream, Header header, Crc16 crc);

        public abstract void ReadScalefactor(Bitstream stream, Header header);

        public abstract bool ReadSampledata(Bitstream stream);

        public abstract bool PutNextSample(int channels, SynthesisFilter filter1, SynthesisFilter filter2);
    }

    /**
     * Class for layer I subbands in single channel mode.
     * Used for single channel mode
     * and in derived class for intensity stereo mode
     */
    public class SubbandLayer1 : Subband
    {

        // Factors and offsets for sample requantization
        public static readonly float[] table_factor = {
            0.0f, 1.0f / 2.0f * (4.0f / 3.0f),
            1.0f / 4.0f * (8.0f / 7.0f),
            1.0f / 8.0f * (16.0f / 15.0f),
            1.0f / 16.0f * (32.0f / 31.0f),
            1.0f / 32.0f * (64.0f / 63.0f),
            1.0f / 64.0f * (128.0f / 127.0f),
            1.0f / 128.0f * (256.0f / 255.0f),
            1.0f / 256.0f * (512.0f / 511.0f),
            1.0f / 512.0f * (1024.0f / 1023.0f),
            1.0f / 1024.0f * (2048.0f / 2047.0f),
            1.0f / 2048.0f * (4096.0f / 4095.0f),
            1.0f / 4096.0f * (8192.0f / 8191.0f),
            1.0f / 8192.0f * (16384.0f / 16383.0f),
            1.0f / 16384.0f * (32768.0f / 32767.0f)
        };

        public static readonly float[] table_offset = {
            0.0f, ((1.0f / 2.0f) - 1.0f) * (4.0f / 3.0f),
            ((1.0f / 4.0f) - 1.0f) * (8.0f / 7.0f),
            ((1.0f / 8.0f) - 1.0f) * (16.0f / 15.0f),
            ((1.0f / 16.0f) - 1.0f) * (32.0f / 31.0f),
            ((1.0f / 32.0f) - 1.0f) * (64.0f / 63.0f),
            ((1.0f / 64.0f) - 1.0f) * (128.0f / 127.0f),
            ((1.0f / 128.0f) - 1.0f) * (256.0f / 255.0f),
            ((1.0f / 256.0f) - 1.0f) * (512.0f / 511.0f),
            ((1.0f / 512.0f) - 1.0f) * (1024.0f / 1023.0f),
            ((1.0f / 1024.0f) - 1.0f) * (2048.0f / 2047.0f),
            ((1.0f / 2048.0f) - 1.0f) * (4096.0f / 4095.0f),
            ((1.0f / 4096.0f) - 1.0f) * (8192.0f / 8191.0f),
            ((1.0f / 8192.0f) - 1.0f) * (16384.0f / 16383.0f),
            ((1.0f / 16384.0f) - 1.0f) * (32768.0f / 32767.0f)
        };

        protected int subbandnumber;
        protected int samplenumber;
        protected int allocation;
        protected float scalefactor;
        protected int samplelength;
        protected float sample;
        protected float factor, offset;

        /**
         * Construtor.
         */
        public SubbandLayer1(int subbandnumber)
        {
            this.subbandnumber = subbandnumber;
            samplenumber = 0;
        }

        /**
         *
         */
        public override void ReadAllocation(Bitstream stream, Header header, Crc16 crc)
        {
            if ((allocation = stream.GetBits(4)) == 15)
            {
                // CGJ: catch this condition and throw appropriate exception
                throw new DecoderException(DecoderErrors.ILLEGAL_SUBBAND_ALLOCATION, null);
                //	 cerr << "WARNING: stream contains an illegal allocation!\n";
                // MPEG-stream is corrupted!
            }

            if (crc != null) crc.AddBits(allocation, 4);
            if (allocation != 0)
            {
                samplelength = allocation + 1;
                factor = table_factor[allocation];
                offset = table_offset[allocation];
            }
        }

        /**
         *
         */
        public override void ReadScalefactor(Bitstream stream, Header header)
        {
            if (allocation != 0) scalefactor = scalefactors[stream.GetBits(6)];
        }

        /**
         *
         */
        public override bool ReadSampledata(Bitstream stream)
        {
            if (allocation != 0)
            {
                sample = stream.GetBits(samplelength);
            }
            if (++samplenumber == 12)
            {
                samplenumber = 0;
                return true;
            }
            return false;
        }

        /**
         *
         */
        public override bool PutNextSample(int channels, SynthesisFilter filter1, SynthesisFilter filter2)
        {
            if ((allocation != 0) && (channels != OutputChannels.RIGHT_CHANNEL))
            {
                float scaled_sample = (sample * factor + offset) * scalefactor;
                filter1.InputSample(scaled_sample, subbandnumber);
            }
            return true;
        }
    }

    /**
     * Class for layer I subbands in joint stereo mode.
     */
    public class SubbandLayer1IntensityStereo : SubbandLayer1
    {
        protected float channel2_scalefactor;

        /**
         * Constructor
         */
        public SubbandLayer1IntensityStereo(int subbandnumber) : base(subbandnumber)
        {

        }

        /**
         *
         */
        public override void ReadAllocation(Bitstream stream, Header header, Crc16 crc)
        {
            base.ReadAllocation(stream, header, crc);
        }

        /**
         *
         */
        public override void ReadScalefactor(Bitstream stream, Header header)
        {
            if (allocation != 0)
            {
                scalefactor = scalefactors[stream.GetBits(6)];
                channel2_scalefactor = scalefactors[stream.GetBits(6)];
            }
        }

        /**
         *
         */
        public override bool ReadSampledata(Bitstream stream)
        {
            return base.ReadSampledata(stream);
        }

        /**
         *
         */
        public override bool PutNextSample(int channels, SynthesisFilter filter1, SynthesisFilter filter2)
        {
            if (allocation != 0)
            {
                sample = sample * factor + offset;        // requantization
                if (channels == OutputChannels.BOTH_CHANNELS)
                {
                    float sample1 = sample * scalefactor,
                            sample2 = sample * channel2_scalefactor;
                    filter1.InputSample(sample1, subbandnumber);
                    filter2.InputSample(sample2, subbandnumber);
                }
                else if (channels == OutputChannels.LEFT_CHANNEL)
                {
                    float sample1 = sample * scalefactor;
                    filter1.InputSample(sample1, subbandnumber);
                }
                else
                {
                    float sample2 = sample * channel2_scalefactor;
                    filter1.InputSample(sample2, subbandnumber);
                }
            }
            return true;
        }
    }

    /**
     * Class for layer I subbands in stereo mode.
     */
    public class SubbandLayer1Stereo : SubbandLayer1
    {
        protected int channel2_allocation;
        protected float channel2_scalefactor;
        protected int channel2_samplelength;
        protected float channel2_sample;
        protected float channel2_factor, channel2_offset;


        /**
         * Constructor
         */
        public SubbandLayer1Stereo(int subbandnumber) : base(subbandnumber)
        {
        }

        /**
         *
         */
        public override void ReadAllocation(Bitstream stream, Header header, Crc16 crc)
        {
            allocation = stream.GetBits(4);
            channel2_allocation = stream.GetBits(4);
            if (crc != null)
            {
                crc.AddBits(allocation, 4);
                crc.AddBits(channel2_allocation, 4);
            }
            if (allocation != 0)
            {
                samplelength = allocation + 1;
                factor = table_factor[allocation];
                offset = table_offset[allocation];
            }
            if (channel2_allocation != 0)
            {
                channel2_samplelength = channel2_allocation + 1;
                channel2_factor = table_factor[channel2_allocation];
                channel2_offset = table_offset[channel2_allocation];
            }
        }

        /**
         *
         */
        public override void ReadScalefactor(Bitstream stream, Header header)
        {
            if (allocation != 0) scalefactor = scalefactors[stream.GetBits(6)];
            if (channel2_allocation != 0) channel2_scalefactor = scalefactors[stream.GetBits(6)];
        }

        /**
         *
         */
        public override bool ReadSampledata(Bitstream stream)
        {
            bool returnvalue = base.ReadSampledata(stream);
            if (channel2_allocation != 0)
            {
                channel2_sample = (float)(stream.GetBits(channel2_samplelength));
            }
            return (returnvalue);
        }

        /**
         *
         */
        public override bool PutNextSample(int channels, SynthesisFilter filter1, SynthesisFilter filter2)
        {
            base.PutNextSample(channels, filter1, filter2);
            if ((channel2_allocation != 0) && (channels != OutputChannels.LEFT_CHANNEL))
            {
                float sample2 = (channel2_sample * channel2_factor + channel2_offset) *
                        channel2_scalefactor;
                if (channels == OutputChannels.BOTH_CHANNELS)
                    filter2.InputSample(sample2, subbandnumber);
                else
                    filter1.InputSample(sample2, subbandnumber);
            }
            return true;
        }
    }
}
