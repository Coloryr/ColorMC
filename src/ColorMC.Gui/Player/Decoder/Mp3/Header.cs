using System;
using System.Text;

namespace ColorMC.Gui.Player.Decoder.Mp3;

public sealed class Header
{
    public static readonly int[,] frequencies =
        {{22050, 24000, 16000, 1},
         {44100, 48000, 32000, 1},
         {11025, 12000, 8000, 1}};    // SZD: MPEG25

    /**
     * Constant for MPEG-2 LSF version
     */
    public const int MPEG2_LSF = 0;
    public const int MPEG25_LSF = 2;    // SZD

    /**
     * Constant for MPEG-1 version
     */
    public const int MPEG1 = 1;

    public const int STEREO = 0;
    public const int JOINT_STEREO = 1;
    public const int DUAL_CHANNEL = 2;
    public const int SINGLE_CHANNEL = 3;
    public const int FOURTYFOUR_POINT_ONE = 0;
    public const int FOURTYEIGHT = 1;
    public const int THIRTYTWO = 2;
    // E.B -> private to public
    public static readonly int[,,] bitrates = {
            {{0 /*free format*/, 32000, 48000, 56000, 64000, 80000, 96000,
                    112000, 128000, 144000, 160000, 176000, 192000, 224000, 256000, 0},
            {0 /*free format*/, 8000, 16000, 24000, 32000, 40000, 48000,
                    56000, 64000, 80000, 96000, 112000, 128000, 144000, 160000, 0},
            {0 /*free format*/, 8000, 16000, 24000, 32000, 40000, 48000,
                    56000, 64000, 80000, 96000, 112000, 128000, 144000, 160000, 0}},

            {{0 /*free format*/, 32000, 64000, 96000, 128000, 160000, 192000,
                    224000, 256000, 288000, 320000, 352000, 384000, 416000, 448000, 0},
            {0 /*free format*/, 32000, 48000, 56000, 64000, 80000, 96000,
                    112000, 128000, 160000, 192000, 224000, 256000, 320000, 384000, 0},
            {0 /*free format*/, 32000, 40000, 48000, 56000, 64000, 80000,
                    96000, 112000, 128000, 160000, 192000, 224000, 256000, 320000, 0}},
            // SZD: MPEG2.5
            {{0 /*free format*/, 32000, 48000, 56000, 64000, 80000, 96000,
                    112000, 128000, 144000, 160000, 176000, 192000, 224000, 256000, 0},
            {0 /*free format*/, 8000, 16000, 24000, 32000, 40000, 48000,
                    56000, 64000, 80000, 96000, 112000, 128000, 144000, 160000, 0},
            {0 /*free format*/, 8000, 16000, 24000, 32000, 40000, 48000,
                    56000, 64000, 80000, 96000, 112000, 128000, 144000, 160000, 0}},

    };
    // E.B -> private to public
    public static readonly string[,,] bitrate_str = {
            {{"free format", "32 kbit/s", "48 kbit/s", "56 kbit/s", "64 kbit/s",
                    "80 kbit/s", "96 kbit/s", "112 kbit/s", "128 kbit/s", "144 kbit/s",
                    "160 kbit/s", "176 kbit/s", "192 kbit/s", "224 kbit/s", "256 kbit/s",
                    "forbidden"},
            {"free format", "8 kbit/s", "16 kbit/s", "24 kbit/s", "32 kbit/s",
                    "40 kbit/s", "48 kbit/s", "56 kbit/s", "64 kbit/s", "80 kbit/s",
                    "96 kbit/s", "112 kbit/s", "128 kbit/s", "144 kbit/s", "160 kbit/s",
                    "forbidden"},
            {"free format", "8 kbit/s", "16 kbit/s", "24 kbit/s", "32 kbit/s",
                    "40 kbit/s", "48 kbit/s", "56 kbit/s", "64 kbit/s", "80 kbit/s",
                    "96 kbit/s", "112 kbit/s", "128 kbit/s", "144 kbit/s", "160 kbit/s",
                    "forbidden"}},

            {{"free format", "32 kbit/s", "64 kbit/s", "96 kbit/s", "128 kbit/s",
                    "160 kbit/s", "192 kbit/s", "224 kbit/s", "256 kbit/s", "288 kbit/s",
                    "320 kbit/s", "352 kbit/s", "384 kbit/s", "416 kbit/s", "448 kbit/s",
                    "forbidden"},
            {"free format", "32 kbit/s", "48 kbit/s", "56 kbit/s", "64 kbit/s",
                    "80 kbit/s", "96 kbit/s", "112 kbit/s", "128 kbit/s", "160 kbit/s",
                    "192 kbit/s", "224 kbit/s", "256 kbit/s", "320 kbit/s", "384 kbit/s",
                    "forbidden"},
            {"free format", "32 kbit/s", "40 kbit/s", "48 kbit/s", "56 kbit/s",
                    "64 kbit/s", "80 kbit/s", "96 kbit/s", "112 kbit/s", "128 kbit/s",
                    "160 kbit/s", "192 kbit/s", "224 kbit/s", "256 kbit/s", "320 kbit/s",
                    "forbidden"}},
            // SZD: MPEG2.5
            {{"free format", "32 kbit/s", "48 kbit/s", "56 kbit/s", "64 kbit/s",
                    "80 kbit/s", "96 kbit/s", "112 kbit/s", "128 kbit/s", "144 kbit/s",
                    "160 kbit/s", "176 kbit/s", "192 kbit/s", "224 kbit/s", "256 kbit/s",
                    "forbidden"},
            {"free format", "8 kbit/s", "16 kbit/s", "24 kbit/s", "32 kbit/s",
                    "40 kbit/s", "48 kbit/s", "56 kbit/s", "64 kbit/s", "80 kbit/s",
                    "96 kbit/s", "112 kbit/s", "128 kbit/s", "144 kbit/s", "160 kbit/s",
                    "forbidden"},
            {"free format", "8 kbit/s", "16 kbit/s", "24 kbit/s", "32 kbit/s",
                    "40 kbit/s", "48 kbit/s", "56 kbit/s", "64 kbit/s", "80 kbit/s",
                    "96 kbit/s", "112 kbit/s", "128 kbit/s", "144 kbit/s", "160 kbit/s",
                    "forbidden"}},
    };
    // VBR support added by E.B
    private readonly double[] h_vbr_time_per_frame = { -1, 384, 1152, 1152 };

    public short checksum;
    public int framesize;

    private int h_protection_bit,
            h_padding_bit;

    private bool h_vbr;
    private int h_vbr_frames;
    private int h_vbr_bytes;
    private byte syncmode = Bitstream.INITIAL_SYNC;
    private Crc16 crc;

    public int BitrateIndex { get; private set; }
    public int NumberOfSubbands { get; private set; }
    public int Slots { get; private set; }
    public int IntensityStereoBound { get; private set; }
    public int Mode { get; private set; }
    public int SampleFrequency { get; private set; }
    public int ModeExtension { get; private set; }
    public int Layer { get; private set; }
    public int Version { get; private set; }

    public new string ToString()
    {
        StringBuilder buffer = new();
        buffer.Append("Layer ");
        buffer.Append(LayerString());
        buffer.Append(" frame ");
        buffer.Append(ModeString());
        buffer.Append(' ');
        buffer.Append(VersionString());
        if (!Checksums())
            buffer.Append(" no");
        buffer.Append(" checksums");
        buffer.Append(' ');
        buffer.Append(SampleFrequencyString());
        buffer.Append(',');
        buffer.Append(' ');
        buffer.Append(BitrateString());

        return buffer.ToString();
    }

    // Functions to query header contents:

    /**
     * Read a 32-bit header from the bitstream.
     */
    public void ReadHeader(Bitstream stream, Crc16?[] crcp)
    {
        int headerstring;
        int channel_bitrate;
        bool sync = false;
        do
        {
            headerstring = stream.SyncHeader(syncmode);
            if (syncmode == Bitstream.INITIAL_SYNC)
            {
                Version = ((headerstring >>> 19) & 1);
                if (((headerstring >>> 20) & 1) == 0) // SZD: MPEG2.5 detection
                    if (Version == MPEG2_LSF)
                        Version = MPEG25_LSF;
                    else
                        throw new BitstreamException(BitstreamErrors.UNKNOWN_ERROR, null);
                if ((SampleFrequency = ((headerstring >>> 10) & 3)) == 3)
                {
                    throw new BitstreamException(BitstreamErrors.UNKNOWN_ERROR, null);
                }
            }
            Layer = 4 - (headerstring >>> 17) & 3;
            h_protection_bit = (headerstring >>> 16) & 1;
            BitrateIndex = (headerstring >>> 12) & 0xF;
            h_padding_bit = (headerstring >>> 9) & 1;
            Mode = ((headerstring >>> 6) & 3);
            ModeExtension = (headerstring >>> 4) & 3;
            if (Mode == JOINT_STEREO)
                IntensityStereoBound = (ModeExtension << 2) + 4;
            else
                IntensityStereoBound = 0; // should never be used
                                              // calculate number of subbands:
            if (Layer == 1)
                NumberOfSubbands = 32;
            else
            {
                channel_bitrate = BitrateIndex;
                // calculate bitrate per channel:
                if (Mode != SINGLE_CHANNEL)
                    if (channel_bitrate == 4)
                        channel_bitrate = 1;
                    else
                        channel_bitrate -= 4;
                if ((channel_bitrate == 1) || (channel_bitrate == 2))
                    if (SampleFrequency == THIRTYTWO)
                        NumberOfSubbands = 12;
                    else
                        NumberOfSubbands = 8;
                else if ((SampleFrequency == FOURTYEIGHT) || ((channel_bitrate >= 3) && (channel_bitrate <= 5)))
                    NumberOfSubbands = 27;
                else
                    NumberOfSubbands = 30;
            }
            if (IntensityStereoBound > NumberOfSubbands)
                IntensityStereoBound = NumberOfSubbands;
            // calculate framesize and nSlots
            CalculateFramesize();
            // read framedata:
            int framesizeloaded = stream.ReadFrameData(framesize);
            if (framesizeloaded == 0)
            {
                throw new BitstreamException(BitstreamErrors.STREAM_EOF, null);
            }
            if ((framesize >= 0) && (framesizeloaded != framesize))
            {
                // Data loaded does not match to expected framesize,
                // it might be an ID3v1 TAG. (Fix 11/17/04).
                throw new BitstreamException(BitstreamErrors.INVALIDFRAME, null);
            }
            if (stream.IsSyncCurrentPosition(syncmode))
            {
                if (syncmode == Bitstream.INITIAL_SYNC)
                {
                    syncmode = Bitstream.STRICT_SYNC;
                    stream.SetSyncword((int)(headerstring & 0xFFF80CC0));
                }
                sync = true;
            }
            else
            {
                stream.UnreadFrame();
            }
        }
        while (!sync);
        stream.ParseFrame();
        if (h_protection_bit == 0)
        {
            // frame contains a crc checksum
            checksum = (short)stream.GetBits(16);
            crc ??= new Crc16();
            crc.AddBits(headerstring, 16);
            crcp[0] = crc;
        }
        else
            crcp[0] = null;
    }

    /**
     * Parse frame to extract optionnal VBR frame.
     *
     * @param firstframe
     * @author E.B (javalayer@javazoom.net)
     */
    public void ParseVBR(byte[] firstframe)
    {
        // Trying Xing header.
        string xing = "Xing";
        byte[] tmp = new byte[4];
        int offset;
        // Compute "Xing" offset depending on MPEG version and channels.
        if (Version == MPEG1)
        {
            if (Mode == SINGLE_CHANNEL) offset = 21 - 4;
            else offset = 36 - 4;
        }
        else
        {
            if (Mode == SINGLE_CHANNEL) offset = 13 - 4;
            else offset = 21 - 4;
        }
        byte[] h_vbr_toc;
        try
        {
            Array.Copy(firstframe, offset, tmp, 0, 4);
            // Is "Xing" ?
            if (xing == Encoding.UTF8.GetString(tmp))
            {
                //Yes.
                h_vbr = true;
                h_vbr_frames = -1;
                h_vbr_bytes = -1;
                h_vbr_toc = new byte[100];

                int length = 4;
                // Read flags.
                byte[] flags = new byte[4];
                Array.Copy(firstframe, offset + length, flags, 0, flags.Length);
                length += flags.Length;
                // Read number of frames (if available).
                if ((flags[3] & 1) != 0)
                {
                    Array.Copy(firstframe, offset + length, tmp, 0, tmp.Length);
                    h_vbr_frames = (int)((tmp[0] << 24) & 0xFF000000) | ((tmp[1] << 16) & 0x00FF0000) | ((tmp[2] << 8) & 0x0000FF00) | ((tmp[3]) & 0x000000FF);
                    length += 4;
                }
                // Read size (if available).
                if ((flags[3] & (1 << 1)) != 0)
                {
                    Array.Copy(firstframe, offset + length, tmp, 0, tmp.Length);
                    h_vbr_bytes = (int)((tmp[0] << 24) & 0xFF000000) | ((tmp[1] << 16) & 0x00FF0000) | ((tmp[2] << 8) & 0x0000FF00) | ((tmp[3]) & 0x000000FF);
                    length += 4;
                }
                // Read TOC (if available).
                if ((flags[3] & (byte)(1 << 2)) != 0)
                {
                    Array.Copy(firstframe, offset + length, h_vbr_toc, 0, h_vbr_toc.Length);
                    length += h_vbr_toc.Length;
                }
                // Read scale (if available).
                if ((flags[3] & (byte)(1 << 3)) != 0)
                {
                    Array.Copy(firstframe, offset + length, tmp, 0, tmp.Length);
                }
            }
        }
        catch (IndexOutOfRangeException e)
        {
            throw new BitstreamException("XingVBRHeader Corrupted", e);
        }

        // Trying VBRI header.
        string vbri = "VBRI";
        offset = 36 - 4;
        try
        {
            Array.Copy(firstframe, offset, tmp, 0, 4);
            // Is "VBRI" ?
            if (vbri == Encoding.UTF8.GetString(tmp))
            {
                //Yes.
                h_vbr = true;
                h_vbr_frames = -1;
                h_vbr_bytes = -1;
                // Bytes.
                int length = 4 + 6;
                Array.Copy(firstframe, offset + length, tmp, 0, tmp.Length);
                h_vbr_bytes = (int)((tmp[0] << 24) & 0xFF000000) | ((tmp[1] << 16) & 0x00FF0000) | ((tmp[2] << 8) & 0x0000FF00) | ((tmp[3]) & 0x000000FF);
                length += 4;
                // Frames.
                Array.Copy(firstframe, offset + length, tmp, 0, tmp.Length);
                h_vbr_frames = (int)((tmp[0] << 24) & 0xFF000000) | ((tmp[1] << 16) & 0x00FF0000) | ((tmp[2] << 8) & 0x0000FF00) | ((tmp[3]) & 0x000000FF);
            }
        }
        catch (IndexOutOfRangeException e)
        {
            throw new BitstreamException("VBRIVBRHeader Corrupted", e);
        }
    }

    /**
     * Returns Frequency.
     */
    public int Frequency()
    {
        return frequencies[Version, SampleFrequency];
    }

    /**
     * Returns Protection bit.
     */
    public bool Checksums()
    {
        return h_protection_bit == 0;
    }

    // Seeking and layer III stuff

    /**
     * Returns Checksum flag.
     * Compares computed checksum with stream checksum.
     */
    public bool ChecksumOK()
    {
        return checksum == crc.Checksum();
    }

    /**
     * Calculate Frame size.
     * Calculates framesize in bytes excluding header size.
     */
    public void CalculateFramesize()
    {

        if (Layer == 1)
        {
            framesize = (12 * bitrates[Version, 0, BitrateIndex]) /
                    frequencies[Version, SampleFrequency];
            if (h_padding_bit != 0) framesize++;
            framesize <<= 2;        // one slot is 4 bytes long
            Slots = 0;
        }
        else
        {
            framesize = (144 * bitrates[Version, Layer - 1, BitrateIndex]) /
                    frequencies[Version, SampleFrequency];
            if (Version == MPEG2_LSF || Version == MPEG25_LSF) framesize >>= 1;    // SZD
            if (h_padding_bit != 0) framesize++;
            // Layer III slots
            if (Layer == 3)
            {
                if (Version == MPEG1)
                {
                    Slots = framesize - ((Mode == SINGLE_CHANNEL) ? 17 : 32) // side info size
                            - ((h_protection_bit != 0) ? 0 : 2)               // CRC size
                            - 4;                                             // header size
                }
                else
                {  // MPEG-2 LSF, SZD: MPEG-2.5 LSF
                    Slots = framesize - ((Mode == SINGLE_CHANNEL) ? 9 : 17) // side info size
                            - ((h_protection_bit != 0) ? 0 : 2)               // CRC size
                            - 4;                                             // header size
                }
            }
            else
            {
                Slots = 0;
            }
        }
        framesize -= 4;             // subtract header size
    }

    /**
     * Returns ms/frame.
     *
     * @return milliseconds per frame
     */
    public float MsPerFrame() // E.B
    {
        if (h_vbr)
        {
            double tpf = h_vbr_time_per_frame[Layer] / Frequency();
            if ((Version == MPEG2_LSF) || (Version == MPEG25_LSF)) tpf /= 2;
            return ((float)(tpf * 1000));
        }
        else
        {
            float[,] ms_per_frame_array = {{8.707483f, 8.0f, 12.0f},
                    {26.12245f, 24.0f, 36.0f},
                    {26.12245f, 24.0f, 36.0f}};
            return ms_per_frame_array[Layer - 1, SampleFrequency];
        }
    }

    // functions which return header informations as strings:

    /**
     * Return Layer version.
     */
    public string? LayerString()
    {
        return Layer switch
        {
            1 => "I",
            2 => "II",
            3 => "III",
            _ => null,
        };
    }

    /**
     * Return Bitrate.
     *
     * @return bitrate in bps
     */
    public string BitrateString()
    {
        if (h_vbr)
        {
            return Bitrate() / 1000 + " kb/s";
        }
        else return bitrate_str[Version, Layer - 1, BitrateIndex];
    }

    /**
     * Return Bitrate.
     *
     * @return bitrate in bps and average bitrate for VBR header
     */
    public int Bitrate()
    {
        if (h_vbr)
        {
            return ((int)((h_vbr_bytes * 8) / (MsPerFrame() * h_vbr_frames))) * 1000;
        }
        else return bitrates[Version, Layer - 1, BitrateIndex];
    }

    /**
     * Returns Frequency
     *
     * @return frequency string in kHz
     */
    public string? SampleFrequencyString()
    {
        switch (SampleFrequency)
        {
            case THIRTYTWO:
                if (Version == MPEG1)
                    return "32 kHz";
                else if (Version == MPEG2_LSF)
                    return "16 kHz";
                else    // SZD
                    return "8 kHz";
            case FOURTYFOUR_POINT_ONE:
                if (Version == MPEG1)
                    return "44.1 kHz";
                else if (Version == MPEG2_LSF)
                    return "22.05 kHz";
                else    // SZD
                    return "11.025 kHz";
            case FOURTYEIGHT:
                if (Version == MPEG1)
                    return "48 kHz";
                else if (Version == MPEG2_LSF)
                    return "24 kHz";
                else    // SZD
                    return "12 kHz";
        }
        return null;
    }

    /**
     * Returns Mode.
     */
    public string? ModeString()
    {
        return Mode switch
        {
            STEREO => "Stereo",
            JOINT_STEREO => "Joint stereo",
            DUAL_CHANNEL => "Dual channel",
            SINGLE_CHANNEL => "Single channel",
            _ => null
        };
    }

    /**
     * Returns Version.
     *
     * @return MPEG-1 or MPEG-2 LSF or MPEG-2.5 LSF
     */
    public string? VersionString()
    {
        return Version switch
        {
            MPEG1 => "MPEG-1",
            MPEG2_LSF => "MPEG-2 LSF",
            MPEG25_LSF => "MPEG-2.5 LSF", // SZD
            _ => null
        };
    }
}

