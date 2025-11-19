using Avalonia.Platform.Storage;

namespace ColorMC.Gui.Utils;

/// <summary>
/// 路径处理
/// </summary>
public static class PathUtils
{
    /// <summary>
    /// 目录转字符串
    /// </summary>
    /// <param name="file">路径</param>
    /// <returns>路径字符串</returns>
    public static string? GetPath(this IStorageFolder file)
    {
        return file.Path.LocalPath;
    }

    /// <summary>
    /// 文件转字符串
    /// </summary>
    /// <param name="file">文件</param>
    /// <returns>路径字符串</returns>
    public static string? GetPath(this IStorageFile file)
    {
        return file.Path.LocalPath;
    }

    /// <summary>
    /// 文件转字符串
    /// </summary>
    /// <param name="file">文件</param>
    /// <returns>路径字符串</returns>
    public static string? GetPath(this IStorageItem file)
    {
        return file.Path.LocalPath;
    }
}
