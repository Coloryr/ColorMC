namespace ColorMC.Core.Objs;

public class LaunchException : Exception
{
    public LaunchState State { get; private set; }
    public Exception? Ex { get; private set; }
    public LaunchException(LaunchState state, string message) : base(message)
    {
        State = state;
    }

    public LaunchException(LaunchState state, Exception ex)
    {
        State = state;
        Ex = ex;
    }
}
