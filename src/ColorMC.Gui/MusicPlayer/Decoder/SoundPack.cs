namespace ColorMC.Gui.MusicPlayer.Decoder;

/// <summary>
/// 解码数据包
/// </summary>
public record SoundPack
{
    /// <summary>
    /// 数据
    /// </summary>
    public float[] Buff;
    /// <summary>
    /// 长度
    /// </summary>
    public int Length;
    /// <summary>
    /// 时间长度
    /// </summary>
    public float Time;
    /// <summary>
    /// 通道
    /// </summary>
    public int Channel;
    /// <summary>
    /// 采样率
    /// </summary>
    public int SampleRate;
}