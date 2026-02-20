using System.Diagnostics;
using System.Security.Principal;
using ColorMC.Core.Objs;

namespace ColorMC.Core.Utils;

/// <summary>
/// 进程相关函数
/// </summary>
public static class ProcessUtils
{
    /// <summary>
    /// 是否是管理员启动
    /// </summary>
    /// <returns></returns>
    public static bool IsRunAsAdmin()
    {
        if (SystemInfo.Os == OsType.Windows)
        {
#pragma warning disable CA1416 // 验证平台兼容性
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
#pragma warning restore CA1416 // 验证平台兼容性
        }
        else
        {
            // 检查当前用户是否是 root
            return Environment.UserName.Equals("root", StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// 启动进程
    /// </summary>
    /// <param name="process">进程</param>
    /// <param name="admin">是否是管理员启动</param>
    public static bool Launch(Process process, bool admin)
    {
        if (admin && !IsRunAsAdmin())
        {
            process.StartInfo.UseShellExecute = true;
            switch (SystemInfo.Os)
            {
                case OsType.Windows:
                    process.StartInfo.Verb = "runas";
                    break;
                case OsType.MacOs:
                {
                    process.StartInfo.ArgumentList.Insert(0, process.StartInfo.FileName);
                    process.StartInfo.FileName = "sudo";
                    break;
                }
                case OsType.Linux:
                {
                    process.StartInfo.ArgumentList.Insert(0, process.StartInfo.FileName);
                    process.StartInfo.FileName = "pkexec";
                    break;
                }
            }

            process.StartInfo.RedirectStandardInput = false;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.RedirectStandardError = false;

            process.Start();
            return true;
        }

        process.Start();
        return false;
    }

    /// <summary>
    /// 以管理员方式启动ColorMC
    /// </summary>
    /// <param name="args">启动参数</param>
    public static void LaunchAdmin(string[] args)
    {
        var file = Environment.ProcessPath!;
        var p = new Process();

        if (SystemInfo.Os == OsType.Windows)
        {
            p.StartInfo.Verb = "runas";
            p.StartInfo.FileName = file;
            p.StartInfo.UseShellExecute = true;
        }
        else if (SystemInfo.Os == OsType.MacOs)
        {
            p.StartInfo.ArgumentList.Add("-e");
            p.StartInfo.ArgumentList.Add($"do shell script \"{file}\" with administrator privileges");
            p.StartInfo.FileName = "osascript";
            p.StartInfo.UseShellExecute = true;
        }
        else
        {
            p.StartInfo.ArgumentList.Add(file);
            p.StartInfo.FileName = "pkexec";
            p.StartInfo.UseShellExecute = true;
        }
        foreach (var item in args)
        {
            p.StartInfo.ArgumentList.Add(item);
        }

        p.Start();
        p.WaitForExit();
    }
}
