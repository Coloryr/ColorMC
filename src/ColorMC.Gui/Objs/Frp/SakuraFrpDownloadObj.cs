using System.Text.Json.Serialization;

namespace ColorMC.Gui.Objs.Frp;

public record SakuraFrpDownloadObj
{
    public record SakuraFrpDownloadItemObj
    {
        public record Arch
        {
            public record ArchItem
            {
                [JsonPropertyName("title")]
                public string Title { get; set; }
                [JsonPropertyName("url")]
                public string Url { get; set; }
                [JsonPropertyName("hash")]
                public string Hash { get; set; }
                //public long size { get; set; }
            }
            [JsonPropertyName("windows_amd64")]
            public ArchItem WindowsAmd64 { get; set; }
            [JsonPropertyName("windows_arm64")]
            public ArchItem WindowsArm64 { get; set; }
            [JsonPropertyName("linux_amd64")]
            public ArchItem LinuxAmd64 { get; set; }
            [JsonPropertyName("linux_arm64")]
            public ArchItem LinuxArm64 { get; set; }
            [JsonPropertyName("darwin_amd64")]
            public ArchItem DarwinAmd64 { get; set; }
            [JsonPropertyName("darwin_arm64")]
            public ArchItem DarwinArm64 { get; set; }
        }
        [JsonPropertyName("ver")]
        public string Ver { get; set; }
        //public long time { get; set; }
        //public string note { get; set; }
        [JsonPropertyName("archs")]
        public Arch Archs { get; set; }
    }
    [JsonPropertyName("frpc")]
    public SakuraFrpDownloadItemObj Frpc { get; set; }
}
