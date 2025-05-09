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
    /// 将文件夹挪到回收站
    /// </summary>
    /// <param name="dir"></param>
    public static Task<bool> MoveToTrash(string dir)
    {
        return Task.Run(() =>
        {
            try
            {
                if (SystemInfo.Os == OsType.Windows)
                {
                    return Win32.MoveToTrash(dir);
                }
                else if (SystemInfo.Os == OsType.Linux)
                {
                    // 获取用户的主目录
                    string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    // 定义垃圾桶路径
                    string trashPath = Path.Combine(home, ".local", "share", "Trash", "files");

                    if (!Directory.Exists(trashPath))
                    {
                        Directory.CreateDirectory(trashPath);
                    }

                    // 生成唯一的文件名以避免冲突
                    string fileName = Path.GetFileName(dir);
                    string destPath = Path.Combine(trashPath, fileName);
                    int count = 1;
                    while (File.Exists(destPath) || Directory.Exists(destPath))
                    {
                        destPath = Path.Combine(trashPath, $"{fileName}_{count}");
                        count++;
                    }

                    // 移动文件或文件夹到垃圾桶
                    Directory.Move(dir, destPath);

                    return true;
                }
                else if (SystemInfo.Os == OsType.MacOS)
                {
                    // AppleScript命令
                    string appleScriptCommand = $"tell application \"Finder\" to delete POSIX file \"{dir}\"";

                    // 使用bash执行AppleScript
                    var processInfo = new ProcessStartInfo("bash", $"-c \"osascript -e '{appleScriptCommand}'\"")
                    {
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
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

    /// <summary>
    /// 检查路径非法名字
    /// </summary>
    /// <param name="name">名字</param>
    /// <returns>是否合理</returns>
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
        using var stream1 = OpenWrite(output, true);
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
    /// 删除文件夹
    /// </summary>
    /// <param name="arg">参数</param>
    /// <returns>是否成功删除</returns>
    public static async Task<bool> DeleteFilesAsync(DeleteFilesArg arg)
    {
        if (!Directory.Exists(arg.Local))
        {
            return true;
        }

        if (arg.Request != null)
        {
            var res = await arg.Request(string.Format(LanguageHelper.Get("Core.Info2"), arg.Local));
            if (!res)
            {
                return false;
            }
        }

        return await Task.Run(() =>
        {
            try
            {
                Directory.Delete(arg.Local, true);

                return true;
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.Get("Core.Game.Error10"), e);
                return false;
            }
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
#if Phone
        if (SystemInfo.Os == OsType.Android && local.StartsWith("content://"))
        {
            return ColorMCCore.PhoneReadFile(local);
        }
#else
        local = Path.GetFullPath(local);
        if (File.Exists(local))
        {
            return File.Open(local, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }
#endif
        return null;
    }

    /// <summary>
    /// 写文件
    /// </summary>
    /// <param name="local">路径</param>
    /// <param name="create">是否创建</param>
    /// <returns>流</returns>
    public static Stream OpenWrite(string local, bool create)
    {
        local = Path.GetFullPath(local);
        var info = new FileInfo(local);
        info.Directory?.Create();
        return File.Open(local, create ? FileMode.Create : FileMode.OpenOrCreate,
            FileAccess.ReadWrite, FileShare.ReadWrite);
    }

    /// <summary>
    /// 写文本
    /// </summary>
    /// <param name="local">路径</param>
    /// <param name="str">数据</param>
    public static void WriteText(string local, string str)
    {
        var data = Encoding.UTF8.GetBytes(str);
        WriteBytes(local, data);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="local"></param>
    /// <param name="str"></param>
    public static async Task WriteTextAsync(string local, string str)
    {
        var data = Encoding.UTF8.GetBytes(str);
        await WriteBytesAsync(local, data);
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
        using var stream1 = new MemoryStream();
        stream.CopyTo(stream1);
        return Encoding.UTF8.GetString(stream1.ToArray());
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
#if Phone
        if (SystemInfo.Os == OsType.Android && local.StartsWith("content://"))
        {
            return;
        }
#endif
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
        using var stream = OpenWrite(local, true);
        stream.Write(data, 0, data.Length);
    }

    /// <summary>
    /// 写文件
    /// </summary>
    /// <param name="local">路径</param>
    /// <param name="data">数据</param>
    public static void WriteBytes(string local, Stream data)
    {
        using var stream = OpenWrite(local, true);
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
        using var stream = OpenWrite(local, true);
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
        using var stream = OpenWrite(local, true);
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