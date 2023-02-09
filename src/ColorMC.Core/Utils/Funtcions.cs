using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Zip;
using System.Security.Cryptography;
using System.Text;
using System;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace ColorMC.Core.Utils;

public static class Funtcions
{
    public static string GenMd5(byte[] data)
    {
        StringBuilder EnText = new();
        foreach (byte iByte in MD5.HashData(data))
        {
            EnText.AppendFormat("{0:x2}", iByte);
        }
        return EnText.ToString().ToLower();
    }
    public static string GenSha1(byte[] data)
    {
        StringBuilder EnText = new();
        foreach (byte iByte in SHA1.HashData(data))
        {
            EnText.AppendFormat("{0:x2}", iByte);
        }
        return EnText.ToString().ToLower();
    }

    public static string GenSha1(Stream stream)
    {
        SHA1 sha1 = SHA1.Create();
        StringBuilder EnText = new();
        foreach (byte iByte in sha1.ComputeHash(stream))
        {
            EnText.AppendFormat("{0:x2}", iByte);
        }
        return EnText.ToString().ToLower();
    }

    public static string GenSha256(Stream stream)
    {
        SHA256 sha1 = SHA256.Create();
        StringBuilder EnText = new();
        foreach (byte iByte in sha1.ComputeHash(stream))
        {
            EnText.AppendFormat("{0:x2}", iByte);
        }
        return EnText.ToString().ToLower();
    }

    public static string NewUUID()
    {
        return Guid.NewGuid().ToString().ToLower();
    }

    public static string GenBase64(string input)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(bytes);
    }

    public static void RunGC()
    {
        Task.Run(() =>
        {
            Task.Delay(1000).Wait();
            GC.Collect(2);
        });
    }
}

public static class ZipFloClass
{
    public static async Task ZipFile(string strFile, string strZip)
    {
        if (strFile[^1] != Path.DirectorySeparatorChar)
            strFile += Path.DirectorySeparatorChar;
        using var s = new ZipOutputStream(File.Create(strZip));
        s.SetLevel(6); // 0 - store only to 9 - means best compression
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


    public static async Task ZipFile(string strFile, string strZip, List<string> filter)
    {
        if (strFile[^1] != Path.DirectorySeparatorChar)
            strFile += Path.DirectorySeparatorChar;
        using var s = new ZipOutputStream(File.Create(strZip));
        s.SetLevel(6); // 0 - store only to 9 - means best compression
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
            ZipInputStream s = new(File.OpenRead(local));
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
