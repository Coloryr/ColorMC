namespace ColorMC.Core.Objs;

/// <summary>
/// 启动错误
/// </summary>
public class LaunchException(LaunchState state, string message, Exception? ex = null) : Exception(message)
{
    public LaunchState State { get; private set; } = state;
    public Exception? Ex { get; private set; } = ex;
}
