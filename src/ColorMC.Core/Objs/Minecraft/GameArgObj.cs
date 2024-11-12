

using Newtonsoft.Json;

namespace ColorMC.Core.Objs.Minecraft;

/// <summary>
/// 游戏数据
/// </summary>
public record GameArgObj
{
    public record ArgumentsObj
    {
        public record Jvm
        {
            public List<LibrariesObj.RulesObj> rules { get; set; }
            public object value { get; set; }
        }
        public List<object> game { get; set; }
        public List<object> jvm { get; set; }
    }
    public record AssetIndexObj
    {
        public string id { get; set; }
        public string sha1 { get; set; }
        public long size { get; set; }
        public long totalSize { get; set; }
        public string url { get; set; }
    }
    public record DownloadsObj
    {
        public record Download
        {
            public string sha1 { get; set; }
            public long size { get; set; }
            public string url { get; set; }
        }
        public Download client { get; set; }
        public Download client_mappings { get; set; }
        public Download server { get; set; }
        public Download server_mappings { get; set; }
    }
    public record JavaVersionObj
    {
        public string component { get; set; }
        public int majorVersion { get; set; }
    }
    public record LibrariesObj
    {
        public record RulesObj
        {
            public record OsObj
            {
                [JsonProperty("name")]
                public string Name { get; set; }
                //public string version { get; set; }
                [JsonProperty("arch")]
                public string Arch { get; set; }
            }
            [JsonProperty("action")]
            public string Action { get; set; }
            [JsonProperty("os")]
            public OsObj Os { get; set; }
        }
        public record DownloadsObj
        {
            public record ArtifactObj
            {
                [JsonProperty("path")]
                public string Path { get; set; }
                [JsonProperty("sha1")]
                public string Sha1 { get; set; }
                //public long size { get; set; }
                [JsonProperty("url")]
                public string Url { get; set; }
            }
            public record ClassifiersObj
            {
                [JsonProperty("natives-linux")]
                public ArtifactObj NativesLinux { get; set; }
                [JsonProperty("natives-osx")]
                public ArtifactObj NativesOsx { get; set; }
                [JsonProperty("natives-windows")]
                public ArtifactObj NativesWindows { get; set; }
                [JsonProperty("natives-windows-32")]
                public ArtifactObj NativesWindows32 { get; set; }
                [JsonProperty("natives-windows-64")]
                public ArtifactObj NativesWindows64 { get; set; }
            }
            [JsonProperty("classifiers")]
            public ClassifiersObj Classifiers { get; set; }
            [JsonProperty("artifact")]
            public ArtifactObj Artifact { get; set; }
        }
        //public record Natives
        //{
        //    public string linux { get; set; }
        //    public string osx { get; set; }
        //    public string windows { get; set; }
        //}
        //public Natives natives { get; set; }
        [JsonProperty("downloads")]
        public DownloadsObj Downloads { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("rules")]
        public List<RulesObj> Rules { get; set; }
    }
    public record LoggingObj
    {
        public record ClientObj
        {
            public record FileObj
            {
                //public string id { get; set; }
                [JsonProperty("sha1")]
                public string Sha1 { get; set; }
                //public long size { get; set; }
                [JsonProperty("url")]
                public string Url { get; set; }
            }
            [JsonProperty("argument")]
            public string Argument { get; set; }
            [JsonProperty("file")]
            public FileObj File { get; set; }
            //public string type { get; set; }
        }
        [JsonProperty("client")]
        public ClientObj Client { get; set; }
    }

    [JsonProperty("assetIndex")]
    public AssetIndexObj AssetIndex { get; set; }
    [JsonProperty("assets")]
    public string Assets { get; set; }
    //public int complianceLevel { get; set; }
    [JsonProperty("downloads")]
    public DownloadsObj Downloads { get; set; }
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("javaVersion")]
    public JavaVersionObj JavaVersion { get; set; }
    [JsonProperty("libraries")]
    public List<LibrariesObj> Libraries { get; set; }
    [JsonProperty("logging")]
    public LoggingObj Logging { get; set; }
    [JsonProperty("mainClass")]
    public string MainClass { get; set; }
    [JsonProperty("minecraftArguments")]
    public string MinecraftArguments { get; set; }
    [JsonProperty("minimumLauncherVersion")]
    public int MinimumLauncherVersion { get; set; }
    //public string releaseTime { get; set; }
    //public string time { get; set; }
    //public string type { get; set; }
    [JsonProperty("arguments")]
    public ArgumentsObj Arguments { get; set; }
}
