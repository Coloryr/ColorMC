using System;
using System.IO;
using ColorMC.Gui.Objs;

namespace ColorMC.Gui.MusicPlayer.Decoder.Mp3;

public class Mp3File : IDecoder
{
    /// <summary>
    /// The Obuffer instance that will receive the decoded PCM samples.
    /// </summary>
    private SampleBuffer _output;
    /// <summary>
    /// Synthesis filter for the left channel.
    /// </summary>
    private SynthesisFilter _filterLeft;
    /// <summary>
    /// Sythesis filter for the right channel.
    /// </summary>
    private SynthesisFilter _filterRight;
    /// <summary>
    /// The decoder used to decode layer III frames.
    /// </summary>
    private LayerIIIDecoder _l3decoder;
    private LayerIIDecoder _l2decoder;
    private LayerIDecoder _l1decoder;

    private bool _initialized;
    private readonly BitStream _bitstream;

    public int OutputFrequency { get; private set; }
    public int OutputChannels { get; private set; }
    public int Bitrate { get; private set; }

    public bool IsChek { get; init; }

    public Mp3File(Stream stream)
    {
        _bitstream = new BitStream(stream);
        if (LoadInfo())
        {
            IsChek = true;
        }
    }

    public void Reset()
    {
        _bitstream.CloseFrame();
        _bitstream.Reset();
        LoadInfo();
    }

    /// <summary>
    /// Decodes one frame from an MPEG audio bitstream.
    /// </summary>
    /// <returns></returns>
    public SoundPack? DecodeFrame()
    {
        var header = _bitstream.ReadFrame();
        if (header == null)
        {
            return null;
        }
        var layer = header.Layer;
        _output.ClearBuffer();
        var decoder = RetrieveDecoder(header, _bitstream, layer);
        decoder.DecodeFrame();
        var pack = new SoundPack
        {
            Buff = _output.Buffer,
            Length = _output.GetBufferLength(),
            Time = header.GetSecPerFrame() / 1000,
            SampleRate = OutputFrequency,
            Channel = OutputChannels
        };
        _bitstream.CloseFrame();
        return pack;
    }

    public double GetTimeCount()
    {
        double duration = 0;
        var frame = _bitstream.ReadFrame();
        while (frame != null)
        {
            duration += frame.GetSecPerFrame();
            _bitstream.CloseFrame();
            frame = _bitstream.ReadFrame();
        }
        _bitstream.CloseFrame();

        _bitstream.Reset();
        LoadInfo();

        return duration / 1000;
    }

    public void Dispose()
    {
        _bitstream.Dispose();
    }

    private bool LoadInfo()
    {
        var header = _bitstream.ReadFrame();
        if (header == null)
        {
            return false;
        }
        if (!_initialized)
        {
            Initialize(header);
        }

        return true;
    }

    protected IFrameDecoder RetrieveDecoder(Mp3Header header, BitStream stream, LayerType layer)
    {
        IFrameDecoder? decoder = null;

        switch (layer)
        {
            case LayerType.III:
                _l3decoder ??= new LayerIIIDecoder(stream,
                            header, _filterLeft, _filterRight,
                            _output);
                decoder = _l3decoder;
                break;
            case LayerType.II:
                _l2decoder ??= new LayerIIDecoder(stream,
                            header, _filterLeft, _filterRight,
                            _output);
                decoder = _l2decoder;
                break;
            case LayerType.I:
                _l1decoder ??= new LayerIDecoder(stream,
                            header, _filterLeft, _filterRight,
                            _output);
                decoder = _l1decoder;
                break;
        }

        if (decoder == null)
        {
            throw new Exception("Unsupported Layer");
        }

        return decoder;
    }

    private void Initialize(Mp3Header header)
    {
        float scalefactor = 32700.0f;

        var mode = header.Mode;
        int channels = mode == ChannelType.SingelChannel ? 1 : 2;

        _output ??= new SampleBuffer(channels);

        _filterLeft = new SynthesisFilter(0, scalefactor);
        _filterRight = new SynthesisFilter(1, scalefactor);

        OutputChannels = channels;
        OutputFrequency = header.Frequency;
        Bitrate = header.GetBitrate();

        _initialized = true;
    }

    public MusicInfoObj? GetInfo()
    {
        var id3 = _bitstream.Id3;
        if (id3 == null)
        {
            return null;
        }
        return new MusicInfoObj()
        {
            Title = id3.Title,
            Album = id3.Album,
            Auther = id3.Auther,
            Image = id3.Image
        };
    }
}

