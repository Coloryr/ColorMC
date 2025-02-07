using System.Text;
using ColorMC.Core.Helpers;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using Crc32 = ICSharpCode.SharpZipLib.Checksum.Crc32;

namespace ColorMC.Core.Utils;

/// <summary>
/// 压缩包处理
/// </summary>
public class ZipUtils(ColorMCCore.ZipUpdate? ZipUpdate = null,
    ColorMCCore.Request? GameRequest = null)
{
    private int _size = 0;
    private int _now = 0;

    /// <summary>
    /// 压缩文件
    /// </summary>
    /// <param name="zipDir">路径</param>
    /// <param name="zipFile">文件名</param>
    /// <param name="filter">过滤</param>
    /// <returns></returns>
    public async Task ZipFileAsync(string zipDir, string zipFile, List<string>? filter = null)
    {
        if (zipDir[^1] != Path.DirectorySeparatorChar)
            zipDir += Path.DirectorySeparatorChar;
        using var s = new ZipOutputStream(PathHelper.OpenWrite(zipFile, true));
        s.SetLevel(9);
        _size = PathHelper.GetAllFile(zipDir).Count;
        _now = 0;
        await ZipAsync(zipDir, s, zipDir, filter);
        await s.FinishAsync(CancellationToken.None);
        s.Close();
    }

    /// <summary>
    /// 打包Zip
    /// </summary>
    /// <param name="strFile"></param>
    /// <param name="s"></param>
    /// <param name="staticFile"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    private async Task ZipAsync(string strFile, ZipOutputStream s,
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
                await ZipAsync(file, s, staticFile, filter);
            }
            else
            {
                _now++;
                ZipUpdate?.Invoke(Path.GetFileName(file), _now, _size);
                var buffer = PathHelper.ReadByte(file)!;

                string tempfile = file[(staticFile.LastIndexOf(Path.DirectorySeparatorChar) + 1)..];
                var entry = new ZipEntry(tempfile)
                {
                    DateTime = DateTime.Now,
                    Size = buffer.Length
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
    /// <param name="zipFile">压缩包路径</param>
    /// <param name="zipList">压缩的文件</param>
    /// <param name="path">替换的前置路径</param>
    /// <returns></returns>
    public async Task ZipFileAsync(string zipFile, List<string> zipList, string path)
    {
        using var s = new ZipOutputStream(PathHelper.OpenWrite(zipFile, true));
        s.SetLevel(9);
        _size = zipList.Count;
        _now = 0;
        var crc = new Crc32();

        foreach (var item in zipList)
        {
            string tempfile = item[(path.Length + 1)..];
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
                _now++;
                ZipUpdate?.Invoke(item, _now, _size);
                var buffer = PathHelper.ReadByte(item)!;

                var entry = new ZipEntry(tempfile)
                {
                    DateTime = DateTime.Now,
                    Size = buffer.Length
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
    public async Task<bool> UnzipAsync(string path, string local, Stream stream)
    {
        if (local.EndsWith(Names.NameTarGzExt))
        {
            using var gzipStream = new GZipInputStream(stream);
            var tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.UTF8);
            _size = tarArchive.RecordSize;
            tarArchive.ProgressMessageEvent += TarArchive_ProgressMessageEvent;
            tarArchive.ExtractContents(path);
            tarArchive.Close();
        }
        else
        {
            using var s = new ZipFile(stream);
            _size = (int)s.Count;
            foreach (ZipEntry theEntry in s)
            {
                _now++;
                ZipUpdate?.Invoke(theEntry.Name, _now, _size);

                var file = Path.GetFullPath($"{path}/{theEntry.Name}");
                var info = new FileInfo(file);

                info.Directory?.Create();

                if (info.Name != string.Empty)
                {
                    if (PathHelper.FileHasInvalidChars(info.Name))
                    {
                        if (GameRequest == null)
                        {
                            return false;
                        }
                        var res = await GameRequest.Invoke(string.Format(
                            LanguageHelper.Get("Core.Zip.Info1"), theEntry.Name));
                        if (!res)
                        {
                            return false;
                        }
                        file = Path.Combine(info.Directory!.FullName, PathHelper.ReplaceFileName(info.Name));
                    }
                    using var stream2 = s.GetInputStream(theEntry);
                    await PathHelper.WriteBytesAsync(file, stream2);
                }
            }
        }

        return true;
    }

    /// <summary>
    /// 进度事件回调
    /// </summary>
    /// <param name="archive"></param>
    /// <param name="entry"></param>
    /// <param name="message"></param>
    private void TarArchive_ProgressMessageEvent(TarArchive archive, TarEntry entry, string message)
    {
        if (entry != null && message == null)
        {
            _now++;
            ZipUpdate?.Invoke(entry.Name, _now, _size);
        }
    }
}

