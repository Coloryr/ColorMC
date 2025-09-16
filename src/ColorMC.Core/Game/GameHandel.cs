using System.Diagnostics;
using System.Text;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Game;

/// <summary>
/// 游戏进程句柄
/// </summary>
public interface IGameHandel
{
    /// <summary>
    /// 游戏实例UUID
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
public class DesktopGameHandel : IGameHandel
{
    /// <summary>
    /// 游戏进程
    /// </summary>
    public Process Process { get; init; }

    public string UUID => _game.UUID;
    public bool IsExit => Process.HasExited;
    public IntPtr Handel => Process.MainWindowHandle;

    private readonly GameSettingObj _game;
    private bool _stop;

    public DesktopGameHandel(GameRunObj run)
    {
        _game = run.Obj;

        Process = new Process()
        {
            EnableRaisingEvents = true
        };
        Process.StartInfo.FileName = run.Path;
        Process.StartInfo.WorkingDirectory = run.Obj.GetGamePath();
        Directory.CreateDirectory(Process.StartInfo.WorkingDirectory);
        foreach (var item in run.Arg)
        {
            Process.StartInfo.ArgumentList.Add(item);
        }
        foreach (var item in run.Env)
        {
            Process.StartInfo.Environment.Add(item.Key, item.Value);
        }

        Process.StartInfo.RedirectStandardInput = true;
        Process.StartInfo.RedirectStandardOutput = true;
        Process.StartInfo.RedirectStandardError = true;

        Process.Exited += (_, _) =>
        {
            _stop = true;
            ColorMCCore.OnGameExit(_game, run.Auth, Process.ExitCode);
            Process.Dispose();
        };

        if (ProcessUtils.Launch(Process, run.Admin))
        {
            ColorMCCore.OnGameLog(_game, LanguageHelper.Get("Core.Game.Info2"));
        }

        //监听日志
        Encoding? encoding = null;
        if (_game.Encoding is LogEncoding.GBK)
        {
            try
            {
                encoding = Encoding.GetEncoding("gbk");
            }
            catch
            {

            }
        }

        encoding ??= Encoding.UTF8;

        new Thread(() =>
        {
            using var reader = new StreamReader(Process.StandardOutput.BaseStream, encoding);
            while (!_stop)
            {
                var output = reader.ReadLine();
                ColorMCCore.OnGameLog(_game, output);
            }
        })
        {
            Name = "ColorMC_Game_" + _game.UUID + "_StandardOutput"
        }.Start();
        new Thread(() =>
        {
            using var reader = new StreamReader(Process.StandardError.BaseStream, encoding);
            while (!_stop)
            {
                var output = reader.ReadLine();
                ColorMCCore.OnGameLog(_game, output);
            }
        })
        {
            Name = "ColorMC_Game_" + _game.UUID + "_StandardError"
        }.Start();
    }

    public void Kill()
    {
        Process.Kill();
    }
}