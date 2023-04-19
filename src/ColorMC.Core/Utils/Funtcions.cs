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
    public static bool CheckNotNumber(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return true;
        return Regex1().IsMatch(input);
    }

    [GeneratedRegex("[^0-9]+")]
    private static partial Regex Regex1();

    public static DateTime MillisecondsToDataTime(long unixTimeStamp)
    {
        var start = new DateTime(1970, 1, 1) +
            TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
        return start.AddMilliseconds(unixTimeStamp);
    }

    public static DateTime SecondsToDataTime(long unixTimeStamp)
    {
        var start = new DateTime(1970, 1, 1) +
            TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
        return start.AddSeconds(unixTimeStamp);
    }
    public static string GenMd5(byte[] data)
    {
        var text = new StringBuilder();
        foreach (byte iByte in MD5.HashData(data))
        {
            text.AppendFormat("{0:x2}", iByte);
        }
        return text.ToString().ToLower();
    }
    public static string GenSha1(byte[] data)
    {
        var text = new StringBuilder();
        foreach (byte item in SHA1.HashData(data))
        {
            text.AppendFormat("{0:x2}", item);
        }
        return text.ToString().ToLower();
    }

    public static string GenSha1(string input)
    {
        return GenSha1(Encoding.UTF8.GetBytes(input));
    }

    public static string GenSha256(string input)
    {
        return GenSha256(Encoding.UTF8.GetBytes(input));
    }

    public static string GenSha1(Stream stream)
    {
        var text = new StringBuilder();
        foreach (byte item in SHA1.HashData(stream))
        {
            text.AppendFormat("{0:x2}", item);
        }
        return text.ToString().ToLower();
    }

    public static async Task<string> GenSha1Async(Stream stream)
    {
        var text = new StringBuilder();
        foreach (byte item in await SHA1.HashDataAsync(stream))
        {
            text.AppendFormat("{0:x2}", item);
        }
        return text.ToString().ToLower();
    }

    public static string GenSha256(byte[] data)
    {
        var text = new StringBuilder();
        foreach (byte item in SHA256.HashData(data))
        {
            text.AppendFormat("{0:x2}", item);
        }
        return text.ToString().ToLower();
    }

    public static string GenSha256(Stream stream)
    {
        var text = new StringBuilder();
        foreach (byte item in SHA256.HashData(stream))
        {
            text.AppendFormat("{0:x2}", item);
        }
        return text.ToString().ToLower();
    }

    public static string NewUUID()
    {
        return Guid.NewGuid().ToString().ToLower();
    }

    public static string GenBase64(string input)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
    }

    public static void RunGC()
    {
        Task.Run(() =>
        {
            Task.Delay(1000).Wait();
            GC.Collect();
            GC.Collect();
        });
    }

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
