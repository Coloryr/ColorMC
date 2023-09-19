using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using System.Text;

namespace ColorMC.Core.Helpers;

public static class PathHelper
{
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
    /// <param name="dir">路径</param>
    /// <returns>文件列表</returns>
    public static List<FileInfo> GetAllFile(string dir)
    {
        var list = new List<FileInfo>();
        var info = new DirectoryInfo(dir);
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
    public static Task<bool> DeleteFiles(string dir)
    {
        return Task.Run(() =>
        {
            try
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }

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
    /// <param name="path">路径</param>
    /// <param name="name">文件名</param>
    /// <returns>文件名</returns>
    public static string? GetFile(string path, string name)
    {
        var list = GetAllFile(path);
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
    /// <param name="path">路径</param>
    /// <returns>流</returns>
    public static Stream? OpenRead(string path)
    {
        if (SystemInfo.Os == OsType.Android && path.StartsWith("content://"))
        {
            return ColorMCCore.PhoneReadFile?.Invoke(path);
        }
        if (File.Exists(path))
        {
            return File.OpenRead(path);
        }

        return null;
    }

    public static Stream OpenWrite(string path)
    {
        return File.Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
    }

    public static void WriteText(string local, string str)
    {
        var data = Encoding.UTF8.GetBytes(str);
        WriteBytes(local, data);
    }

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

    public static void Delete(string local)
    {
        if (SystemInfo.Os == OsType.Android && local.StartsWith("content://"))
        {
            return;
        }
        File.Delete(local);
    }

    public static void WriteBytes(string local, byte[] data)
    {
        var info = new FileInfo(local);
        info.Directory?.Create();
        using var stream = OpenWrite(local);
        stream.Write(data, 0, data.Length);
    }
}