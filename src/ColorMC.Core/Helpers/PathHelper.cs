using System.Diagnostics;
using System.Text;
using ColorMC.Core.Hook;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Helpers;

/// <summary>
/// 文件与路径处理
/// </summary>
public static class PathHelper
{
    /// <summary>
    /// 提升权限
    /// </summary>
    /// <param name="path">文件</param>
    public static void Chmod(string path)
    {
        using var p = new Process();
        p.StartInfo.FileName = "sh";
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.CreateNoWindow = true;
        p.Start();

        p.StandardInput.WriteLine("chmod a+x " + path);

        p.StandardInput.WriteLine("exit");
        p.WaitForExit();
    }

    /// <summary>
    /// 提升Java文件夹权限
    /// </summary>
    /// <param name="path">文件</param>
    public static void PerJavaChmod(string path)
    {
        using var p = new Process();
        p.StartInfo.FileName = "sh";
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.CreateNoWindow = true;
        p.Start();

        var info = new FileInfo(path);
        p.StandardInput.WriteLine("chmod a+x " + info.Directory!.FullName + "/*");
        p.StandardInput.WriteLine("chmod a+x " + info.Directory!.Parent!.FullName + "/lib/*");
        p.StandardInput.WriteLine("exit");
        p.WaitForExit();

        string temp = p.StandardOutput.ReadToEnd();

        p.Dispose();
    }

    /// <summary>
    /// 获取回收站路径
    /// </summary>
    /// <returns></returns>
    private static string GetTrashFilesPath()
    {
        string dataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME") ??
                          Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share");
        string trashPath = Path.Combine(dataHome, "Trash", "files");
        Directory.CreateDirectory(trashPath); // 确保目录存在
        return trashPath;
    }

    /// <summary>
    /// 获取回收站路径
    /// </summary>
    /// <returns></returns>
    private static string GetTrashInfoPath()
    {
        string dataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME") ??
                          Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share");
        string trashInfoPath = Path.Combine(dataHome, "Trash", "info");
        Directory.CreateDirectory(trashInfoPath); // 确保目录存在
        return trashInfoPath;
    }

    /// <summary>
    /// 生成回收站内容
    /// </summary>
    /// <returns></returns>
    private static string GenerateTrashInfoContent(string originalPath, DateTime deletionDate)
    {
        return $"[Trash Info]\n" +
               $"Path={originalPath}\n" +
               $"DeletionDate={deletionDate:yyyy-MM-ddTHH:mm:ss}\n";
    }

    /// <summary>
    /// 将文件夹挪到回收站
    /// </summary>
    /// <param name="dir"></param>
    public static Task<bool> MoveToTrashAsync(string dir)
    {
        return Task.Run(() =>
        {
            try
            {
                switch (SystemInfo.Os)
                {
                    case OsType.Windows:
                        return Win32.MoveToTrash(dir);
                    case OsType.Linux:
                    {
                        string trashFilesPath = GetTrashFilesPath();
                        string trashInfoPath = GetTrashInfoPath();

                        string fileName = Path.GetFileName(dir);
                        string destPath = Path.Combine(trashFilesPath, fileName);
                        string trashInfoFile = Path.Combine(trashInfoPath, fileName + ".trashinfo");

                        // 处理文件名冲突
                        int counter = 1;
                        while (File.Exists(destPath) || Directory.Exists(destPath))
                        {
                            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                            string ext = Path.GetExtension(fileName);
                            destPath = Path.Combine(trashFilesPath, $"{nameWithoutExt}_{counter}{ext}");
                            trashInfoFile = Path.Combine(trashInfoPath, $"{nameWithoutExt}_{counter}{ext}.trashinfo");
                            counter++;
                        }

                        // 创建 .trashinfo 文件
                        string trashInfoContent = GenerateTrashInfoContent(dir, DateTime.Now);
                        File.WriteAllText(trashInfoFile, trashInfoContent);

                        if (File.Exists(dir))
                        {
                            File.Move(dir, destPath);
                        }
                        else if (Directory.Exists(dir))
                        {
                            Directory.Move(dir, destPath);
                        }

                        return true;
                    }
                    case OsType.MacOs:
                    {
                        dir = dir.Replace("\\", @"\\")
                            .Replace("\"", "\\\"")
                            .Replace("\n", "\\n")
                            .Replace("\r", "\\r")
                            .Replace("\t", "\\t");
                    
                        // AppleScript命令
                        string appleScriptCommand = $"tell application \"Finder\" to delete POSIX file \"{dir}\"";

                        // 使用bash执行AppleScript
                        var processInfo = new ProcessStartInfo("osascript", ["-e", appleScriptCommand])
                        {
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        };

                        using var process = new Process { StartInfo = processInfo };
                        process.Start();
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        process.WaitForExit();

                        return process.ExitCode == 0;
                    }
                }
            }
            catch
            {

            }

            return false;
        });
    }

    /// <summary>
    /// 检查非法名字
    /// </summary>
    /// <param name="name">名字</param>
    /// <returns>是否合理</returns>
    public static bool FileHasInvalidChars(string name)
    {
        return string.IsNullOrWhiteSpace(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || (name.All('.'.Equals) && name.StartsWith('.') && name.EndsWith('.')) || name.Length > 80;
    }

    ///// <summary>
    ///// 检查路径非法名字
    ///// </summary>
    ///// <param name="name">名字</param>
    ///// <returns>是否合理</returns>
    //public static bool PathHasInvalidChars(string name)
    //{
    //    return string.IsNullOrWhiteSpace(name) || name.IndexOfAny(Path.GetInvalidPathChars()) >= 0
    //        || name.All('.'.Equals) || name.Length > 50;
    //}

    /// <summary>
    /// 获取所有文件
    /// </summary>
    /// <param name="local">路径</param>
    /// <returns>文件列表</returns>
    public static List<FileInfo> GetAllFiles(string local)
    {
        var list = new List<FileInfo>();
        var info = new DirectoryInfo(local);
        if (!info.Exists)
        {
            return list;
        }

        list.AddRange(info.GetFiles());
        foreach (var item in info.GetDirectories())
        {
            list.AddRange(GetAllFiles(item.FullName));
        }

        return list;
    }

    /// <summary>
    /// 获取当前目录所有文件
    /// </summary>
    /// <param name="local">路径</param>
    /// <returns>文件列表</returns>
    public static List<FileInfo> GetFiles(string local)
    {
        var list = new List<FileInfo>();
        var info = new DirectoryInfo(local);
        if (!info.Exists)
        {
            return list;
        }

        list.AddRange(info.GetFiles());
        return list;
    }

    /// <summary>
    /// 获取目录占用大小
    /// </summary>
    /// <param name="folderPath">目录</param>
    /// <returns></returns>
    public static long GetFolderSize(string folderPath)
    {
        var dirInfo = new DirectoryInfo(folderPath);
        if (!dirInfo.Exists)
        {
            return 0;
        }

        long size = 0;

        foreach (FileInfo file in dirInfo.EnumerateFiles())
        {
            try
            {
                size += file.Length;
            }
            catch (UnauthorizedAccessException) { }
        }

        foreach (DirectoryInfo dir in dirInfo.EnumerateDirectories())
        {
            try
            {
                size += GetFolderSize(dir.FullName);
            }
            catch (UnauthorizedAccessException) { }
        }

        return size;
    }

    /// <summary>
    /// 获取当前目录所有目录
    /// </summary>
    /// <param name="local">路径</param>
    /// <returns>目录列表</returns>
    public static List<DirectoryInfo> GetDirs(string local)
    {
        var list = new List<DirectoryInfo>();
        var info = new DirectoryInfo(local);
        if (!info.Exists)
        {
            return list;
        }

        list.AddRange(info.GetDirectories());
        return list;
    }

    /// <summary>
    /// 复制文件夹
    /// </summary>
    /// <param name="dir">输入路径</param>
    /// <param name="dir1">输出路径</param>
    private static void Copys(string dir, string dir1)
    {
        Directory.CreateDirectory(dir1);
        var files = Directory.GetFileSystemEntries(dir);

        foreach (string file in files)
        {
            if (Directory.Exists(file))
            {
                var floderName = Path.GetFileName(file);
                Copys(file, Path.GetFullPath(dir1 + "/" + floderName));
            }
            else
            {
                CopyFile(file, Path.GetFullPath(dir1 + "/" + Path.GetFileName(file)));
            }
        }
    }

    /// <summary>
    /// 复制文件
    /// </summary>
    /// <param name="input">输入文件</param>
    /// <param name="output">输出文件</param>
    public static void CopyFile(string input, string output)
    {
        using var stream = OpenRead(input);
        using var stream1 = OpenWrite(output);
        if (stream == null)
        {
            return;
        }
        stream.CopyTo(stream1);
    }

    /// <summary>
    /// 搬运文件
    /// </summary>
    /// <param name="input">输入文件</param>
    /// <param name="output">输出文件</param>
    public static void MoveFile(string input, string output)
    {
        CopyFile(input, output);
        Delete(input);
    }

    /// <summary>
    /// 复制文件夹
    /// </summary>
    /// <param name="dir">输入路径</param>
    /// <param name="dir1">输出路径</param>
    /// <returns></returns>
    public static Task CopyDirAsync(string dir, string dir1)
    {
        return Task.Run(() =>
        {
            Copys(dir, dir1);
        });
    }

    /// <summary>
    /// 查找文件
    /// </summary>
    /// <param name="local">路径</param>
    /// <param name="name">文件名</param>
    /// <returns>完整路径</returns>
    public static string? GetFile(string local, string name)
    {
        var list = GetAllFiles(local);
        foreach (var item in list)
        {
            if (item.Name == name)
                return item.FullName;
        }

        return null;
    }

    /// <summary>
    /// 读文件
    /// </summary>
    /// <param name="local">路径</param>
    /// <returns>流</returns>
    public static Stream? OpenRead(string local)
    {
        local = Path.GetFullPath(local);
        return File.Exists(local) ? File.Open(local, FileMode.Open, FileAccess.Read, FileShare.ReadWrite) : null;
    }

    /// <summary>
    /// 写文件
    /// </summary>
    /// <param name="local">路径</param>
    /// <returns>流</returns>
    public static Stream OpenWrite(string local)
    {
        local = Path.GetFullPath(local);
        var info = new FileInfo(local);
        info.Directory?.Create();
        return File.Open(local, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
    }

    /// <summary>
    /// 继续写文件
    /// </summary>
    /// <param name="local">路径</param>
    /// <returns>流</returns>
    public static Stream OpenAppend(string local)
    {
        local = Path.GetFullPath(local);
        var info = new FileInfo(local);
        info.Directory?.Create();
        var stream = File.Open(local, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        stream.Seek(0, SeekOrigin.End);
        return stream;
    }

    /// <summary>
    /// 写文本
    /// </summary>
    /// <param name="local">路径</param>
    /// <param name="str">文本</param>
    public static void WriteText(string local, string? str)
    {
        local = Path.GetFullPath(local);
        File.WriteAllText(local, str);
    }

    /// <summary>
    /// 异步写文本
    /// </summary>
    /// <param name="local">路径</param>
    /// <param name="str">文本</param>
    public static async Task WriteTextAsync(string local, string? str)
    {
        local = Path.GetFullPath(local);
        await File.WriteAllTextAsync(local, str);
    }

    /// <summary>
    /// 读文本
    /// </summary>
    /// <param name="local">路径</param>
    /// <returns>文本</returns>
    public static string? ReadText(string local)
    {
        using var stream = OpenRead(local);
        if (stream == null)
        {
            return null;
        }
        using var stream1 = new StreamReader(stream, Encoding.UTF8);
        return stream1.ReadToEnd();
    }

    /// <summary>
    /// 读文本
    /// </summary>
    /// <param name="stream">流</param>
    /// <returns>文字</returns>
    public static string? ReadText(Stream stream)
    {
        using var stream1 = new MemoryStream();
        stream.CopyTo(stream1);
        return Encoding.UTF8.GetString(stream1.ToArray());
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="local">路径</param>
    public static void Delete(string local)
    {
        if (!File.Exists(local))
        {
            return;
        }
        File.Delete(local);
    }

    /// <summary>
    /// 写文件
    /// </summary>
    /// <param name="local">路径</param>
    /// <param name="data">数据</param>
    public static void WriteBytes(string local, byte[] data)
    {
        using var stream = OpenWrite(local);
        stream.Write(data, 0, data.Length);
    }

    /// <summary>
    /// 写文件
    /// </summary>
    /// <param name="local">路径</param>
    /// <param name="data">数据</param>
    public static void WriteBytes(string local, Stream data)
    {
        using var stream = OpenWrite(local);
        data.CopyTo(stream);
    }

    /// <summary>
    /// 写文件
    /// </summary>
    /// <param name="local"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static async Task WriteBytesAsync(string local, Stream data)
    {
        using var stream = OpenWrite(local);
        await data.CopyToAsync(stream);
    }

    /// <summary>
    /// 写文件
    /// </summary>
    /// <param name="local"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public static async Task WriteBytesAsync(string local, byte[] data)
    {
        using var stream = OpenWrite(local);
        await stream.WriteAsync(data);
    }

    /// <summary>
    /// 替换文件名非法字符
    /// </summary>
    /// <param name="name">输入名字</param>
    /// <returns>替换结果</returns>
    public static string ReplaceFileName(string? name)
    {
        if (name == null)
        {
            return "";
        }
        var chars = Path.GetInvalidFileNameChars().ToList();
        var builder = new StringBuilder();
        foreach (var item in name)
        {
            if (chars.Contains(item) || item == 0x3F)
            {
                builder.Append('_');
            }
            else
            {
                builder.Append(item);
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// 替换文件名非法字符
    /// </summary>
    /// <param name="name">输入名字</param>
    /// <returns>替换结果</returns>
    public static string ReplacePathName(string? name)
    {
        if (name == null)
        {
            return "";
        }
        var chars = Path.GetInvalidPathChars().ToList();
        var builder = new StringBuilder();
        foreach (var item in name)
        {
            if (chars.Contains(item) || item == 0x3F)
            {
                builder.Append('_');
            }
            else
            {
                builder.Append(item);
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// 读取byte数据
    /// </summary>
    /// <param name="local">文件路径</param>
    /// <returns>数据</returns>
    public static byte[]? ReadByte(string local)
    {
        using var stream = OpenRead(local);
        if (stream == null)
        {
            return null;
        }
        using var stream1 = new MemoryStream();
        stream.CopyTo(stream1);
        return stream1.ToArray();
    }
}