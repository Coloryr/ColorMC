using System;
using System.IO;
using ColorMC.Gui.Objs;

namespace ColorMC.Gui.MusicPlayer.Decoder.Flac;

public class FlacFile : IDecoder
{
    public StreamInfoBlock StreamInfo { get; protected set; }
    public VorbisCommenBlock VorbisCommen { get; protected set; }
    public SeekTableBlock SeekTable { get; protected set; }
    public PictureBlock Picture { get; protected set; }

    public bool IsChek { get; init; }

    private readonly FlacStream _flacStream;

    private readonly FlacDecoder _flacDecoder;

    public FlacFile(Stream stream)
    {
        _flacStream = new(stream);
        _flacDecoder = new(_flacStream);

        if (_flacStream.CheckHead())
        {
            DecodeInfo();
            IsChek = true;
        }
    }

    public void Reset()
    {
        _flacStream.Reset();
        FlacInfoBlock? first;
        do
        {
            first = _flacStream.DecodeInfo();
        }
        while (first != null);
    }

    public SoundPack? DecodeFrame()
    {
        var meta = new FlacSoundBlock(_flacStream);
        if (meta.IsLast)
        {
            return null;
        }
        if (meta.BitsPerSample == -1 || meta.BitsPerSample != StreamInfo.BitsPerSample)
        {
            throw new Exception("Sample depth mismatch");
        }

        _flacDecoder.BlockSize = meta.BlockSize;
        int[][] temp = new int[meta.NumChannels][];
        for (int a = 0; a < temp.Length; a++)
        {
            temp[a] = new int[meta.BlockSize];
        }
        _flacDecoder.DecodeSubframes(meta.BitsPerSample, meta.ChannelAssignment, temp);

        if (_flacStream.ReadUintWithCrc((8 - _flacStream.GetBitPosition()) % 8) != 0)
        {
            throw new Exception("Invalid padding bits");
        }
        int computedCrc16 = _flacStream.GetCrc16();
        if (_flacStream.ReadUintWithCrc(16) != computedCrc16)
        {
            throw new Exception("CRC-16 mismatch");
        }

        int sampleLen = 0;
        var samples = new float[meta.NumChannels * meta.BlockSize];
        for (int i = 0; i < meta.BlockSize; i++)
        {
            for (int ch = 0; ch < meta.NumChannels; ch++)
            {
                int val = temp[ch][i];
                if (meta.BitsPerSample == 8)
                {
                    samples[sampleLen++] = val / 128f;
                }
                else if (meta.BitsPerSample == 16)
                {
                    samples[sampleLen++] = val / 32768f;
                }
                else if (meta.BitsPerSample == 24)
                {
                    samples[sampleLen++] = val / 16777216f;
                }
                else if (meta.BitsPerSample == 32)
                {
                    samples[sampleLen++] = val / 1099511627776f;
                }
            }
        }

        return new()
        {
            Length = sampleLen,
            Buff = samples,
            Channel = meta.NumChannels,
            SampleRate = meta.SampleRate,
            Time = (float)meta.BlockSize / meta.SampleRate
        };
    }

    private void DecodeInfo()
    {
        var first = _flacStream.DecodeInfo();
        if (first is not StreamInfoBlock info)
        {
            throw new Exception("first block is not streaminfo");
        }
        else
        {
            StreamInfo = info;
        }

        do
        {
            first = _flacStream.DecodeInfo();
            if (first is VorbisCommenBlock block)
            {
                VorbisCommen = block;
            }
            else if (first is SeekTableBlock block1)
            {
                SeekTable = block1;
            }
            else if (first is PictureBlock block2)
            {
                Picture = block2;
            }
        }
        while (first != null);
    }

    public void Dispose()
    {
        _flacStream.Dispose();
    }

    public double GetTimeCount()
    {
        return (float)StreamInfo.SampleCount / StreamInfo.SampleRate;
    }

    public MusicInfoObj? GetInfo()
    {
        if (VorbisCommen == null)
        {
            return null;
        }
        var vorbisCommen = VorbisCommen;
        var info = new MusicInfoObj();
        if (vorbisCommen.Vorbis.TryGetValue("Title", out var temp1)
            || vorbisCommen.Vorbis.TryGetValue("TITLE", out temp1))
        {
            info.Title = temp1;
        }
        if (vorbisCommen.Vorbis.TryGetValue("Artist", out temp1)
            || vorbisCommen.Vorbis.TryGetValue("ARTIST", out temp1))
        {
            info.Auther = temp1;
        }
        if (vorbisCommen.Vorbis.TryGetValue("Album", out temp1)
            || vorbisCommen.Vorbis.TryGetValue("ALBUM", out temp1))
        {
            info.Album = temp1;
        }

        if (Picture != null)
        {
            info.Image = Picture.Picture;
        }

        return info;
    }
}
