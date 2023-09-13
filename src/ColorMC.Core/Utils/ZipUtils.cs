using ColorMC.Core.Helpers;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using SharpCompress.Compressors.Xz;
using System.Text;
using Crc32 = ICSharpCode.SharpZipLib.Checksum.Crc32;

namespace ColorMC.Core.Utils;

public class ZipUtils
{
    private int Size = 0;
    private int Now = 0;

    /// <summary>
    /// 压缩文件
    /// </summary>
    /// <param name="strFile">路径</param>
    /// <param name="strZip">文件名</param>
    /// <param name="filter">过滤</param>
    /// <returns></returns>
    public async Task ZipFile(string strFile, string strZip, List<string>? filter = null)
    {
        if (strFile[^1] != Path.DirectorySeparatorChar)
            strFile += Path.DirectorySeparatorChar;
        using var s = new ZipOutputStream(PathHelper.OpenWrite(strZip));
        s.SetLevel(9);
        Size = PathHelper.GetAllFile(strFile).Count;
        Now = 0;
        await Zip(strFile, s, strFile, filter);
        await s.FinishAsync(CancellationToken.None);
        s.Close();
    }

    private async Task Zip(string strFile, ZipOutputStream s,
        string staticFile, List<string>? filter)
    {
        if (strFile[^1] != Path.DirectorySeparatorChar) strFile += Path.DirectorySeparatorChar;
        var crc = new Crc32();
        string[] filenames = Directory.GetFileSystemEntries(strFile);
        foreach (string file in filenames)
        {
            if (filter != null && filter.Contains(file))
            {
                continue;
            }
            if (Directory.Exists(file))
            {
                await Zip(file, s, staticFile, filter);
            }
            else
            {
                Now++;
                ColorMCCore.UnZipItem?.Invoke(Path.GetFileName(file), Now, Size);
                using var fs = PathHelper.OpenRead(file)!;

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

    public async Task ZipFile(string strZip, List<string> list, string basepath)
    {
        using var s = new ZipOutputStream(PathHelper.OpenWrite(strZip));
        s.SetLevel(9);
        Size = list.Count;
        Now = 0;
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
                Now++;
                ColorMCCore.UnZipItem?.Invoke(item, Now, Size);
                using var fs = PathHelper.OpenRead(item)!;

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
    /// <param name="path">解压路径</param>
    /// <param name="local">文件名</param>
    public void Unzip(string path, string local, Stream stream)
    {
        if (local.EndsWith("tar.gz"))
        {
            using var gzipStream = new GZipInputStream(stream);
            var tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.UTF8);
            Size = tarArchive.RecordSize;
            tarArchive.ProgressMessageEvent += TarArchive_ProgressMessageEvent;
            tarArchive.ExtractContents(path);
            tarArchive.Close();
        }
        else if (local.EndsWith("tar.xz"))
        {
            using var gzipStream = new XZStream(stream);
            var tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.UTF8);
            Size = tarArchive.RecordSize;
            tarArchive.ProgressMessageEvent += TarArchive_ProgressMessageEvent;
            tarArchive.ExtractContents(path);
            tarArchive.Close();
        }
        else
        {
            using ZipFile s = new(stream);
            Size = (int)s.Count;
            foreach (ZipEntry theEntry in s)
            {
                Now++;
                ColorMCCore.UnZipItem?.Invoke(theEntry.Name, Now, Size);
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
                    using var stream1 = PathHelper.OpenWrite(filename);
                    using var stream2 = s.GetInputStream(theEntry);
                    stream2.CopyTo(stream1);
                }
            }
        }
    }

    private void TarArchive_ProgressMessageEvent(TarArchive archive, TarEntry entry, string message)
    {
        if (entry != null && message == null)
        {
            Now++;
            ColorMCCore.UnZipItem?.Invoke(entry.Name, Now, Size);
        }
    }
}

