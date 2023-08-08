using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Utils;

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
        s.SetLevel(9);
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

    public static async Task ZipFile(string strZip, List<string> list, string basepath)
    {
        using var s = new ZipOutputStream(File.Create(strZip));
        s.SetLevel(9);
        var crc = new Crc32();

        foreach (var item in list)
        {
            string tempfile = item[(basepath.Length + 1)..];
            if (Directory.Exists(item))
            {
                var entry = new ZipEntry(tempfile + "/")
                {
                    DateTime = DateTime.Now
                };
                await s.PutNextEntryAsync(entry);
            }
            else
            {
                using var fs = File.OpenRead(item);

                byte[] buffer = new byte[fs.Length];
                await fs.ReadAsync(buffer);

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
                    using var streamWriter = File.Create(filename);

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

