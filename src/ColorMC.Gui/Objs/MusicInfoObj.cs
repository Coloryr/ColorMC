namespace ColorMC.Gui.Objs;

/// <summary>
/// 音乐信息
/// </summary>
public record MusicInfoObj
{
    /// <summary>
    /// 专辑
    /// </summary>
    public string? Album { get; set; }
    /// <summary>
    /// 作者
    /// </summary>
    public string? Auther { get; set; }
    /// <summary>
    /// 标题
    /// </summary>
    public string? Title { get; set; }
    /// <summary>
    /// 图片
    /// </summary>
    public byte[]? Image { get; set; }
}
