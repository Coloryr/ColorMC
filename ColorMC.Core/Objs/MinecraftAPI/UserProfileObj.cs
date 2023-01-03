namespace ColorMC.Core.Objs.MinecraftAPI;

public record UserProfileObj
{
    public record Properties
    {
        public string name { get; set; }
        public string value { get; set; }
        public string signature { get; set; }
    }
    public string id { get; set; }
    public string name { get; set; }
    public List<Properties> properties { get; set; }
}
