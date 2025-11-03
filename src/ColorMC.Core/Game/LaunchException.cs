using ColorMC.Core.Objs;

namespace ColorMC.Core.Game;

/// <summary>
/// 启动错误
/// </summary>
public class LaunchException(LaunchError state, Exception? ex = null, string? data = null) : Exception
{
    /// <summary>
    /// 错误状态
    /// </summary>
    public LaunchError State => state;
    /// <summary>
    /// 内联的错误
    /// </summary>
    public Exception? Inner => ex;
    /// <summary>
    /// 附加的信息
    /// </summary>
    public string? InnerData => data;
}
