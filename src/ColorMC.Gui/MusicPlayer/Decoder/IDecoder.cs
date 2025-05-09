using System;
using ColorMC.Gui.Objs;

namespace ColorMC.Gui.MusicPlayer.Decoder;

/// <summary>
/// 音频解码器
/// </summary>
public interface IDecoder : IDisposable
{
    /// <summary>
    /// 是否是音频文件
    /// </summary>
    bool IsChek { get; }
    /// <summary>
    /// 解码一帧
    /// </summary>
    /// <returns>音频数据</returns>
    SoundPack? DecodeFrame();
    /// <summary>
    /// 获取音乐总长度
    /// </summary>
    /// <returns></returns>
    double GetTimeCount();
    /// <summary>
    /// 获取音乐信息
    /// </summary>
    /// <returns></returns>
    MusicInfoObj? GetInfo();
    /// <summary>
    /// 初始化流
    /// </summary>
    void Reset();
}
