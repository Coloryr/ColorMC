using System.Diagnostics;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using Microsoft.Win32;

namespace ColorMC.Core.Helpers;

/// <summary>
/// Java处理
/// </summary>
public static class JavaHelper
{
    /// <summary>
    /// 提升权限
    /// </summary>
    /// <param name="path">文件</param>
    public static void Per(string path)
    {
        try
        {
            Process p = new();
            p.StartInfo.FileName = "sh";
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.Start();

            var info = new FileInfo(path);
            p.StandardInput.WriteLine("chmod a+x " + info.Directory!.FullName + "/*");
            string path1 = info.Directory!.Parent!.FullName + "/lib/*";
            p.StandardInput.WriteLine("chmod a+x " + path1);

            p.StandardInput.WriteLine("exit");
            p.WaitForExit();

            string temp = p.StandardOutput.ReadToEnd();

            p.Dispose();
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Jvm.Error9"), e);
        }
    }

    /// <summary>
    /// 获取主版本
    /// </summary>
    /// <param name="version">版本号</param>
    /// <returns>主版本</returns>
    private static int GetMajorVersion(string version)
    {
        string[] vers = version.Trim().Split('.', '_', '-', '+', 'u', 'U');
        if (vers[0] == "1")
        {
            if (int.TryParse(vers[1], out var data))
            {
                return data;
            }

            return 0;
        }
        else
        {
            if (int.TryParse(vers[0], out var data))
            {
                return data;
            }

            return 0;
        }
    }

    /// <summary>
    /// 获取Java信息
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns>Java信息</returns>
    public static JavaInfo? GetJavaInfo(string path)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                Process? p;

                if (SystemInfo.Os == OsType.Android)
                {
                    p = ColorMCCore.PhoneStartJvm(path);
                }
                else
                {
                    p = new Process();
                    p.StartInfo.FileName = path;
                }
                if (p == null)
                {
                    return null;
                }

                var info1 = new FileInfo(path);

                p.StartInfo.ArgumentList.Add("-version");
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WorkingDirectory = info1.Directory?.Parent?.FullName ?? ColorMCCore.BaseDir;
                p.Start();
                p.WaitForExit();
                var result = p.StandardError.ReadToEnd();
                var result1 = p.StandardOutput.ReadToEnd();
                var lines = result.Split('\n');

                var select = lines[0];

                foreach (var item in lines)
                {
                    var firstL = item.Trim().Split(' ');
                    if (firstL[1] == "version")
                    {
                        var type = firstL[0];
                        var version = firstL[2].Trim('\"');
                        var is64 = result.Contains("64-Bit");
                        var arch = SystemInfo.IsArm
                            ? (is64 ? ArchEnum.aarch64 : ArchEnum.arm)
                            : (is64 ? ArchEnum.x86_64 : ArchEnum.x86);
                        var info = new JavaInfo()
                        {
                            Path = path,
                            Version = version,
                            Arch = arch,
                            Type = type,
                            MajorVersion = GetMajorVersion(version)
                        };
                        return info;
                    }
                }
            }

            return null;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Jvm.Error10"), e);
            return null;
        }
    }

#pragma warning disable CA1416 // 验证平台兼容性

    /// <summary>
    /// 获取所有Java版本
    /// </summary>
    /// <param name="javaKeyPath">注册表路径</param>
    /// <returns>Java版本</returns>
    private static List<string> GetOracleJavaInstallPath(string javaKeyPath)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(javaKeyPath);
            if (key == null)
            {
                return [];
            }

            var path = new List<string>();
            foreach (var item in key.GetSubKeyNames())
            {
                using var key1 = key!.OpenSubKey(item);
                if (key != null && key1?.GetValue("JavaHome")?.ToString() is { } home)
                {
                    path.Add(home);
                }
            }

            return path;
        }
        catch
        {

        }
        return [];
    }

    /// <summary>
    /// 获取所有Java版本
    /// </summary>
    /// <param name="javaKeyPath">注册表路径</param>
    /// <returns>Java版本</returns>
    private static List<string> GetAdoptiumJavaInstallPath(string javaKeyPath)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(javaKeyPath);
            if (key == null)
            {
                return [];
            }

            var path = new List<string>();
            foreach (var item in key.GetSubKeyNames())
            {
                using var key1 = key!.OpenSubKey(item + @"\hotspot\MSI");
                if (key != null && key1?.GetValue("Path")?.ToString() is { } home)
                {
                    path.Add(home);
                }
            }

            return path;
        }
        catch
        {

        }
        return [];
    }

    /// <summary>
    /// 获取所有Java版本
    /// </summary>
    /// <param name="javaKeyPath">注册表路径</param>
    /// <returns>Java版本</returns>
    private static List<string> GetZuluJavaInstallPath(string javaKeyPath)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(javaKeyPath);
            if (key == null)
            {
                return [];
            }

            var path = new List<string>();
            foreach (var item in key.GetSubKeyNames())
            {
                using var key1 = key!.OpenSubKey(item);
                if (key != null && key1?.GetValue("InstallationPath")?.ToString() is { } home)
                {
                    path.Add(home);
                }
            }

            return path;
        }
        catch
        {

        }
        return [];
    }
#pragma warning restore CA1416

    /// <summary>
    /// 获取所有Java版本
    /// </summary>
    /// <param name="local">文件路径</param>
    /// <returns>Java版本</returns>
    public static List<JavaInfo>? FindJava(string local)
    {
        try
        {
            var list = new List<JavaInfo>();
            var list1 = PathHelper.GetAllFile(local)
                .Where(item => item.Name == "javaw.exe" || item.Name == "java.exe");
            foreach (var item in list1)
            {
                var info = GetJavaInfo(item.FullName);
                if (info != null)
                {
                    list.Add(info);
                }
            }

            return list;
        }
        catch (Exception e)
        {
            Logs.Error("error on find java", e);
            return null;
        }
    }

    /// <summary>
    /// 查找本机所有Java
    /// </summary>
    /// <returns>Java列表</returns>
    public static async Task<List<JavaInfo>?> FindJava()
    {
        bool ex = false;
        var list = new List<JavaInfo>();

        await Task.Run(() =>
        {
            try
            {
                //简单扫描
                var list1 = SystemInfo.Os switch
                {
                    OsType.Windows => GetList("where", "javaw"),
                    _ => GetList("which", "java"),
                };

                foreach (var item in list1)
                {
                    if (string.IsNullOrWhiteSpace(item))
                    {
                        continue;
                    }
                    var info = GetJavaInfo(item);
                    if (info != null)
                    {
                        list.Add(info);
                    }
                }

                //扫描注册表
                if (SystemInfo.Os == OsType.Windows)
                {
                    foreach (var item in GetOracleJavaInstallPath(@"SOFTWARE\JavaSoft\Java Runtime Environment\"))
                    {
                        var info = GetJavaInfo(Path.GetFullPath(item + @"\bin\javaw.exe"));
                        if (info != null)
                        {
                            list.Add(info);
                        }
                    }
                    foreach (var item in GetOracleJavaInstallPath(@"SOFTWARE\JavaSoft\JDK\"))
                    {
                        var info = GetJavaInfo(Path.GetFullPath(item + @"\bin\javaw.exe"));
                        if (info != null)
                        {
                            list.Add(info);
                        }
                    }
                    foreach (var item in GetAdoptiumJavaInstallPath(@"SOFTWARE\Eclipse Adoptium\JDK\"))
                    {
                        var info = GetJavaInfo(Path.GetFullPath(item + @"\bin\javaw.exe"));
                        if (info != null)
                        {
                            list.Add(info);
                        }
                    }
                    foreach (var item in GetAdoptiumJavaInstallPath(@"SOFTWARE\Eclipse Adoptium\JDK\"))
                    {
                        var info = GetJavaInfo(Path.GetFullPath(item + @"\bin\javaw.exe"));
                        if (info != null)
                        {
                            list.Add(info);
                        }
                    }
                    foreach (var item in GetZuluJavaInstallPath(@"SOFTWARE\Azul Systems\Zulu\"))
                    {
                        var info = GetJavaInfo(Path.GetFullPath(item + @"\bin\javaw.exe"));
                        if (info != null)
                        {
                            list.Add(info);
                        }
                    }
                }
                else if (SystemInfo.Os == OsType.Linux)
                {
                    if (SystemInfo.System.Contains("Ubuntu") ||
                        SystemInfo.System.Contains("Debian"))
                    {
                        list1 = GetList("update-alternatives", "--list java");

                        foreach (var item in list1)
                        {
                            if (string.IsNullOrWhiteSpace(item))
                            {
                                continue;
                            }
                            var info = GetJavaInfo(item);
                            if (info != null)
                            {
                                list.Add(info);
                            }
                        }
                    }
                    else if (SystemInfo.SystemName.Contains("Arch")
                        || SystemInfo.SystemName.Contains("Manjaro"))
                    {
                        list1 = GetList("sh", "-c \"pacman -Qs jre jdk | grep jdk\"");

                        foreach (var item in list1)
                        {
                            if (string.IsNullOrWhiteSpace(item))
                            {
                                continue;
                            }
                            var list2 = item.Split(' ');
                            var list3 = GetList("pacman", "-Ql " + list2[0]);
                            var item1 = list3.Where(item => item.EndsWith("/java"))
                                .FirstOrDefault();
                            if (item1 == null)
                            {
                                continue;
                            }
                            var list4 = item1.Split(' ');
                            if (list4.Length < 2)
                            {
                                continue;
                            }
                            var info = GetJavaInfo(list4[1]);
                            if (info != null)
                            {
                                list.Add(info);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.Get("Core.Jvm.Error10"), e);
                ex = true;
            }
        });

        return ex ? null : list;
    }

    /// <summary>
    /// 执行并获取控制台数据
    /// </summary>
    /// <param name="file">文件</param>
    /// <param name="arg">参数</param>
    /// <returns>数据</returns>
    private static string[] GetList(string file, string arg)
    {
        using var p = new Process();
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.WorkingDirectory = ColorMCCore.BaseDir;
        p.StartInfo.FileName = file;
        p.StartInfo.Arguments = arg;
        p.Start();
        p.WaitForExit();

        var result = p.StandardOutput.ReadToEnd();
        var result1 = p.StandardError.ReadToEnd();
        return result.Split([Environment.NewLine], StringSplitOptions.None);
    }
}
