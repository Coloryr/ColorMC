namespace ColorMC.Core.Objs.Optifine;

public record OptifineObj
{
    public string Version { get; set; }
    public string MCVersion { get; set; }

    public string Forge { get; set; }
    public string FileName { get; set; }
    public string Date { get; set; }

    public string Url1 { get; set; }
    public string Url2 { get; set; }
}
