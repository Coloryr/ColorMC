using ColorMC.Core.Objs.Optifine;

namespace ColorMC.Gui.Objs;

/// <summary>
/// Optifine显示项目
/// </summary>
public record OptifineDisplayObj
{
    /// <summary>
    /// 游戏版本
    /// </summary>
    public string MC { get; set; }
    /// <summary>
    /// 版本
    /// </summary>
    public string Version { get; set; }
    /// <summary>
    /// 日期
    /// </summary>
    public string Date { get; set; }
    /// <summary>
    /// Forge版本
    /// </summary>
    public string Forge { get; set; }

    /// <summary>
    /// 数据
    /// </summary>
    public OptifineObj Data;
}
