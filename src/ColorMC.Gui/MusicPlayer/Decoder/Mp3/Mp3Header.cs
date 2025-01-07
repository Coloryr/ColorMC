using System;
using System.Text;

namespace ColorMC.Gui.MusicPlayer.Decoder.Mp3;

public class Mp3Header
{
    public static readonly int[,] FrequencieTable =
    {
        {22050, 24000, 16000, 1},
        {44100, 48000, 32000, 1},
        {11025, 12000, 8000, 1}
    };    // SZD: MPEG25

    // VBR support added by E.B
    public static readonly double[] VbrTimePerFrameTable = [-1, 384, 1152, 1152];

    public static readonly float[,] MsPerFrameTable =
    {
        {8.707483f, 8.0f, 12.0f},
        {26.12245f, 24.0f, 36.0f},
        {26.12245f, 24.0f, 36.0f}
    };

    public static readonly int[,,] BitratesTable =
    {
        {
            {0, 32000, 48000, 56000, 64000, 80000, 96000,
                112000, 128000, 144000, 160000, 176000, 192000, 224000, 256000, 0},
            {0, 8000, 16000, 24000, 32000, 40000, 48000,
                    56000, 64000, 80000, 96000, 112000, 128000, 144000, 160000, 0},
            {0, 8000, 16000, 24000, 32000, 40000, 48000,
                    56000, 64000, 80000, 96000, 112000, 128000, 144000, 160000, 0}
        },
        {
            {0, 32000, 64000, 96000, 128000, 160000, 192000,
                224000, 256000, 288000, 320000, 352000, 384000, 416000, 448000, 0},
            {0, 32000, 48000, 56000, 64000, 80000, 96000,
                    112000, 128000, 160000, 192000, 224000, 256000, 320000, 384000, 0},
            {0, 32000, 40000, 48000, 56000, 64000, 80000,
                    96000, 112000, 128000, 160000, 192000, 224000, 256000, 320000, 0}
        },
        // SZD: MPEG2.5
        {
            {0, 32000, 48000, 56000, 64000, 80000, 96000,
                112000, 128000, 144000, 160000, 176000, 192000, 224000, 256000, 0},
            {0, 8000, 16000, 24000, 32000, 40000, 48000,
                    56000, 64000, 80000, 96000, 112000, 128000, 144000, 160000, 0},
            {0, 8000, 16000, 24000, 32000, 40000, 48000,
                    56000, 64000, 80000, 96000, 112000, 128000, 144000, 160000, 0}
        }
    };

    private short _checksum;
    private int _framesize;
    private int _vbrFrames;
    private int _protectionBit, _paddingBit;
    public bool Vbr { get; private set; }

    private int _vbrBytes;
    private SyncMode _syncmode = SyncMode.Initial;
    private Crc16 _crc;

    public int BitrateIndex { get; private set; }
    public int NumberOfSubbands { get; private set; }
    public int Slots { get; private set; }
    public int IntensityStereoBound { get; private set; }
    public ChannelType Mode { get; private set; }
    public FrequencyType SampleFrequency { get; private set; }
    public int ModeExtension { get; private set; }
    public LayerType Layer { get; private set; }
    public VersionType Version { get; private set; }

    /// <summary>
    /// Returns Frequency.
    /// </summary>
    /// <returns></returns>
    public int Frequency => FrequencieTable[(int)Version, (int)SampleFrequency];

    /// <summary>
    /// Returns Protection bit.
    /// </summary>
    /// <returns></returns>
    public bool Checksums => _protectionBit == 0;

    // Seeking and layer III stuff

    /// <summary>
    /// Compares computed checksum with stream checksum.
    /// </summary>
    public bool ChecksumOK => _checksum == _crc.Checksum();

    /// <summary>
    /// Read a 32-bit header from the bitstream.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="crcp"></param>
    /// <exception cref="BitStreamException"></exception>
    public bool ReadHeader(BitStream stream, Crc16?[] crcp)
    {
        int headerstring;
        int channel_bitrate;
        bool sync = false;
        do
        {
            headerstring = stream.SyncHeader(_syncmode);
            if (headerstring == -1)
            {
                return false;
            }
            if (_syncmode == SyncMode.Initial)
            {
                var temp = (headerstring >>> 19) & 1;
                Version = (VersionType)temp;
                // SZD: MPEG2.5 detection
                if (((headerstring >>> 20) & 1) == 0)
                {
                    if (Version == VersionType.Mpeg2LSF)
                    {
                        Version = VersionType.Mpeg25LSF;
                    }
                    else
                    {
                        throw new Exception("Unknow head version");
                    }
                }
                temp = (headerstring >>> 10) & 3;
                if (temp == 3)
                {
                    throw new Exception("Unknow head frequency");
                }
                SampleFrequency = (FrequencyType)temp;
            }
            Layer = (LayerType)(4 - (headerstring >>> 17) & 3);
            _protectionBit = (headerstring >>> 16) & 1;
            BitrateIndex = (headerstring >>> 12) & 0xF;
            _paddingBit = (headerstring >>> 9) & 1;
            Mode = (ChannelType)((headerstring >>> 6) & 3);
            ModeExtension = (headerstring >>> 4) & 3;
            if (Mode == ChannelType.JointStereo)
                IntensityStereoBound = (ModeExtension << 2) + 4;
            else
                IntensityStereoBound = 0; // should never be used
                                          // calculate number of subbands:
            if (Layer == LayerType.I)
            {
                NumberOfSubbands = 32;
            }
            else
            {
                channel_bitrate = BitrateIndex;
                // calculate bitrate per channel:
                if (Mode != ChannelType.SingelChannel)
                    if (channel_bitrate == 4)
                        channel_bitrate = 1;
                    else
                        channel_bitrate -= 4;
                if (channel_bitrate == 1 || channel_bitrate == 2)
                    if (SampleFrequency == FrequencyType.ThirtyTow)
                        NumberOfSubbands = 12;
                    else
                        NumberOfSubbands = 8;
                else if (SampleFrequency == FrequencyType.FourtyEight || channel_bitrate >= 3 && channel_bitrate <= 5)
                    NumberOfSubbands = 27;
                else
                    NumberOfSubbands = 30;
            }
            if (IntensityStereoBound > NumberOfSubbands)
                IntensityStereoBound = NumberOfSubbands;
            // calculate framesize and nSlots
            CalculateFramesize();
            // read framedata:
            int framesizeloaded = stream.ReadFrameData(_framesize);
            if (framesizeloaded == -1)
            {
                return false;
            }
            if (_framesize >= 0 && framesizeloaded != _framesize)
            {
                return false;
            }
            if (stream.IsSyncCurrentPosition(_syncmode))
            {
                if (_syncmode == SyncMode.Initial)
                {
                    _syncmode = SyncMode.Static;
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
        if (_protectionBit == 0)
        {
            // frame contains a crc checksum
            _checksum = (short)stream.GetBits(16);
            _crc ??= new Crc16();
            _crc.AddBits(headerstring, 16);
            crcp[0] = _crc;
        }
        else
        {
            crcp[0] = null;
        }

        return true;
    }

    /// <summary>
    /// Parse frame to extract optionnal VBR frame.
    /// </summary>
    /// <param name="firstframe"></param>
    /// <exception cref="BitStreamException"></exception>
    public void ParseVBR(byte[] firstframe)
    {
        // Trying Xing header.
        string xing = "Xing";
        byte[] tmp = new byte[4];
        int offset;
        // Compute "Xing" offset depending on MPEG version and channels.
        if (Version == VersionType.Mpeg1)
        {
            if (Mode == ChannelType.SingelChannel) offset = 21 - 4;
            else offset = 36 - 4;
        }
        else
        {
            if (Mode == ChannelType.SingelChannel) offset = 13 - 4;
            else offset = 21 - 4;
        }
        byte[] h_vbr_toc;
        Array.Copy(firstframe, offset, tmp, 0, 4);
        // Is "Xing" ?
        if (xing == Encoding.UTF8.GetString(tmp))
        {
            //Yes.
            Vbr = true;
            _vbrFrames = -1;
            _vbrBytes = -1;
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
                _vbrFrames = (int)(tmp[0] << 24 & 0xFF000000) | tmp[1] << 16 & 0x00FF0000 | tmp[2] << 8 & 0x0000FF00 | tmp[3] & 0x000000FF;
                length += 4;
            }
            // Read size (if available).
            if ((flags[3] & 1 << 1) != 0)
            {
                Array.Copy(firstframe, offset + length, tmp, 0, tmp.Length);
                _vbrBytes = (int)(tmp[0] << 24 & 0xFF000000) | tmp[1] << 16 & 0x00FF0000 | tmp[2] << 8 & 0x0000FF00 | tmp[3] & 0x000000FF;
                length += 4;
            }
            // Read TOC (if available).
            if ((flags[3] & 1 << 2) != 0)
            {
                Array.Copy(firstframe, offset + length, h_vbr_toc, 0, h_vbr_toc.Length);
                length += h_vbr_toc.Length;
            }
            // Read scale (if available).
            if ((flags[3] & 1 << 3) != 0)
            {
                Array.Copy(firstframe, offset + length, tmp, 0, tmp.Length);
            }
        }

        // Trying VBRI header.
        string vbri = "VBRI";
        offset = 36 - 4;

        Array.Copy(firstframe, offset, tmp, 0, 4);
        // Is "VBRI" ?
        if (vbri == Encoding.UTF8.GetString(tmp))
        {
            //Yes.
            Vbr = true;
            _vbrFrames = -1;
            _vbrBytes = -1;
            // Bytes.
            int length = 4 + 6;
            Array.Copy(firstframe, offset + length, tmp, 0, tmp.Length);
            _vbrBytes = (int)(tmp[0] << 24 & 0xFF000000) | tmp[1] << 16 & 0x00FF0000 | tmp[2] << 8 & 0x0000FF00 | tmp[3] & 0x000000FF;
            length += 4;
            // Frames.
            Array.Copy(firstframe, offset + length, tmp, 0, tmp.Length);
            _vbrFrames = (int)(tmp[0] << 24 & 0xFF000000) | tmp[1] << 16 & 0x00FF0000 | tmp[2] << 8 & 0x0000FF00 | tmp[3] & 0x000000FF;
        }
    }

    /// <summary>
    /// Calculates framesize in bytes excluding header size.
    /// </summary>
    public void CalculateFramesize()
    {
        if (Layer == LayerType.I)
        {
            _framesize = 12 * BitratesTable[(int)Version, 0, BitrateIndex] /
                    FrequencieTable[(int)Version, (int)SampleFrequency];
            if (_paddingBit != 0) _framesize++;
            _framesize <<= 2;        // one slot is 4 bytes long
            Slots = 0;
        }
        else
        {
            _framesize = 144 * BitratesTable[(int)Version, (int)Layer - 1, BitrateIndex] /
                    FrequencieTable[(int)Version, (int)SampleFrequency];
            if (Version == VersionType.Mpeg2LSF || Version == VersionType.Mpeg25LSF) _framesize >>= 1;    // SZD
            if (_paddingBit != 0) _framesize++;
            // Layer III slots
            if (Layer == LayerType.III)
            {
                if (Version == VersionType.Mpeg1)
                {
                    Slots = _framesize - (Mode == ChannelType.SingelChannel ? 17 : 32) // side info size
                            - (_protectionBit != 0 ? 0 : 2)                  // CRC size
                            - 4;                                             // header size
                }
                else
                {  // MPEG-2 LSF, SZD: MPEG-2.5 LSF
                    Slots = _framesize - (Mode == ChannelType.SingelChannel ? 9 : 17) // side info size
                            - (_protectionBit != 0 ? 0 : 2)                  // CRC size
                            - 4;                                             // header size
                }
            }
            else
            {
                Slots = 0;
            }
        }
        _framesize -= 4;             // subtract header size
    }

    /// <summary>
    /// Returns s/frame.
    /// </summary>
    /// <returns></returns>
    public float GetSecPerFrame()
    {
        if (Vbr)
        {
            double tpf = VbrTimePerFrameTable[(int)Layer] / Frequency;
            if (Version == VersionType.Mpeg2LSF || Version == VersionType.Mpeg25LSF) tpf /= 2;
            return (float)tpf;
        }
        else
        {
            return MsPerFrameTable[(int)Layer - 1, (int)SampleFrequency];
        }
    }

    /// <summary>
    /// Return Bitrate.
    /// </summary>
    /// <returns></returns>
    public int GetBitrate()
    {
        if (Vbr)
        {
            return (int)(_vbrBytes * 8 / (GetSecPerFrame() * _vbrFrames)) * 1000;
        }
        else
        {
            return BitratesTable[(int)Version, (int)Layer - 1, BitrateIndex];
        }
    }

    public override string ToString()
    {
        return this.GetString();
    }
}

