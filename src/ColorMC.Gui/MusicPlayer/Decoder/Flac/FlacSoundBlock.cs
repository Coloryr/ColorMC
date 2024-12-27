using System;
using System.Collections.Generic;

namespace ColorMC.Gui.MusicPlayer.Decoder.Flac;

public class FlacSoundBlock
{
    private static readonly Dictionary<int, int> BlockSizeTable = new()
    {
        {1, 192}, {2, 576}, {3, 1152}, {4, 2304}, {5, 4608},
        {8, 256}, {9, 512}, {10, 1024}, {11, 2048}, {12, 4096},
        {13, 8192}, {14, 16384}, {15, 32768}
    };

    private static readonly Dictionary<int, int> SampleDepthTable = new()
    {
        {1, 8}, {2, 12}, {4, 16}, {5, 20}, {6, 24}, {7, 32}
    };

    private static readonly Dictionary<int, int> SampleRateTable = new()
    {
        {1, 88200}, {2, 176400}, {3, 192000}, {4, 8000},
        {5, 16000}, {6, 22050}, {7, 24000}, {8, 32000},
        {9, 44100}, {10, 48000}, {11, 96000}
    };

    public int FrameIndex { get; private set; } = -1;
    public long SampleOffset { get; private set; } = -1;
    public int NumChannels { get; private set; } = -1;
    public int ChannelAssignment { get; private set; } = -1;
    public int BlockSize { get; private set; } = -1;
    public int SampleRate { get; private set; } = -1;
    public int BitsPerSample { get; private set; } = -1;

    public bool IsLast { get; private set; }

    public FlacSoundBlock(FlacStream stream)
    {
        stream.ResetCrcs();
        int temp = stream.ReadUintWithCrc(8);
        if (temp == -1)
        {
            IsLast = true;
            return;
        }
        int sync = temp << 6 | stream.ReadUintWithCrc(6);
        if (sync != 0x3FFE)
        {
            throw new Exception("Sync code expected");
        }

        if (stream.ReadUintWithCrc(1) != 0)
        {
            throw new Exception("Reserved bit error");
        }
        int blockStrategy = stream.ReadUintWithCrc(1);
        int blockSizeCode = stream.ReadUintWithCrc(4);
        int sampleRateCode = stream.ReadUintWithCrc(4);
        int chanAsgn = stream.ReadUintWithCrc(4);
        ChannelAssignment = chanAsgn;
        if (chanAsgn < 8)
        {
            NumChannels = chanAsgn + 1;
        }
        else if (8 <= chanAsgn && chanAsgn <= 10)
        {
            NumChannels = 2;
        }
        else
        {
            throw new Exception("unsupport channel assignment");
        }
        BitsPerSample = GetSampleDepth(stream.ReadUintWithCrc(3));
        if (stream.ReadUintWithCrc(1) != 0)
        {
            throw new Exception("Reserved bit error");
        }

        long position = stream.ReadCodedNumber();
        if (blockStrategy == 0)
        {
            if ((position >>> 31) != 0)
            {
                throw new Exception("Frame index too large");
            }
            FrameIndex = (int)position;
            SampleOffset = -1;
        }
        else if (blockStrategy == 1)
        {
            SampleOffset = position;
            FrameIndex = -1;
        }

        BlockSize = GetBlockSize(blockSizeCode, stream);
        SampleRate = GetSampleRate(sampleRateCode, stream);
        int computedCrc8 = stream.GetCrc8();
        if (stream.ReadUintWithCrc(8) != computedCrc8)
        {
            throw new Exception("CRC-8 mismatch");
        }
    }

    private static int GetSampleRate(int code, FlacStream stream)
    {
        if ((code >>> 4) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(code));
        }
        switch (code)
        {
            case 0:
                return -1;
            case 12:
                return stream.ReadUintWithCrc(8);
            case 13:
                return stream.ReadUintWithCrc(16);
            case 14:
                return stream.ReadUintWithCrc(16) * 10;
            case 15:
                throw new Exception("can't get sample rate");
            default:
                if (!SampleRateTable.TryGetValue(code, out var result))
                {
                    throw new Exception();
                }
                return result;
        }
    }

    private static int GetBlockSize(int code, FlacStream stream)
    {
        if ((code >>> 4) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(code));
        }
        switch (code)
        {
            case 0:
                throw new Exception("can't get block size");
            case 6:
                return stream.ReadUintWithCrc(8) + 1;
            case 7:
                return stream.ReadUintWithCrc(16) + 1;
            default:
                if (!BlockSizeTable.TryGetValue(code, out var result))
                {
                    throw new Exception("can't get block size");
                }
                return result;
        }
    }

    private static int GetSampleDepth(int code)
    {
        if ((code >>> 3) != 0)
        {
            throw new ArgumentOutOfRangeException(nameof(code));
        }
        if (!SampleDepthTable.TryGetValue(code, out var result))
        {
            throw new Exception("can't get bit depth");
        }
        return result;
    }
}
