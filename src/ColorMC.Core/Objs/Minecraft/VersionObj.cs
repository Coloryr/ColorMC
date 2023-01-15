namespace ColorMC.Core.Objs.Minecraft;

public record VersionObj
{
    public record Lastest
    {
        public string release { get; set; }
        public string snapshot { get; set; }
    }

    public record Versions
    {
        public string id { get; set; }
        public string type { get; set; }
        public string url { get; set; }
        public string time { get; set; }
        public string releaseTime { get; set; }
        public string sha1 { get; set; }
        public int complianceLevel { get; set; }
    }

    public Lastest lastest { get; set; }
    public List<Versions> versions { get; set; }
}
