using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using System.Text;

namespace ColorMC.Core.Helpers;

/// <summary>
/// 文件与路径处理
/// </summary>
public static class PathHelper
{
    /// <summary>
    /// 检查非法名字
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool FileHasInvalidChars(string name)
    {
        return string.IsNullOrWhiteSpace(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || name.All('.'.Equals) || name.Length > 50;
    }

    /// <summary>
    /// 检查路径非法名字
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool PathHasInvalidChars(string name)
    {
        return string.IsNullOrWhiteSpace(name) || name.IndexOfAny(Path.GetInvalidPathChars()) >= 0
            || name.All('.'.Equals) || name.Length > 50;
    }

    /// <summary>
    /// 获取名字
    /// </summary>
    public static (string Path, string Name) ToPathAndName(string input)
    {
        var arg = input.Split(':');
        var arg1 = arg[0].Split('.');
        string path = "";
        for (int a = 0; a < arg1.Length; a++)
        {
            path += arg1[a] + '/';
        }
        string name;
        if (arg.Length > 3)
        {
            path += $"{arg[1]}/{arg[2]}/{arg[1]}-{arg[2]}-{arg[3]}.jar";
            name = $"{arg[1]}-{arg[2]}-{arg[3]}.jar";
        }
        else
        {
            path += $"{arg[1]}/{arg[2]}/{arg[1]}-{arg[2]}.jar";
            name = $"{arg[1]}-{arg[2]}.jar";
        }

        return (path, name);
    }

    /// <summary>
    /// 获取所有文件
    /// </summary>
    /// <param name="local">路径</param>
    /// <returns>文件列表</returns>
    public static List<FileInfo> GetAllFile(string local)
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
            list.AddRange(GetAllFile(item.FullName));
        }

        return list;
    }

    /// <summary>
    /// 复制文件夹
    /// </summary>
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
    /// <param name="file1">源</param>
    /// <param name="file2">目的</param>
    public static void CopyFile(string file1, string file2)
    {
        using var stream = OpenRead(file1);
        using var stream1 = OpenWrite(file2);
        if (stream == null)
        {
            return;
        }
        stream.CopyTo(stream1);
    }

    /// <summary>
    /// 搬运文件
    /// </summary>
    /// <param name="file1">源</param>
    /// <param name="file2">目的</param>
    public static void MoveFile(string file1, string file2)
    {
        CopyFile(file1, file2);
        Delete(file1);
    }

    /// <summary>
    /// 复制文件夹
    /// </summary>
    public static Task CopyDir(string dir, string dir1)
    {
        return Task.Run(() =>
        {
            Copys(dir, dir1);
        });
    }

    /// <summary>
    /// 删除文件夹
    /// </summary>
    public static async Task<bool> DeleteFiles(string local, bool req = true)
    {
        if (!Directory.Exists(local))
        {
            return true;
        }

        if (req && ColorMCCore.GameRequest != null)
        {
            var res = await ColorMCCore.GameRequest.Invoke(
                string.Format(LanguageHelper.Get("Core.Info2"), local));
            if (!res)
            {
                return false;
            }
        }

        return await Task.Run(() =>
        {
            try
            {
                Directory.Delete(local, true);

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
    /// <returns>文件名</returns>
    public static string? GetFile(string local, string name)
    {
        var list = GetAllFile(local);
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
        if (SystemInfo.Os == OsType.Android && local.StartsWith("content://"))
        {
            return ColorMCCore.PhoneReadFile?.Invoke(local);
        }
        if (File.Exists(local))
        {
            return File.Open(local, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        return null;
    }

    /// <summary>
    /// 写文件
    /// </summary>
    /// <param name="local">路径</param>
    /// <returns>流</returns>
    public static Stream OpenWrite(string local)
    {
        return File.Open(local, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
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
        if (SystemInfo.Os == OsType.Android && local.StartsWith("content://"))
        {
            return;
        }
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
        var info = new FileInfo(local);
        info.Directory?.Create();
        using var stream = OpenWrite(local);
        stream.Write(data, 0, data.Length);
    }

    /// <summary>
    /// 替换文件名非法字符
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string ReplaceFileName(string name)
    {
        var chars = Path.GetInvalidFileNameChars().ToList();
        var builder = new StringBuilder();
        foreach (var item in name)
        {
            if (chars.Contains(item))
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
}