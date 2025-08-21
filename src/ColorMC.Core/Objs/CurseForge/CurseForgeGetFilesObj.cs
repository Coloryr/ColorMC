using System.Text.Json.Serialization;

namespace ColorMC.Core.Objs.CurseForge;

public record CurseForgeGetFilesObj
{
    [JsonPropertyName("fileIds")]
    public List<long> FileIds { get; set; }
}

public record CurseForgeGetFilesResObj
{
    [JsonPropertyName("data")]
    public List<CurseForgeModObj.CurseForgeDataObj> Data { get; set; }
}
