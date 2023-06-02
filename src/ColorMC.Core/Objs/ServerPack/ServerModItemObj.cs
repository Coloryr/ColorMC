namespace ColorMC.Core.Objs.ServerPack;

public record ServerModItemObj
{
    public string File { get; set; }
    public SourceType? Source { get; set; }
    public string? Projcet { get; set; }
    public string? FileId { get; set; }
    public string Sha1 { get; set; }
    public string Url { get; set; }
}
