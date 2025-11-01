using ColorMC.Core.Objs;

namespace ColorMC.Core.Game;

/// <summary>
/// 启动错误
/// </summary>
public class LaunchException(LaunchState state, Exception? ex = null) : Exception
{
    /// <summary>
    /// 错误状态
    /// </summary>
    public LaunchState State => state;
    /// <summary>
    /// 内联的错误
    /// </summary>
    public Exception? Inner => ex;
}
