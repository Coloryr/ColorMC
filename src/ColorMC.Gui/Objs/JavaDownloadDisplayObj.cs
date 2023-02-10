namespace ColorMC.Gui.Objs;

public record JavaDownloadDisplayObj
{
    public string Name { get; set; }
    public string Os { get; set; }
    public string Arch { get; set; }
    public string MainVersion { get; set; }
    public string Version { get; set; }
    public string Size { get; set; }

    public string Url;
    public string Sha256;
    public string File;
}
