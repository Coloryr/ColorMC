namespace ColorMC.Core.Objs.ServerPack;

public record ConfigPackObj
{
    public string Group { get; set; }
    public string FileName { get; set; }
    public string Sha256 { get; set; }
    public bool IsZip { get; set; }
    public bool IsDir { get; set; }
    public string Url { get; set; }
}
