namespace ColorMC.Gui.MusicPlayer.Decoder;

/// <summary>
/// 解码数据包
/// </summary>
public record BuffPack
{
    /// <summary>
    /// 数据
    /// </summary>
    public byte[] Buff;
    /// <summary>
    /// 长度
    /// </summary>
    public int Len;
    /// <summary>
    /// 时间长度
    /// </summary>
    public float Time;
}