using System.Diagnostics;

namespace ColorMC.Core.Game;

/// <summary>
/// 游戏进程句柄
/// </summary>
public interface IGameHandel
{
    /// <summary>
    /// 游戏UUID
    /// </summary>
    public string UUID { get; }
    /// <summary>
    /// 是否已经退出
    /// </summary>
    public bool IsExit { get; }
    /// <summary>
    /// 句柄
    /// </summary>
    public IntPtr Handel { get; }
    /// <summary>
    /// 结束进程
    /// </summary>
    public void Kill();
}

/// <summary>
/// 桌面环境游戏句柄
/// </summary>
/// <param name="process">进程</param>
/// <param name="uuid">游戏UUID</param>
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