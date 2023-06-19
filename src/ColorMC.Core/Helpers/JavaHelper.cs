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

            p.StandardInput.WriteLine("chmod -R 777 " + path);
            p.StandardInput.WriteLine("exit");
            p.WaitForExit();

            string temp = p.StandardOutput.ReadToEnd();

            p.Dispose();
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.GetName("Core.Jvm.Error9"), e);
        }
    }

    /// <summary>
    /// 获取Java信息
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns>信息</returns>
    public static JavaInfo? GetJavaInfo(string path)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                using var p = new Process();
                p.StartInfo.FileName = path;
                p.StartInfo.Arguments = "-version";
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WorkingDirectory = ColorMCCore.BaseDir;
                p.Start();
                string result = p.StandardError.ReadToEnd();
                string[] lines = result.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                string[] firstL = lines[0].Split(' ');
                string type = firstL[0];
                string version = firstL[2].Trim('\"');
                bool is64 = result.Contains("64-Bit");
                ArchEnum arch = is64 ? ArchEnum.x64 : ArchEnum.x32;
                JavaInfo info = new()
                {
                    Path = path,
                    Version = version,
                    Arch = arch,
                    Type = type
                };
                return info;
            }
            else
            {
                return null;
            }
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.GetName("Core.Jvm.Error10"), e);
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
            Logs.Error(LanguageHelper.GetName("Core.Jvm.Error10"), e);
            return null;
        }
    }
}
