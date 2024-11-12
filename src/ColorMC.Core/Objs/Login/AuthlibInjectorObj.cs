using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Login;

public record AuthlibInjectorMetaObj
{
    public record ArtifactsObj
    {
        [JsonProperty("build_number")]
        public int BuildNumber { get; set; }
        //public string version { get; set; }
    }
    [JsonProperty("latest_build_number")]
    public int LatestBuildNumber { get; set; }
    [JsonProperty("artifacts")]
    public List<ArtifactsObj> Artifacts { get; set; }
}

public record AuthlibInjectorObj
{
    public record ChecksumsObj
    {
        [JsonProperty("sha256")]
        public string Sha256 { get; set; }
    }
    [JsonProperty("build_number")]
    public int BuildNumber { get; set; }
    [JsonProperty("version")]
    public string Version { get; set; }
    [JsonProperty("download_url")]
    public string DownloadUrl { get; set; }
    [JsonProperty("checksums")]
    public ChecksumsObj Checksums { get; set; }
}