using System;
using System.Buffers.Binary;
using System.IO;
using ColorMC.Gui.Objs;

namespace ColorMC.Gui.MusicPlayer.Decoder.Wav;

public class WavFile : IDecoder
{
    private short numChannels = -1;
    private int sampleRate = -1;
    private short bitsPerSample = -1;
    private int index = 12;
    private int _dataSize;

    private readonly Stream _stream;

    public bool IsChek { get; init; }

    public WavFile(Stream stream)
    {
        var temp = new byte[4];
        stream.ReadExactly(temp);
        if (temp[0] != 'R' || temp[1] != 'I' || temp[2] != 'F' || temp[3] != 'F')
        {
            return;
        }

        stream.ReadExactly(temp);

        stream.ReadExactly(temp);
        if (temp[0] != 'W' || temp[1] != 'A' || temp[2] != 'V' || temp[3] != 'E')
        {
            return;
        }

        IsChek = true;

        _stream = stream;

        DecodeInfo();
    }

    public void Reset()
    {
        _stream.Seek(12, SeekOrigin.Begin);
        DecodeInfo();
    }

    public SoundPack? DecodeFrame()
    {
        if (_dataSize <= 0)
        {
            return null;
        }

        int pack = numChannels * 1024;
        int bps = bitsPerSample / 8;

        var temp1 = new float[pack];
        var temp = new byte[pack * bps];

        var length = Math.Min(_dataSize, temp.Length);
        _stream.ReadExactly(temp, 0, length);

        if (bitsPerSample == 8)
        {
            for (int i = 0; i < length / bps; i++)
            {
                byte sample = temp[i];
                temp1[i] = (sample - 128) / 128f;
            }
        }
        else if (bitsPerSample == 16)
        {
            for (int i = 0; i < length / bps; i++)
            {
                short sample = BitConverter.ToInt16(temp, i * 2);
                temp1[i] = sample / 32768f;
            }
        }
        else if (bitsPerSample == 24)
        {
            for (int i = 0; i < length / bps; i++)
            {
                int sample = (temp[i * 3 + 2] << 16) | (temp[i * 3 + 1] << 8) | temp[i * 3];
                if ((sample & 0x800000) != 0)
                {
                    sample |= unchecked((int)0xFF000000);
                }
                temp1[i] = sample / 8388608f;
            }
        }
        else if (bitsPerSample == 32)
        {
            for (int i = 0; i < length / bps; i++)
            {
                int sample = BitConverter.ToInt32(temp, i * 4);
                temp1[i] = sample / 2147483648f;
            }
        }

        _dataSize -= length;

        return new()
        {
            SampleRate = sampleRate,
            Channel = numChannels,
            Buff = temp1,
            Length = length / bps
        };
    }

    private void DecodeInfo()
    {
        var temp = new byte[4];
        bool haveInfo = false;
        while (index < _stream.Length)
        {
            _stream.ReadExactly(temp);
            index += 4;
            var identifier = "" + (char)temp[0] + (char)temp[1] + (char)temp[2] + (char)temp[3];
            _stream.ReadExactly(temp);
            index += 4;
            var size = BinaryPrimitives.ReadInt32LittleEndian(temp);
            if (identifier == "fmt ")
            {
                if (size != 16)
                {
                    throw new Exception("fmt it not 16");
                }
                else
                {
                    _stream.ReadExactly(temp, 0, 2);
                    var audioFormat = BinaryPrimitives.ReadInt16LittleEndian(temp);
                    index += 2;
                    if (audioFormat != 1)
                    {
                        throw new Exception("this is not pcm value");
                    }
                    else
                    {
                        _stream.ReadExactly(temp, 0, 2);
                        index += 2;
                        numChannels = BinaryPrimitives.ReadInt16LittleEndian(temp);
                        _stream.ReadExactly(temp);
                        index += 4;
                        sampleRate = BinaryPrimitives.ReadInt32LittleEndian(temp);
                        _stream.ReadExactly(temp);
                        index += 4;
                        _stream.ReadExactly(temp, 0, 2);
                        index += 2;
                        _stream.ReadExactly(temp, 0, 2);
                        index += 2;
                        bitsPerSample = BinaryPrimitives.ReadInt16LittleEndian(temp);
                    }
                }

                haveInfo = true;
            }
            else if (identifier == "data")
            {
                if (!haveInfo)
                {
                    throw new Exception("No have wav info");
                }
                _dataSize = size;
                return;
            }
            else
            {
                _stream.Seek(size, SeekOrigin.Current);
                index += size;
            }
        }
    }

    public double GetTimeCount()
    {
        return _dataSize / (bitsPerSample / 8) * numChannels / sampleRate;
    }

    public MusicInfoObj? GetInfo()
    {
        return null;
    }

    public void Dispose()
    {
        _stream.Dispose();
    }
}
