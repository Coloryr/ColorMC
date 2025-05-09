using System.Text.Json.Serialization;

namespace ColorMC.Gui.Objs.Frp;

public record FrpDownloadObj
{
    [JsonPropertyName("linux_arm64")]
    public string LinuxArm64 { get; set; }
    [JsonPropertyName("linux_amd64")]
    public string LinuxAmd64 { get; set; }
    [JsonPropertyName("darwin_amd64")]
    public string DarwinAmd64 { get; set; }
    [JsonPropertyName("darwin_arm64")]
    public string DarwinArm64 { get; set; }
    [JsonPropertyName("windows_arm64")]
    public string WindowsArm64 { get; set; }
    [JsonPropertyName("windows_amd64")]
    public string WindowsAmd64 { get; set; }
}
