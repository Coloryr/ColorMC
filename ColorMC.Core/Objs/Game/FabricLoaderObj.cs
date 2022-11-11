namespace ColorMC.Core.Objs.Game;

public record FabricLoaderObj
{
    public record Arguments
    {
        public List<string> game { get; set; }
        public List<string> jvm { get; set; }
    }
    public record Libraries
    {
        public string name { get; set; }
        public string url { get; set; }
    }
    public string id { get; set; }
    public string inheritsFrom { get; set; }
    public string releaseTime { get; set; }
    public string time { get; set; }
    public string type { get; set; }
    public string mainClass { get; set; }
    public Arguments arguments { get; set; }
    public List<Libraries> libraries { get; set; }
}
