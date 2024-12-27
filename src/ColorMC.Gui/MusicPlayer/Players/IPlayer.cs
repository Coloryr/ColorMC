using ColorMC.Gui.MusicPlayer.Decoder;

namespace ColorMC.Gui.MusicPlayer.Players;

/// <summary>
/// 音频输出
/// </summary>
public interface IPlayer
{
    /// <summary>
    /// 音量
    /// </summary>
    float Volume { set; }
    /// <summary>
    /// 暂停
    /// </summary>
    void Pause();
    /// <summary>
    /// 播放
    /// </summary>
    void Play();
    /// <summary>
    /// 停止
    /// </summary>
    void Stop();
    /// <summary>
    /// 关闭
    /// </summary>
    void Close();
    /// <summary>
    /// 写数据
    /// </summary>
    void Write(SoundPack pack);
    /// <summary>
    /// 等待播放结束
    /// </summary>
    void WaitDone();
}