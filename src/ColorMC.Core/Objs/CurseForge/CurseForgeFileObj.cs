namespace ColorMC.Core.Objs.CurseForge;

/// <summary>
/// CF文件列表
/// </summary>
public record CurseForgeFileObj
{
    public List<CurseForgeObj.Data.LatestFiles> data { get; set; }
}
