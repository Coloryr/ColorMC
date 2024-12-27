using System;

namespace ColorMC.Gui.MusicPlayer.Decoder.Flac;

public class StreamInfoBlock : FlacInfoBlock
{
    /// <summary>
    /// 流中使用的最小数据块大小 （以样本为单位），不包括最后一个数据块。
    /// </summary>
    public int MinBlock { get; private set; }
    /// <summary>
    /// 流中使用的最大数据块大小 （以样本为单位）。
    /// </summary>
    public int MaxBlock { get; private set; }
    /// <summary>
    /// 流中使用的最小帧大小（以字节为单位）。值 0 表示该值未知。
    /// </summary>
    public int StreamMin { get; private set; }
    /// <summary>
    /// 流中使用的最大帧大小（以字节为单位）。值 0 表示该值未知。
    /// </summary>
    public int StreamMax { get; private set; }
    /// <summary>
    /// 采样率（以 Hz 为单位）。
    /// </summary>
    public int SampleRate { get; private set; }
    /// <summary>
    /// （通道数）-1。FLAC 支持 1 到 8 个通道。
    /// </summary>
    public int Channel { get; private set; }
    /// <summary>
    /// （每个样本的位数）-1。FLAC 支持每个样本 4 到 32 位。
    /// </summary>
    public int BitsPerSample { get; private set; }
    /// <summary>
    /// 流中的通道间样本总数。此处的值为 0 表示总样本数未知。
    /// </summary>
    public long SampleCount { get; private set; }
    /// <summary>
    /// 未编码音频数据的 MD5 校验和。这允许解码器确定音频数据中是否存在错误，即使存在错误，但 bitstream 本身是有效的。值 0 表示该值未知。
    /// </summary>
    public byte[] Md5 { get; private set; }

    public StreamInfoBlock(FlacStream stream, bool last, BlockType type, int size) : base(last, type, size)
    {
        var data = new byte[size];
        stream.ReadExactly(data);

        MinBlock = FlacStream.ReadInt16BE(data, 0);
        MaxBlock = FlacStream.ReadInt16BE(data, 2);
        StreamMin = FlacStream.ReadInt24BE(data, 4);
        StreamMax = FlacStream.ReadInt24BE(data, 7);
        SampleRate = (FlacStream.ReadInt24BE(data, 10) >> 4) & 0xFFFFF;
        Channel = ((data[12] >> 1) & 0x07) + 1;
        BitsPerSample = (((data[12] & 0x01) << 4) | ((data[13] >> 4) & 0x0F)) + 1;
        SampleCount = FlacStream.ReadInt36BE(data, 13);
        Md5 = data[18..34];

        if (MinBlock < 16)
        {
            throw new Exception("Minimum block size less than 16");
        }
        if (MaxBlock > 65535)
        {
            throw new Exception("Maximum block size greater than 65535");
        }
        if (MaxBlock < MinBlock)
        {
            throw new Exception("Maximum block size less than minimum block size");
        }
        if (StreamMin != 0 && StreamMax != 0 && StreamMax < StreamMin)
        {
            throw new Exception("Maximum frame size less than minimum frame size");
        }
        if (SampleRate == 0 || SampleRate > 655350)
        {
            throw new Exception("Invalid sample rate");
        }
    }
}
