namespace ColorMC.Gui.MusicPlayer.Decoder.Mp3;

public class LayerIDecoder : IFrameDecoder
{
    protected Bitstream Stream;
    protected Header Header;
    protected SynthesisFilter Filter1, Filter2;
    protected Obuffer Buffer;
    protected int WhichChannels;
    protected int Mode;

    protected int NumSubbands;
    protected Subband[] Subbands;
    protected Crc16 Crc;    // new Crc16[1] to enable CRC checking.

    public LayerIDecoder()
    {
        Crc = new Crc16();
    }

    public void Create(Bitstream stream0, Header header0,
                       SynthesisFilter filtera, SynthesisFilter filterb,
                       Obuffer buffer0, int which_ch0)
    {
        Stream = stream0;
        Header = header0;
        Filter1 = filtera;
        Filter2 = filterb;
        Buffer = buffer0;
        WhichChannels = which_ch0;
    }

    public void DecodeFrame()
    {
        NumSubbands = Header.NumberOfSubbands;
        Subbands = new Subband[32];
        Mode = Header.Mode;

        CreateSubbands();

        ReadAllocation();
        ReadScaleFactorSelection();

        if (Crc != null || Header.ChecksumOK())
        {
            ReadScaleFactors();

            ReadSampleData();
        }
    }

    protected virtual void CreateSubbands()
    {
        int i;
        if (Mode == Header.SINGLE_CHANNEL)
            for (i = 0; i < NumSubbands; ++i)
                Subbands[i] = new SubbandLayer1(i);
        else if (Mode == Header.JOINT_STEREO)
        {
            for (i = 0; i < Header.IntensityStereoBound; ++i)
                Subbands[i] = new SubbandLayer1Stereo(i);
            for (; i < NumSubbands; ++i)
                Subbands[i] = new SubbandLayer1IntensityStereo(i);
        }
        else
        {
            for (i = 0; i < NumSubbands; ++i)
                Subbands[i] = new SubbandLayer1Stereo(i);
        }
    }

    protected virtual void ReadAllocation()
    {
        // start to read audio data:
        for (int i = 0; i < NumSubbands; ++i)
            Subbands[i].ReadAllocation(Stream, Header, Crc);
    }

    protected virtual void ReadScaleFactorSelection()
    {
        // scale factor selection not present for layer I.
    }

    protected virtual void ReadScaleFactors()
    {
        for (int i = 0; i < NumSubbands; ++i)
            Subbands[i].ReadScalefactor(Stream, Header);
    }

    protected virtual void ReadSampleData()
    {
        bool read_ready = false;
        bool write_ready = false;
        int mode = Header.Mode;
        int i;
        do
        {
            for (i = 0; i < NumSubbands; ++i)
                read_ready = Subbands[i].ReadSampledata(Stream);
            do
            {
                for (i = 0; i < NumSubbands; ++i)
                    write_ready = Subbands[i].PutNextSample(WhichChannels, Filter1, Filter2);

                Filter1.CalculatePcmSamples(Buffer);
                if (WhichChannels == OutputChannels.BOTH_CHANNELS && mode != Header.SINGLE_CHANNEL)
                    Filter2.CalculatePcmSamples(Buffer);
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
        [
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
        ];

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
    public class SubbandLayer1(int subbandnumber) : Subband
    {
        // Factors and offsets for sample requantization
        public static readonly float[] table_factor =
        [
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
        ];

        public static readonly float[] table_offset =
        [
            0.0f, (1.0f / 2.0f - 1.0f) * (4.0f / 3.0f),
            (1.0f / 4.0f - 1.0f) * (8.0f / 7.0f),
            (1.0f / 8.0f - 1.0f) * (16.0f / 15.0f),
            (1.0f / 16.0f - 1.0f) * (32.0f / 31.0f),
            (1.0f / 32.0f - 1.0f) * (64.0f / 63.0f),
            (1.0f / 64.0f - 1.0f) * (128.0f / 127.0f),
            (1.0f / 128.0f - 1.0f) * (256.0f / 255.0f),
            (1.0f / 256.0f - 1.0f) * (512.0f / 511.0f),
            (1.0f / 512.0f - 1.0f) * (1024.0f / 1023.0f),
            (1.0f / 1024.0f - 1.0f) * (2048.0f / 2047.0f),
            (1.0f / 2048.0f - 1.0f) * (4096.0f / 4095.0f),
            (1.0f / 4096.0f - 1.0f) * (8192.0f / 8191.0f),
            (1.0f / 8192.0f - 1.0f) * (16384.0f / 16383.0f),
            (1.0f / 16384.0f - 1.0f) * (32768.0f / 32767.0f)
        ];

        protected int Subbandnumber = subbandnumber;
        protected int Samplenumber = 0;
        protected int Allocation;
        protected float Scalefactor;
        protected int Samplelength;
        protected float Sample;
        protected float Factor, Offset;

        public override void ReadAllocation(Bitstream stream, Header header, Crc16 crc)
        {
            if ((Allocation = stream.GetBits(4)) == 15)
            {
                // CGJ: catch this condition and throw appropriate exception
                throw new DecoderException(DecoderErrors.ILLEGAL_SUBBAND_ALLOCATION, null);
                //	 cerr << "WARNING: stream contains an illegal allocation!\n";
                // MPEG-stream is corrupted!
            }

            crc?.AddBits(Allocation, 4);
            if (Allocation != 0)
            {
                Samplelength = Allocation + 1;
                Factor = table_factor[Allocation];
                Offset = table_offset[Allocation];
            }
        }

        public override void ReadScalefactor(Bitstream stream, Header header)
        {
            if (Allocation != 0) Scalefactor = scalefactors[stream.GetBits(6)];
        }

        public override bool ReadSampledata(Bitstream stream)
        {
            if (Allocation != 0)
            {
                Sample = stream.GetBits(Samplelength);
            }
            if (++Samplenumber == 12)
            {
                Samplenumber = 0;
                return true;
            }
            return false;
        }

        public override bool PutNextSample(int channels, SynthesisFilter filter1, SynthesisFilter filter2)
        {
            if (Allocation != 0 && channels != OutputChannels.RIGHT_CHANNEL)
            {
                float scaled_sample = (Sample * Factor + Offset) * Scalefactor;
                filter1.InputSample(scaled_sample, Subbandnumber);
            }
            return true;
        }
    }

    /// <summary>
    /// Class for layer I subbands in joint stereo mode.
    /// </summary>
    /// <param name="subbandnumber"></param>
    public class SubbandLayer1IntensityStereo(int subbandnumber) : SubbandLayer1(subbandnumber)
    {
        protected float Channel2Scalefactor;

        public override void ReadAllocation(Bitstream stream, Header header, Crc16 crc)
        {
            base.ReadAllocation(stream, header, crc);
        }

        public override void ReadScalefactor(Bitstream stream, Header header)
        {
            if (Allocation != 0)
            {
                Scalefactor = scalefactors[stream.GetBits(6)];
                Channel2Scalefactor = scalefactors[stream.GetBits(6)];
            }
        }

        public override bool ReadSampledata(Bitstream stream)
        {
            return base.ReadSampledata(stream);
        }

        public override bool PutNextSample(int channels, SynthesisFilter filter1, SynthesisFilter filter2)
        {
            if (Allocation != 0)
            {
                Sample = Sample * Factor + Offset;        // requantization
                if (channels == OutputChannels.BOTH_CHANNELS)
                {
                    float sample1 = Sample * Scalefactor,
                            sample2 = Sample * Channel2Scalefactor;
                    filter1.InputSample(sample1, Subbandnumber);
                    filter2.InputSample(sample2, Subbandnumber);
                }
                else if (channels == OutputChannels.LEFT_CHANNEL)
                {
                    float sample1 = Sample * Scalefactor;
                    filter1.InputSample(sample1, Subbandnumber);
                }
                else
                {
                    float sample2 = Sample * Channel2Scalefactor;
                    filter1.InputSample(sample2, Subbandnumber);
                }
            }
            return true;
        }
    }

    /// <summary>
    /// Class for layer I subbands in stereo mode.
    /// </summary>
    public class SubbandLayer1Stereo(int subbandnumber) : SubbandLayer1(subbandnumber)
    {
        protected int Channel2Allocation;
        protected float Channel2Scalefactor;
        protected int Channel2Samplelength;
        protected float Channel2Sample;
        protected float Channel2Factor, Channel2Offset;

        public override void ReadAllocation(Bitstream stream, Header header, Crc16 crc)
        {
            Allocation = stream.GetBits(4);
            Channel2Allocation = stream.GetBits(4);
            if (crc != null)
            {
                crc.AddBits(Allocation, 4);
                crc.AddBits(Channel2Allocation, 4);
            }
            if (Allocation != 0)
            {
                Samplelength = Allocation + 1;
                Factor = table_factor[Allocation];
                Offset = table_offset[Allocation];
            }
            if (Channel2Allocation != 0)
            {
                Channel2Samplelength = Channel2Allocation + 1;
                Channel2Factor = table_factor[Channel2Allocation];
                Channel2Offset = table_offset[Channel2Allocation];
            }
        }

        public override void ReadScalefactor(Bitstream stream, Header header)
        {
            if (Allocation != 0) Scalefactor = scalefactors[stream.GetBits(6)];
            if (Channel2Allocation != 0) Channel2Scalefactor = scalefactors[stream.GetBits(6)];
        }

        public override bool ReadSampledata(Bitstream stream)
        {
            bool returnvalue = base.ReadSampledata(stream);
            if (Channel2Allocation != 0)
            {
                Channel2Sample = stream.GetBits(Channel2Samplelength);
            }
            return returnvalue;
        }

        public override bool PutNextSample(int channels, SynthesisFilter filter1, SynthesisFilter filter2)
        {
            base.PutNextSample(channels, filter1, filter2);
            if (Channel2Allocation != 0 && channels != OutputChannels.LEFT_CHANNEL)
            {
                float sample2 = (Channel2Sample * Channel2Factor + Channel2Offset) *
                        Channel2Scalefactor;
                if (channels == OutputChannels.BOTH_CHANNELS)
                    filter2.InputSample(sample2, Subbandnumber);
                else
                    filter1.InputSample(sample2, Subbandnumber);
            }
            return true;
        }
    }
}
