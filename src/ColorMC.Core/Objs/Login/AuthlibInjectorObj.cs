using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.Login;

public record AuthlibInjectorMetaObj
{
    public record ArtifactsObj
    {
        [JsonPropertyName("build_number")]
        public int BuildNumber { get; set; }
        //public string version { get; set; }
    }
    [JsonPropertyName("latest_build_number")]
    public int LatestBuildNumber { get; set; }
    [JsonPropertyName("artifacts")]
    public List<ArtifactsObj> Artifacts { get; set; }
}

public record AuthlibInjectorObj
{
    public record ChecksumsObj
    {
        [JsonPropertyName("sha256")]
        public string Sha256 { get; set; }
    }
    [JsonPropertyName("build_number")]
    public int BuildNumber { get; set; }
    [JsonPropertyName("version")]
    public string Version { get; set; }
    [JsonPropertyName("download_url")]
    public string DownloadUrl { get; set; }
    [JsonPropertyName("checksums")]
    public ChecksumsObj Checksums { get; set; }
}