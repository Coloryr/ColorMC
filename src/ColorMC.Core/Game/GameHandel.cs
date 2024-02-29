using System.Diagnostics;

namespace ColorMC.Core.Game;

public interface IGameHandel
{
    public string UUID { get; }
    public bool IsExit { get; }
    public IntPtr Handel { get; }
    public void Kill();
}

public class DesktopGameHandel(Process process, string uuid) : IGameHandel
{
    public Process Process => process;
    public string UUID => uuid;
    public bool IsExit => process.HasExited;
    public IntPtr Handel => process.MainWindowHandle;

    public void Kill()
    {
        process.Kill();
    }
}