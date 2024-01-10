using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using System.Diagnostics;

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
        if (version == JvmPath.Unknow)
        {
            return -1;
        }
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
                    p = ColorMCCore.PhoneStartJvm?.Invoke(path);
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

                p.StartInfo.Arguments = "-version";
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WorkingDirectory = ColorMCCore.BaseDir;
                p.Start();
                string result = p.StandardError.ReadToEnd();
                string[] lines = result.Split('\n');

                string select = lines[0];

                foreach (var item in lines)
                {
                    string[] firstL = item.Trim().Split(' ');
                    if (firstL[1] == "version")
                    {
                        string type = firstL[0];
                        string version = firstL[2].Trim('\"');
                        bool is64 = result.Contains("64-Bit");
                        ArchEnum arch = is64 ? ArchEnum.x86_64 : ArchEnum.x86;
                        JavaInfo info = new()
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

    /// <summary>
    /// 查找本机所有Java
    /// </summary>
    /// <returns></returns>
    public static List<JavaInfo>? FindJava()
    {
        try
        {
            var list = new List<JavaInfo>();
            using var p = new Process();
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.WorkingDirectory = ColorMCCore.BaseDir;
            if (SystemInfo.Os == OsType.Windows)
            {
                p.StartInfo.FileName = "where";
                p.StartInfo.Arguments = "javaw";
            }
            else if (SystemInfo.Os == OsType.Linux)
            {
                p.StartInfo.FileName = "which";
                p.StartInfo.Arguments = "java";
            }
            else if (SystemInfo.Os == OsType.MacOS)
            {
                p.StartInfo.FileName = "which";
                p.StartInfo.Arguments = "java";
            }
            p.Start();

            string result = p.StandardOutput.ReadToEnd();
            var list1 = result.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            foreach (var item in list1)
            {
                var info = GetJavaInfo(item);
                if (info != null)
                {
                    list.Add(info);
                }
            }

            return list;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Jvm.Error10"), e);
            return null;
        }
    }
}
