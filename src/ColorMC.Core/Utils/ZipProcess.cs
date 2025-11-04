using System.Formats.Tar;
using ColorMC.Core.GuiHandel;
using ColorMC.Core.Helpers;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Writers.Zip;

namespace ColorMC.Core.Utils;

/// <summary>
/// 压缩包处理
/// </summary>
public class ZipProcess(IZipGui? gui = null)
{
    private int _size = 0;
    private int _now = 0;

    /// <summary>
    /// 压缩文件
    /// </summary>
    /// <param name="dir">压缩路径</param>
    /// <param name="stream">文件流</param>
    /// <param name="filter">过滤</param>
    /// <returns>压缩流</returns>
    public async Task<ZipWriter> ZipFileAsync(string dir, Stream stream, List<string>? filter = null)
    {
        if (dir[^1] != Path.DirectorySeparatorChar)
            dir += Path.DirectorySeparatorChar;
        var zip = new ZipWriter(stream, new ZipWriterOptions(CompressionType.Deflate));
        _size = PathHelper.GetAllFiles(dir).Count;
        _now = 0;
        await ZipAsync(dir, zip, dir, filter);
        return zip;
    }

    /// <summary>
    /// 打包Zip
    /// </summary>
    /// <param name="strFile">打包路径</param>
    /// <param name="zip">压缩流</param>
    /// <param name="rootPath">根路径</param>
    /// <param name="filter">文件过滤</param>
    private async Task ZipAsync(string strFile, ZipWriter zip, string rootPath, List<string>? filter)
    {
        if (strFile[^1] != Path.DirectorySeparatorChar) strFile += Path.DirectorySeparatorChar;
        var filenames = Directory.GetFileSystemEntries(strFile);
        foreach (string file in filenames)
        {
            if (filter != null && filter.Contains(file))
            {
                continue;
            }
            if (Directory.Exists(file))
            {
                await ZipAsync(file, zip, rootPath, filter);
            }
            else
            {
                _now++;
                gui?.ZipUpdate(Path.GetFileName(file), _now, _size);
                var buffer = PathHelper.OpenRead(file)!;
                string tempfile = file[(rootPath.LastIndexOf(Path.DirectorySeparatorChar) + 1)..];
                using var stream = zip.WriteToStream(tempfile, new ZipWriterEntryOptions());
                await buffer.CopyToAsync(stream);
            }
        }
    }

    /// <summary>
    /// 压缩文件
    /// </summary>
    /// <param name="zipFile">压缩包路径</param>
    /// <param name="zipList">压缩的文件</param>
    /// <param name="rootPath">替换的前置路径</param>
    /// <returns></returns>
    public async Task ZipFileAsync(string zipFile, List<string> zipList, string rootPath)
    {
        using var stream = PathHelper.OpenWrite(zipFile);
        var zip = new ZipWriter(stream, new ZipWriterOptions(CompressionType.Deflate));
        _size = zipList.Count;
        _now = 0;

        foreach (var item in zipList)
        {
            string tempfile = item[(rootPath.Length + 1)..];
            _now++;
            gui?.ZipUpdate(item, _now, _size);
            using var buffer = PathHelper.OpenRead(item)!;
            using var stream1 = zip.WriteToStream(tempfile, new ZipWriterEntryOptions());
            await buffer.CopyToAsync(stream1);
        }
    }

    /// <summary>
    /// 解压
    /// </summary>
    /// <param name="path">解压路径</param>
    /// <param name="file">文件名</param>
    /// <param name="stream"></param>
    public async Task<bool> UnzipAsync(string path, string file, Stream stream)
    {
        if (file.EndsWith(Names.NameTarGzExt))
        {
            using var gzipStream = new GZipStream(stream, CompressionMode.Decompress);
            var tarArchive = new TarReader(gzipStream);
            _size = 0;
            _now = 0;

            while (await tarArchive.GetNextEntryAsync().ConfigureAwait(false) is { } entry)
            {
                var item = Path.GetFullPath($"{path}/{entry.Name}");
                var info = new FileInfo(item);
                info.Directory?.Create();
                if (entry.EntryType is not TarEntryType.GlobalExtendedAttributes)
                {
                    gui?.ZipUpdate(entry.Name, 0, 0);
                    await entry.ExtractToFileAsync(item, true);
                }
            }
        }
        else
        {
            using var s = ZipArchive.Open(stream);
            _size = s.Entries.Count;
            foreach (var e in s.Entries)
            {
                _now++;
                gui?.ZipUpdate(e.Key ?? "", _now, _size);

                if (!FuntionUtils.IsFile(e))
                {
                    continue;
                }

                var item = Path.GetFullPath($"{path}/{e.Key}");
                var info = new FileInfo(item);
                if (PathHelper.FileHasInvalidChars(info.Name))
                {
                    if (gui == null)
                    {
                        return false;
                    }
                    var res = await gui.ZipRequest(e.Key);
                    if (!res)
                    {
                        return false;
                    }
                    item = Path.Combine(info.Directory!.FullName, PathHelper.ReplaceFileName(info.Name));
                }
                using var stream2 = e.OpenEntryStream();
                await PathHelper.WriteBytesAsync(item, stream2);
            }
        }

        return true;
    }
}

