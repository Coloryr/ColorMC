using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Gui.Objs;

/// <summary>
/// 模组下载
/// </summary>
public record DownloadModArg
{
    /// <summary>
    /// 下载项目
    /// </summary>
    public FileItemObj Item;
    /// <summary>
    /// 模组信息
    /// </summary>
    public ModInfoObj Info;
    /// <summary>
    /// 旧的模组
    /// </summary>
    public ModObj? Old;
}