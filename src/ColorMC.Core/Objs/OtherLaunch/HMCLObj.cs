using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.OtherLaunch;

public record HMCLObj
{
    public record AddonsObj
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("version")]
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
        [JsonPropertyName("minMemory")]
        public uint MinMemory { get; set; }
        [JsonPropertyName("launchArgument")]
        public List<string> LaunchArgument { get; set; }
        [JsonPropertyName("javaArgument")]
        public List<string> JavaArgument { get; set; }
    }
    //public string manifestType { get; set; }
    //public int manifestVersion { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    //public string version { get; set; }
    //public string author { get; set; }
    //public string description { get; set; }
    //public string fileApi { get; set; }
    //public string url { get; set; }
    //public bool forceUpdate { get; set; }
    //public List<object> origin { get; set; }
    [JsonPropertyName("addons")]
    public List<AddonsObj> Addons { get; set; }
    //public List<object> libraries { get; set; }
    //public List<Files> files { get; set; }
    //public Settings settings { get; set; }
    [JsonPropertyName("launchInfo")]
    public LaunchInfoObj LaunchInfo { get; set; }
}
