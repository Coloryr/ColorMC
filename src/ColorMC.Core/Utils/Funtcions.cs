using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ColorMC.Core.Utils;

public static partial class Funtcions
{
    [GeneratedRegex("[^0-9]+")]
    private static partial Regex Regex1();

    /// <summary>
    /// 检查是否为数字
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool CheckNotNumber(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return true;
        return Regex1().IsMatch(input);
    }

    /// <summary>
    /// Tick转时间
    /// </summary>
    /// <param name="unixTimeStamp"></param>
    /// <returns></returns>
    public static DateTime MillisecondsToDataTime(long unixTimeStamp)
    {
        var start = new DateTime(1970, 1, 1) +
            TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
        return start.AddMilliseconds(unixTimeStamp);
    }
    
    /// <summary>
    /// 获取MD5值
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string GenMd5(byte[] data)
    {
        var text = new StringBuilder();
        foreach (byte iByte in MD5.HashData(data))
        {
            text.AppendFormat("{0:x2}", iByte);
        }
        return text.ToString().ToLower();
    }
    /// <summary>
    /// 获取SHA1值
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string GenSha1(byte[] data)
    {
        var text = new StringBuilder();
        foreach (byte item in SHA1.HashData(data))
        {
            text.AppendFormat("{0:x2}", item);
        }
        return text.ToString().ToLower();
    }
    /// <summary>
    /// 获取SHA1值
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string GenSha1(string input)
    {
        return GenSha1(Encoding.UTF8.GetBytes(input));
    }
    /// <summary>
    /// 获取SHA256值
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string GenSha256(string input)
    {
        return GenSha256(Encoding.UTF8.GetBytes(input));
    }
    /// <summary>
    /// 获取SHA1值
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static string GenSha1(Stream stream)
    {
        var text = new StringBuilder();
        foreach (byte item in SHA1.HashData(stream))
        {
            text.AppendFormat("{0:x2}", item);
        }
        return text.ToString().ToLower();
    }
    /// <summary>
    /// 获取SHA1值
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static async Task<string> GenSha1Async(Stream stream)
    {
        var text = new StringBuilder();
        foreach (byte item in await SHA1.HashDataAsync(stream))
        {
            text.AppendFormat("{0:x2}", item);
        }
        return text.ToString().ToLower();
    }
    /// <summary>
    /// 获取SHA256值
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string GenSha256(byte[] data)
    {
        var text = new StringBuilder();
        foreach (byte item in SHA256.HashData(data))
        {
            text.AppendFormat("{0:x2}", item);
        }
        return text.ToString().ToLower();
    }
    /// <summary>
    /// 获取SHA256值
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static string GenSha256(Stream stream)
    {
        var text = new StringBuilder();
        foreach (byte item in SHA256.HashData(stream))
        {
            text.AppendFormat("{0:x2}", item);
        }
        return text.ToString().ToLower();
    }
    /// <summary>
    /// 新的UUID
    /// </summary>
    /// <returns></returns>
    public static string NewUUID()
    {
        return Guid.NewGuid().ToString().ToLower();
    }
    /// <summary>
    /// 生成Base64
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string GenBase64(string input)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
    }
    /// <summary>
    /// 反解Base64
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string DeBase64(string input)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(input));
    }

    /// <summary>
    /// 执行内存回收
    /// </summary>
    public static void RunGC()
    {
        Task.Run(() =>
        {
            Task.Delay(1000).Wait();
            GC.Collect();
            GC.Collect();
        });
    }

    /// <summary>
    /// 截取字符串
    /// </summary>
    /// <param name="input"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static string GetString(string input, string start, string end)
    {
        var temp = input.IndexOf(start);
        if (temp == -1)
        {
            return input;
        }
        var temp1 = input.IndexOf(end, temp + start.Length + 1);
        if (temp1 == -1)
        {
            return input;
        }

        return input[(temp + start.Length)..temp1];
    }
}

public static class ZipUtils
{
    /// <summary>
    /// 压缩文件
    /// </summary>
    /// <param name="strFile">路径</param>
    /// <param name="strZip">文件名</param>
    public static async Task ZipFile(string strFile, string strZip)
    {
        if (strFile[^1] != Path.DirectorySeparatorChar)
            strFile += Path.DirectorySeparatorChar;
        using var s = new ZipOutputStream(File.Create(strZip));
        s.SetLevel(9); // 0 - store only to 9 - means best compression
        await Zip(strFile, s, strFile);
        await s.FinishAsync(CancellationToken.None);
        s.Close();
    }

    private static async Task Zip(string strFile, ZipOutputStream s, string staticFile)
    {
        if (strFile[^1] != Path.DirectorySeparatorChar) strFile += Path.DirectorySeparatorChar;
        var crc = new Crc32();
        string[] filenames = Directory.GetFileSystemEntries(strFile);
        foreach (string file in filenames)
        {
            if (Directory.Exists(file))
            {
                await Zip(file, s, staticFile);
            }

            else
            {
                using var fs = File.OpenRead(file);

                byte[] buffer = new byte[fs.Length];
                await fs.ReadAsync(buffer);
                string tempfile = file[(staticFile.LastIndexOf("\\") + 1)..];
                var entry = new ZipEntry(tempfile)
                {
                    DateTime = DateTime.Now,
                    Size = fs.Length
                };
                crc.Reset();
                crc.Update(buffer);
                entry.Crc = crc.Value;
                await s.PutNextEntryAsync(entry);
                await s.WriteAsync(buffer);
            }
        }
    }

    /// <summary>
    /// 压缩文件
    /// </summary>
    /// <param name="strFile">路径</param>
    /// <param name="strZip">文件名</param>
    /// <param name="filter">过滤</param>
    /// <returns></returns>
    public static async Task ZipFile(string strFile, string strZip, List<string> filter)
    {
        if (strFile[^1] != Path.DirectorySeparatorChar)
            strFile += Path.DirectorySeparatorChar;
        using var s = new ZipOutputStream(File.Create(strZip));
        s.SetLevel(9); // 0 - store only to 9 - means best compression
        await Zip(strFile, s, strFile, filter);
        await s.FinishAsync(CancellationToken.None);
        s.Close();
    }

    private static async Task Zip(string strFile, ZipOutputStream s,
        string staticFile, List<string> filter)
    {
        if (strFile[^1] != Path.DirectorySeparatorChar) strFile += Path.DirectorySeparatorChar;
        var crc = new Crc32();
        string[] filenames = Directory.GetFileSystemEntries(strFile);
        foreach (string file in filenames)
        {
            if (filter.Contains(file))
            {
                continue;
            }
            if (Directory.Exists(file))
            {
                await Zip(file, s, staticFile);
            }
            else
            {
                using var fs = File.OpenRead(file);

                byte[] buffer = new byte[fs.Length];
                await fs.ReadAsync(buffer);
                string tempfile = file[(staticFile.LastIndexOf("\\") + 1)..];
                var entry = new ZipEntry(tempfile)
                {
                    DateTime = DateTime.Now,
                    Size = fs.Length
                };
                crc.Reset();
                crc.Update(buffer);
                entry.Crc = crc.Value;
                await s.PutNextEntryAsync(entry);
                await s.WriteAsync(buffer);
            }
        }
    }

    /// <summary>
    /// 解压
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="local">文件</param>
    public static void Unzip(string path, string local)
    {
        if (local.EndsWith("tar.gz"))
        {
            using var inStream = File.OpenRead(local);
            using var gzipStream = new GZipInputStream(inStream);
            var tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.UTF8);
            tarArchive.ExtractContents(path);
            tarArchive.Close();
        }
        else
        {
            using ZipInputStream s = new(File.OpenRead(local));
            ZipEntry theEntry;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string filename = $"{path}/{theEntry.Name}";

                var directoryName = Path.GetDirectoryName(filename);
                string fileName = Path.GetFileName(theEntry.Name);

                // create directory
                if (directoryName?.Length > 0)
                {
                    Directory.CreateDirectory(directoryName);
                }

                if (fileName != string.Empty)
                {
                    using FileStream streamWriter = File.Create(filename);

                    s.CopyTo(streamWriter);
                }
            }
        }
    }
    /// <summary>
    /// 解压Zip
    /// </summary>
    /// <param name="path"></param>
    /// <param name="stream"></param>
    /// <param name="tar"></param>
    public static void Unzip(string path, Stream stream, bool tar)
    {
        if (tar)
        {
            using var gzipStream = new GZipInputStream(stream);
            var tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.UTF8);
            tarArchive.ExtractContents(path);
            tarArchive.Close();
        }
        else
        {
            using ZipInputStream s = new(stream);
            ZipEntry theEntry;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string filename = $"{path}/{theEntry.Name}";

                var directoryName = Path.GetDirectoryName(filename);
                string fileName = Path.GetFileName(theEntry.Name);

                // create directory
                if (directoryName?.Length > 0)
                {
                    Directory.CreateDirectory(directoryName);
                }

                if (fileName != string.Empty)
                {
                    using FileStream streamWriter = File.Create(filename);

                    s.CopyTo(streamWriter);
                }
            }
        }
    }
}

public static class CheckRule
{
    /// <summary>
    /// 检查是否允许
    /// </summary>
    public static bool CheckAllow(List<GameArgObj.Libraries.Rules> list)
    {
        bool download = true;
        if (list == null)
        {
            return true;
        }

        foreach (var item2 in list)
        {
            var action = item2.action;
            if (action == "allow")
            {
                if (item2.os == null)
                {
                    download = true;
                    continue;
                }
                var os = item2.os.name;

                if (os == "osx" && SystemInfo.Os == OsType.MacOS)
                {
                    download = true;
                }
                else if (os == "windows" && SystemInfo.Os == OsType.Windows)
                {
                    download = true;
                }
                else if (os == "linux" && SystemInfo.Os == OsType.Linux)
                {
                    download = true;
                }
                else
                {
                    download = false;
                }
            }
            else if (action == "disallow")
            {
                if (item2.os == null)
                {
                    download = false;
                    continue;
                }
                var os = item2.os.name;

                if (os == "osx" && SystemInfo.Os == OsType.MacOS)
                {
                    download = false;
                }
                else if (os == "windows" && SystemInfo.Os == OsType.Windows)
                {
                    download = false;
                }
                else if (os == "linux" && SystemInfo.Os == OsType.Linux)
                {
                    download = false;
                }
                else
                {
                    download = true;
                }
            }
        }

        return download;
    }

    /// <summary>
    /// 是否V2版本
    /// </summary>
    public static bool GameLaunchVersion(GameArgObj version)
    {
        return version.minimumLauncherVersion > 18;
    }

    /// <summary>
    /// 是否是1.17以上版本
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    public static bool GameLaunchVersion117(string version)
    {
        Version version1 = new(version);
        return version1.Minor >= 17;
    }

    public static bool GameLaunchVersion119(string version)
    {
        Version version1 = new(version);
        return version1.Minor >= 19;
    }
}

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
                Logs.Error(LanguageHelper.GetName("Core.Game.Error10"), e);
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
}