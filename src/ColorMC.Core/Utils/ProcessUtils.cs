using System.Diagnostics;
using System.Security.Principal;
using ColorMC.Core.Objs;

namespace ColorMC.Core.Utils;

public static class ProcessUtils
{
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
            if (Environment.UserName.Equals("root", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // 检查当前用户是否属于 sudo 组
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "groups",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output.Contains("sudo");
        }
    }

    public static bool Launch(Process process, bool admin)
    {
        if (admin && !IsRunAsAdmin())
        {
            process.StartInfo.UseShellExecute = true;
            if (SystemInfo.Os == OsType.Windows)
            {
                process.StartInfo.Verb = "runas";
            }
            else if (SystemInfo.Os == OsType.MacOS)
            {
                var list = new List<string>(process.StartInfo.ArgumentList);
                list.Insert(0, process.StartInfo.FileName);
                process.StartInfo.FileName = "sudo";
            }
            else
            {
                var list = new List<string>(process.StartInfo.ArgumentList);
                list.Insert(0, process.StartInfo.FileName);
                process.StartInfo.FileName = "pkexec";
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
        else if (SystemInfo.Os == OsType.MacOS)
        {
            p.StartInfo.ArgumentList.Add(file);
            p.StartInfo.FileName = "sudo";
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
    }
}
