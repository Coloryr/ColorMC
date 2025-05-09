using System.Formats.Tar;
using System.IO.Compression;
using System.Text;
using ColorMC.Core.Helpers;

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
        using var s = new ZipArchive(PathHelper.OpenWrite(zipFile, true), ZipArchiveMode.Create);
        _size = PathHelper.GetAllFiles(zipDir).Count;
        _now = 0;
        await ZipAsync(zipDir, s, zipDir, filter);
    }

    /// <summary>
    /// 打包Zip
    /// </summary>
    /// <param name="strFile"></param>
    /// <param name="s"></param>
    /// <param name="staticFile"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    private async Task ZipAsync(string strFile, ZipArchive s,
        string staticFile, List<string>? filter)
    {
        if (strFile[^1] != Path.DirectorySeparatorChar) strFile += Path.DirectorySeparatorChar;
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
                var buffer = PathHelper.OpenRead(file)!;
                string tempfile = file[(staticFile.LastIndexOf(Path.DirectorySeparatorChar) + 1)..];
                var entry = s.CreateEntry(tempfile);
                using var stream = entry.Open();
                await buffer.CopyToAsync(stream);
                entry.LastWriteTime = DateTime.Now;
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
        using var s = new ZipArchive(PathHelper.OpenWrite(zipFile, true), ZipArchiveMode.Create);
        _size = zipList.Count;
        _now = 0;

        foreach (var item in zipList)
        {
            string tempfile = item[(path.Length + 1)..];
            _now++;
            ZipUpdate?.Invoke(item, _now, _size);
            using var buffer = PathHelper.OpenRead(item)!;
            var entry = s.CreateEntry(tempfile);
            using var stream = entry.Open();
            await buffer.CopyToAsync(stream);
        }
    }

    /// <summary>
    /// 解压
    /// </summary>
    /// <param name="path">解压路径</param>
    /// <param name="file">文件名</param>
    public async Task<bool> UnzipAsync(string path, string file, Stream stream)
    {
        if (file.EndsWith(Names.NameTarGzExt))
        {
            using var gzipStream = new GZipStream(stream, CompressionMode.Decompress);
            var tarArchive = new TarReader(gzipStream);
            _size = 0;
            _now = 0;

            TarEntry? entry;
            while ((entry = await tarArchive.GetNextEntryAsync().ConfigureAwait(false)) != null)
            {
                var item = Path.GetFullPath($"{path}/{entry.Name}");
                var info = new FileInfo(item);
                info.Directory?.Create();
                if (entry.EntryType is not TarEntryType.GlobalExtendedAttributes)
                {
                    ZipUpdate?.Invoke(entry.Name, 0, 0);
                    await entry.ExtractToFileAsync(item, true);
                }
            }
        }
        else
        {
            using var s = new ZipArchive(stream);
            _size = s.Entries.Count;
            foreach (var theEntry in s.Entries)
            {
                _now++;
                ZipUpdate?.Invoke(theEntry.Name, _now, _size);

                var item = Path.GetFullPath($"{path}/{theEntry.Name}");
                var info = new FileInfo(item);

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
                        item = Path.Combine(info.Directory!.FullName, PathHelper.ReplaceFileName(info.Name));
                    }
                    using var stream2 = theEntry.Open();
                    await PathHelper.WriteBytesAsync(item, stream2);
                }
            }
        }

        return true;
    }
}

