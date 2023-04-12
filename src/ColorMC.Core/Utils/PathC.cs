using ColorMC.Core.Objs;

namespace ColorMC.Core.Utils;

public static class PathC
{
    /// <summary>
    /// 获取名字
    /// </summary>
    public static (string Path, string Name) ToName(string input)
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
    /// 版本名
    /// </summary>
    public static LibVersionObj MakeVersionObj(string name)
    {
        var arg = name.Split(":");
        if (arg.Length < 3)
        {
            return new()
            {
                Name = name
            };
        }
        if (arg.Length > 3)
        {
            return new()
            {
                Pack = arg[0],
                Name = arg[1],
                Verison = arg[2],
                Extr = arg[3]
            };
        }

        return new()
        {
            Pack = arg[0],
            Name = arg[1],
            Verison = arg[2]
        };
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
                File.Copy(file, Path.GetFullPath(dir1 + "/" + Path.GetFileName(file)), true);
            }
        }
    }

    /// <summary>
    /// 复制文件夹
    /// </summary>
    public static Task CopyFiles(string dir, string dir1)
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
                    Directory.Delete(dir, true);

                return true;
            }
            catch (Exception e)
            {
                Logs.Error("Delete Game Error", e);
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
        Logs.Info("seach file");
        foreach (var item in list)
        {
            if (item.Name == name)
                return item.FullName;
        }

        return null;
    }
}
