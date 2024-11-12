using ICSharpCode.SharpZipLib.Zip;

namespace ColorMC.Core.Utils;

/// <summary>
/// 文件流处理
/// </summary>
public class ZipFileStream : IStaticDataSource, IDisposable
{
    private readonly MemoryStream Memory;

    public ZipFileStream(byte[] data)
    {
        Memory = new(data);
        Memory.Seek(0, SeekOrigin.Begin);
    }

    public void Dispose()
    {
        Memory.Dispose();
    }

    public Stream GetSource()
    {
        return Memory;
    }
}
