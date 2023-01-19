using ColorMC.Core.Objs;

namespace ColorMC.Core.Utils;

public static class PathC
{
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

    public static LibVersionObj MakeVersionObj(string name)
    {
        var arg = name.Split(":");
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

    public static void Copys(string dir, string dir1)
    {
        var floderName = Path.GetFileName(dir);
        var di = Directory.CreateDirectory(Path.Combine(dir1, floderName));
        var files = Directory.GetFileSystemEntries(dir);

        foreach (string file in files)
        {
            if (Directory.Exists(file))
            {
                CopyFiles(file, di.FullName);
            }
            else
            {
                File.Copy(file, Path.Combine(di.FullName,
                    Path.GetFileName(file)), true);
            }
        }
    }

    public static Task CopyFiles(string dir, string dir1)
    {
        return Task.Run(() =>
        {
            Copys(dir, dir1);
        });
    }

    public static Task DeleteFiles(string dir)
    {
        return Task.Run(() =>
        {
            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        });
    }
}
