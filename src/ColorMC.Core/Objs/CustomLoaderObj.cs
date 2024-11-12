namespace ColorMC.Core.Objs;

/// <summary>
/// 自定义加载器
/// </summary>
public record CustomLoaderObj
{
    /// <summary>
    /// 类型
    /// </summary>
    public CustomLoaderType Type;
    /// <summary>
    /// 数据
    /// </summary>
    public object Loader;
}