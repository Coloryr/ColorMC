namespace ColorMC.Core.Objs.CurseForge;

public record CurseForgeFileObj
{
    public List<CurseForgeObj.Data.LatestFiles> data { get; set; }
}
