using ColorMC.Core.Objs;

namespace ColorMC.Gui.Objs;

/// <summary>
/// 收藏资源下载项目
/// </summary>
public record CollectFileItemObj
{
    public FileItemObj File;
    public SourceItemObj Source;
    public bool Download;
}
