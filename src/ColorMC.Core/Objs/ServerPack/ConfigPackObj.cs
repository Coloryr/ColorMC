namespace ColorMC.Core.Objs.ServerPack;

public record ConfigPackObj
{
    public string Group { get; set; }
    public string FileName { get; set; }
    public string Sha1 { get; set; }
    public bool Zip { get; set; }
    public string Url { get; set; }
    public bool Dir { get; set; }
}
