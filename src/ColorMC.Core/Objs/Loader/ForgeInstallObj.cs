namespace ColorMC.Core.Objs.Loader;

/// <summary>
/// Forge安装数据
/// </summary>
public record ForgeInstallObj
{
    public record Data
    {
        public record DataObj
        {
            public string client { get; set; }
            public string server { get; set; }
        }

        public DataObj MAPPINGS { get; set; }
        public DataObj MOJMAPS { get; set; }
        public DataObj MERGED_MAPPINGS { get; set; }
        public DataObj BINPATCH { get; set; }
        public DataObj MC_UNPACKED { get; set; }
        public DataObj MC_SLIM { get; set; }
        public DataObj MC_SLIM_SHA { get; set; }
        public DataObj MC_EXTRA { get; set; }
        public DataObj MC_EXTRA_SHA { get; set; }
        public DataObj MC_SRG { get; set; }
        public DataObj PATCHED { get; set; }
        public DataObj PATCHED_SHA { get; set; }
        public DataObj MCP_VERSION { get; set; }
    }

    public record ProcessorsItem
    {
        public List<string> sides { get; set; }
        public string jar { get; set; }
        public List<string> classpath { get; set; }
        public List<string> args { get; set; }
    }

    public List<string> _comment_ { get; set; }
    public int spec { get; set; }
    public string profile { get; set; }
    public string version { get; set; }
    public string path { get; set; }
    public string minecraft { get; set; }
    public string serverJarPath { get; set; }
    public Data data { get; set; }
    public List<ProcessorsItem> processors { get; set; }
    public List<ForgeLaunchObj.Libraries> libraries { get; set; }
    public string icon { get; set; }
    public string json { get; set; }
    public string logo { get; set; }
    public string mirrorList { get; set; }
    public string welcome { get; set; }
}


public record ForgeInstallObj1
{
    public record Install
    {
        public string profileName { get; set; }
        public string target { get; set; }
        public string path { get; set; }
        public string version { get; set; }
        public string filePath { get; set; }
        public string welcome { get; set; }
        public string minecraft { get; set; }
        public string mirrorList { get; set; }
        public string logo { get; set; }
    }
    public record VersionInfo
    {
        public record Libraries
        {
            public string name { get; set; }
            public string url { get; set; }
            public bool serverreq { get; set; }
            public bool clientreq { get; set; }
            public List<string> checksums { get; set; }
        }
        public string id { get; set; }
        public string time { get; set; }
        public string releaseTime { get; set; }
        public string type { get; set; }
        public string minecraftArguments { get; set; }
        public string mainClass { get; set; }
        public int minimumLauncherVersion { get; set; }
        public string assets { get; set; }
        public string inheritsFrom { get; set; }
        public string jar { get; set; }

        public List<Libraries> libraries { get; set; }
    }

    public Install install { get; set; }
    public VersionInfo versionInfo { get; set; }
}