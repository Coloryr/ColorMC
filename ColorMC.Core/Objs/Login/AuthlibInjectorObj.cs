namespace ColorMC.Core.Objs.Login;

public record AuthlibInjectorMetaObj
{
    public record Artifacts
    {
        public int build_number { get; set; }
        public string version { get; set; }
    }
    public int latest_build_number { get; set; }
    public List<Artifacts> artifacts { get; set; }
}

public record AuthlibInjectorObj
{
    public record Checksums
    {
        public string sha256 { get; set; }
    }
    public int build_number { get; set; }
    public string version { get; set; }
    public string download_url { get; set; }
    public Checksums checksums { get; set; }
}