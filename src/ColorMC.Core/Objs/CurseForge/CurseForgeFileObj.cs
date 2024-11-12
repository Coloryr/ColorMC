using Newtonsoft.Json;

namespace ColorMC.Core.Objs.CurseForge;

/// <summary>
/// CF文件列表
/// </summary>
public record CurseForgeFileObj
{
    [JsonProperty("data")]
    public List<CurseForgeModObj.DataObj> Data { get; set; }
}
