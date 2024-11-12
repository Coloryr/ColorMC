using Newtonsoft.Json;

namespace ColorMC.Core.Objs.OtherLaunch;

public record HMCLObj
{
    public record AddonsObj
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
    }
    //public record Files
    //{
    //    public string path { get; set; }
    //    public string hash { get; set; }
    //    public bool force { get; set; }
    //    public string type { get; set; }
    //}
    //public record Settings
    //{
    //    public bool install_mods { get; set; }
    //    public bool install_resourcepack { get; set; }
    //}
    public record LaunchInfoObj
    {
        [JsonProperty("minMemory")]
        public uint MinMemory { get; set; }
        [JsonProperty("launchArgument")]
        public List<string> LaunchArgument { get; set; }
        [JsonProperty("javaArgument")]
        public List<string> JavaArgument { get; set; }
    }
    //public string manifestType { get; set; }
    //public int manifestVersion { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    //public string version { get; set; }
    //public string author { get; set; }
    //public string description { get; set; }
    //public string fileApi { get; set; }
    //public string url { get; set; }
    //public bool forceUpdate { get; set; }
    //public List<object> origin { get; set; }
    [JsonProperty("addons")]
    public List<AddonsObj> Addons { get; set; }
    //public List<object> libraries { get; set; }
    //public List<Files> files { get; set; }
    //public Settings settings { get; set; }
    [JsonProperty("launchInfo")]
    public LaunchInfoObj LaunchInfo { get; set; }
}
