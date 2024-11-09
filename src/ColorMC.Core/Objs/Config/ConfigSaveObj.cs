namespace ColorMC.Core.Objs.Config;

/// <summary>
/// 配置文件保存项目
/// </summary>
public record ConfigSaveObj
{
    /// <summary>
    /// 名字
    /// </summary>
    public required string Name;
    /// <summary>
    /// 内容
    /// </summary>
    public required object Obj;
    /// <summary>
    /// 路径
    /// </summary>
    public required string File;
}
