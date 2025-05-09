using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Loader;

/// <summary>
/// Forge安装数据
/// </summary>
public record ForgeInstallObj
{
    //public record Data
    //{
    //    public record DataObj
    //    {
    //        public string client { get; set; }
    //        public string server { get; set; }
    //    }

    //    public DataObj MAPPINGS { get; set; }
    //    public DataObj MOJMAPS { get; set; }
    //    public DataObj MERGED_MAPPINGS { get; set; }
    //    public DataObj BINPATCH { get; set; }
    //    public DataObj MC_UNPACKED { get; set; }
    //    public DataObj MC_SLIM { get; set; }
    //    public DataObj MC_SLIM_SHA { get; set; }
    //    public DataObj MC_EXTRA { get; set; }
    //    public DataObj MC_EXTRA_SHA { get; set; }
    //    public DataObj MC_SRG { get; set; }
    //    public DataObj PATCHED { get; set; }
    //    public DataObj PATCHED_SHA { get; set; }
    //    public DataObj MCP_VERSION { get; set; }
    //}

    //public record ProcessorsItem
    //{
    //    public List<string> sides { get; set; }
    //    public string jar { get; set; }
    //    public List<string> classpath { get; set; }
    //    public List<string> args { get; set; }
    //}

    //public List<string> _comment_ { get; set; }
    //public int spec { get; set; }
    [JsonPropertyName("profile")]
    public string Profile { get; set; }
    [JsonPropertyName("version")]
    public string Version { get; set; }
    //public string path { get; set; }
    [JsonPropertyName("minecraft")]
    public string Minecraft { get; set; }
    //public string serverJarPath { get; set; }
    //public Data data { get; set; }
    //public List<ProcessorsItem> processors { get; set; }
    [JsonPropertyName("libraries")]
    public List<ForgeLaunchObj.ForgeLibrariesObj> Libraries { get; set; }
    //public string icon { get; set; }
    //public string json { get; set; }
    //public string logo { get; set; }
    //public string mirrorList { get; set; }
    //public string welcome { get; set; }
}


public record ForgeInstallOldObj
{
    //public record Install
    //{
    //    public string profileName { get; set; }
    //    public string target { get; set; }
    //    public string path { get; set; }
    //    public string version { get; set; }
    //    public string filePath { get; set; }
    //    public string welcome { get; set; }
    //    public string minecraft { get; set; }
    //    public string mirrorList { get; set; }
    //    public string logo { get; set; }
    //}
    public record VersionInfoObj
    {
        public record LibrariesObj
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
            [JsonPropertyName("url")]
            public string Url { get; set; }
            //public bool serverreq { get; set; }
            //public bool clientreq { get; set; }
            //public List<string> checksums { get; set; }
        }
        //[JsonProperty("id")]
        //public string Id { get; set; }
        //[JsonProperty("time")]
        //public string Time { get; set; }
        //[JsonProperty("releaseTime")]
        //public string ReleaseTime { get; set; }
        //[JsonProperty("type")]
        //public string Type { get; set; }
        [JsonPropertyName("minecraftArguments")]
        public string MinecraftArguments { get; set; }
        [JsonPropertyName("mainClass")]
        public string MainClass { get; set; }
        //public int minimumLauncherVersion { get; set; }
        //public string assets { get; set; }
        //[JsonProperty("inheritsFrom")]
        //public string InheritsFrom { get; set; }
        //public string jar { get; set; }

        [JsonPropertyName("libraries")]
        public List<LibrariesObj> Libraries { get; set; }
    }

    //public Install install { get; set; }
    [JsonPropertyName("versionInfo")]
    public VersionInfoObj VersionInfo { get; set; }
}