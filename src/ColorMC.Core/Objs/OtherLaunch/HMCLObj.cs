namespace ColorMC.Core.Objs.OtherLaunch;

public record HMCLObj
{
    public record Addons
    {
        public string id { get; set; }
        public string version { get; set; }
    }
    public record Files
    {
        public string path { get; set; }
        public string hash { get; set; }
        public bool force { get; set; }
        public string type { get; set; }
    }
    public record Settings
    {
        public bool install_mods { get; set; }
        public bool install_resourcepack { get; set; }
    }
    public record LaunchInfo
    {
        public uint minMemory { get; set; }
        public List<string> launchArgument { get; set; }
        public List<string> javaArgument { get; set; }
    }
    public string manifestType { get; set; }
    public int manifestVersion { get; set; }
    public string name { get; set; }
    public string version { get; set; }
    public string author { get; set; }
    public string description { get; set; }
    public string fileApi { get; set; }
    public string url { get; set; }
    public bool forceUpdate { get; set; }
    public List<object> origin { get; set; }
    public List<Addons> addons { get; set; }
    public List<object> libraries { get; set; }
    public List<Files> files { get; set; }
    public Settings settings { get; set; }
    public LaunchInfo launchInfo { get; set; }
}
