namespace ColorMC.Core.Objs;

public class LaunchException : Exception
{
    public LaunchState State { get; private set; }
    public Exception? Ex { get; private set; }
    public LaunchException(LaunchState state, string message, Exception? ex = null) : base(message)
    {
        State = state;
    }
}
