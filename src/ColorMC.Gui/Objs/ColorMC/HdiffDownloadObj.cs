using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ColorMC.Gui.Objs.ColorMC;

public record HdiffDownloadObj
{
    [JsonPropertyName("linux_arm64")]
    public string LinuxArm64 { get; set; }
    [JsonPropertyName("linux_arm64")]
    public string LinuxAmd64 { get; set; }
    [JsonPropertyName("linux_arm64")]
    public string Macos { get; set; }
    [JsonPropertyName("linux_arm64")]
    public string WindowsArm64 { get; set; }
    [JsonPropertyName("linux_arm64")]
    public string WindowsAmd64 { get; set; }
}
