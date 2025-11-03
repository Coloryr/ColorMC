using System.Diagnostics;
using System.Text;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Game;

/// <summary>
/// 游戏句柄
/// </summary>
public class GameHandel
{
    /// <summary>
    /// 游戏进程
    /// </summary>
    public Process Process { get; init; }
    /// <summary>
    /// 游戏实例UUID
    /// </summary>
    public string UUID => _game.UUID;
    /// <summary>
    /// 进程是否已经退出
    /// </summary>
    public bool IsExit { get; private set; }
    /// <summary>
    /// 游戏实例主窗口句柄
    /// </summary>
    public IntPtr Handel => Process.MainWindowHandle;
    /// <summary>
    /// 是否为管理员启动，且无法获取句柄
    /// </summary>
    public bool IsOutAdmin { get; private set; }

    private readonly GameSettingObj _game;

    /// <summary>
    /// 游戏句柄
    /// </summary>
    /// <param name="run">游戏运行参数</param>
    public GameHandel(GameRunObj run)
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
            IsExit = true;
            ColorMCCore.OnGameExit(_game, run.Auth, Process.ExitCode);
            Process.Dispose();
        };

        if (ProcessUtils.Launch(Process, run.Admin))
        {
            IsOutAdmin = true;
            ColorMCCore.OnGameLog(_game, GameSystemLog.JavaRedirect);
            return;
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
            while (!IsExit)
            {
                var output = reader.ReadLine();
                ColorMCCore.OnGameLog(_game, output);
            }
        })
        {
            Name = "ColorMC Game " + _game.UUID + " StandardOutput",
            IsBackground = true
        }.Start();
        new Thread(() =>
        {
            using var reader = new StreamReader(Process.StandardError.BaseStream, encoding);
            while (!IsExit)
            {
                var output = reader.ReadLine();
                ColorMCCore.OnGameLog(_game, output);
            }
        })
        {
            Name = "ColorMC Game " + _game.UUID + " StandardError",
            IsBackground = true
        }.Start();
    }

    public void Kill()
    {
        Process.Kill();
    }
}