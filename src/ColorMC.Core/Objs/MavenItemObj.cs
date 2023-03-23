namespace ColorMC.Core.Objs;

public record MavenItemObj
{
    public string Name { get; set; }
    public string SHA1 { get; set; }
    public string SHA256 { get; set; }
    public string Local { get; set; }
    public string Url { get; set; }
    public bool Have { get; set; }
}
