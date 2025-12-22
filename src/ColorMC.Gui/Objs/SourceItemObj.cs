using ColorMC.Core.Objs;

namespace ColorMC.Gui.Objs;

/// <summary>
/// 下载源项目信息
/// </summary>
public record SourceItemObj
{
    /// <summary>
    /// 资源类型
    /// </summary>
    public required FileType Type;
    /// <summary>
    /// 下载源
    /// </summary>
    public required SourceType Source;
    /// <summary>
    /// 项目ID
    /// </summary>
    public required string Pid;
    /// <summary>
    /// 文件ID
    /// </summary>
    public required string Fid;

    public bool CheckProject(SourceItemObj other)
    {
        return Type == other.Type && Source == other.Source && Pid == other.Pid;
    }

    public bool CheckProject(SourceType source, FileType type, string pid)
    {
        return Type == type && Source == source && Pid == pid;
    }
}
