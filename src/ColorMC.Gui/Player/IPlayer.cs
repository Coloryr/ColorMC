namespace ColorMC.Gui.Player;

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
    /// <param name="numChannels">通道</param>
    /// <param name="bitsPerSample">bps</param>
    /// <param name="buff">数据</param>
    /// <param name="length">长度</param>
    /// <param name="sampleRate">采样率</param>
    void Write(int numChannels, int bitsPerSample, byte[] buff, int length, int sampleRate);
}
